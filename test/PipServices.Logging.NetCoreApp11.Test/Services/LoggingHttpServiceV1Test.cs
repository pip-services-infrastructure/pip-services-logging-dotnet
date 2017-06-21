using Moq;

using PipServices.Commons.Config;
using PipServices.Commons.Convert;
using PipServices.Commons.Log;
using PipServices.Commons.Refer;
using PipServices.Logging.Logic;
using PipServices.Logging.Logic.Commands;
using PipServices.Logging.Models;
using PipServices.Logging.Persistence;
using PipServices.Logging.Services;

using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace PipServices.Logging.Test.Services
{
    public class LoggingHttpServiceV1Test : AbstractTest
    {
        private LoggingHttpServiceV1 _service;

        private ILoggingController _loggingController;
        private ILoggingPersistence _loggingPersistence;

        private Mock<ILoggingController> _moqLoggingController;
        private Mock<ILoggingPersistence> _moqLoggingPersistence;

        ConfigParams _restConfig = ConfigParams.FromTuples(
            "connection.protocol", "http",
            "connection.host", "localhost",
            "connection.port", 3001 // another port for this test!
        );

        private TestModel Model { get; set; }

        protected override void Initialize()
        {
            Model = new TestModel();

            _moqLoggingController = new Mock<ILoggingController>();
            _loggingController = _moqLoggingController.Object;

            _moqLoggingController.Setup(c => c.GetCommandSet()).Returns(new LoggingCommandSet(_loggingController));

            _moqLoggingPersistence = new Mock<ILoggingPersistence>();
            _loggingPersistence = _moqLoggingPersistence.Object;

            _service = new LoggingHttpServiceV1();
            _service.Configure(_restConfig);

            var references = References.FromTuples(
                new Descriptor("pip-services-logging", "persistence", "memory", "default", "1.0"), _loggingPersistence,
                new Descriptor("pip-services-logging", "controller", "default", "default", "1.0"), _loggingController,
                new Descriptor("pip-services-logging", "service", "http", "default", "1.0"), _service
            );
            _service.SetReferences(references);

            Task.Run(() => _service.OpenAsync(Model.CorrelationId));
            Thread.Sleep(1000); // Just let service a sec to be initialized
        }

        protected override void Uninitialize()
        {
            _service.CloseAsync(null);
        }

        [Fact] // Just ONE test to avoid issues with re-opening service on the same host
        public void It_Should_Perform_CRUD_Operations()
        {
            It_Should_Be_Opened();

            It_Should_Clear_Async();

            It_Should_Write_Message_Async();

            It_Should_Write_Messages_Async();

            It_Should_Read_Messages_Async();

            It_Should_Read_Errors_Async();
        }

        public void It_Should_Be_Opened()
        {
            Assert.True(_service.IsOpened());
        }

        public void It_Should_Clear_Async()
        {
            var clearCalled = false;
            _moqLoggingController.Setup(c => c.ClearAsync(Model.CorrelationId))
                .Callback(() => clearCalled = true);

            SendPostRequest("clear", new
            {
                correlation_id = Model.CorrelationId
            });

            Assert.True(clearCalled);
        }

        public void It_Should_Write_Message_Async()
        {
            var createCalled = false;
            _moqLoggingController.Setup(c => c.WriteMessageAsync(Model.CorrelationId, Model.SampleMessage1))
                .Callback(() => createCalled = true);

            SendPostRequest("write_message", new
            {
                correlation_id = Model.CorrelationId,
                message = Model.SampleMessage1
            });

            Assert.True(createCalled);
        }

        public void It_Should_Write_Messages_Async()
        {
            var createCalled = false;
            _moqLoggingController.Setup(c => c.WriteMessagesAsync(Model.CorrelationId, new LogMessageV1[]
                {
                    Model.SampleMessage1, Model.SampleMessage2, Model.SampleMessage3, Model.SampleErrorMessage1
                }))
                .Callback(() => createCalled = true);

            SendPostRequest("write_messages", new
            {
                correlation_id = Model.CorrelationId,
                messages = new LogMessageV1[]
                {
                    Model.SampleMessage1, Model.SampleMessage2, Model.SampleMessage3, Model.SampleErrorMessage1
                }
            });

            Assert.True(createCalled);
        }

        public void It_Should_Read_Messages_Async()
        {
            var initialLogMessages = new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 };
            _moqLoggingController.Setup(c => c.ReadMessagesAsync(Model.CorrelationId, Model.FilterParams, Model.PagingParams))
                .Returns(Task.FromResult(initialLogMessages));

            var resultContent = SendPostRequest("read_messages", new
            {
                correlation_id = Model.CorrelationId,
                filter = Model.FilterParams,
                paging = Model.PagingParams
            });

            var resultLogMessages = JsonConverter.FromJson<LogMessageV1[]>(resultContent);

            Assert.Equal(initialLogMessages.Length, resultLogMessages.Length);
        }

        public void It_Should_Read_Errors_Async()
        {
            var initialLogMessages = new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 };
            var initialErrorMessages = initialLogMessages.Where(m => m.Level <= LogLevel.Error).ToArray();

            _moqLoggingController.Setup(c => c.ReadErrorsAsync(Model.CorrelationId, Model.FilterParams, Model.PagingParams))
                .Returns(Task.FromResult(initialErrorMessages));

            var resultContent = SendPostRequest("read_errors", new
            {
                correlation_id = Model.CorrelationId,
                filter = Model.FilterParams,
                paging = Model.PagingParams
            });

            var resultLogMessages = JsonConverter.FromJson<LogMessageV1[]>(resultContent);

            Assert.Equal(initialErrorMessages.Length, resultLogMessages.Length);
        }

        private static string SendPostRequest(string route, dynamic request)
        {
            using (var httpClient = new HttpClient())
            {
                using (var content = new StringContent(JsonConverter.ToJson(request), Encoding.UTF8, "application/json"))
                {
                    var response = httpClient.PostAsync("http://localhost:3001/logging/" + route, content).Result;

                    return response.Content.ReadAsStringAsync().Result;
                }
            }
        }
    }
}
