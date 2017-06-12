using PipServices.Commons.Refer;
using PipServices.Logging.Logic;
using PipServices.Logging.Persistence;
using PipServices.Commons.Log;
using PipServices.Logging.Clients;
using PipServices.Logging.Models;
using PipServices.Logging.Services;
using PipServices.Commons.Config;
using PipServices.Logging.Logic.Commands;

using Xunit;

using Moq;

using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace PipServices.Logging.NetCoreApp11.Test.Clients
{
    public class LoggingHttpClientV1Test : AbstractTest
    {
        private LoggingHttpClientV1 _loggingHttpClientV1;

        private ILoggingController _loggingController;
        private ILoggingPersistence _loggingPersistence;

        private Mock<ILoggingController> _moqLoggingController;
        private Mock<ILoggingPersistence> _moqLoggingPersistence;

        private LoggingHttpServiceV1 _service;

        ConfigParams _restConfig = ConfigParams.FromTuples(
            "connection.protocol", "http",
            "connection.host", "localhost",
            "connection.port", 3000
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
                new Descriptor("pip-services-logging", "service", "http", "default", "1.0"), _service);

            _service.SetReferences(references);

            Task.Run(() => _service.OpenAsync(Model.CorrelationId));
            Thread.Sleep(1000); // Just let service a sec to be initialized

            _loggingHttpClientV1 = new LoggingHttpClientV1();
            _loggingHttpClientV1.Configure(_restConfig);
            _loggingHttpClientV1.SetReferences(references);

            var clientTask = _loggingHttpClientV1.OpenAsync(Model.CorrelationId);
            clientTask.Wait();
        }

        protected override void Uninitialize()
        {
            _service.CloseAsync(Model.CorrelationId);
            _loggingHttpClientV1.CloseAsync(Model.CorrelationId);
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
            Assert.True(_loggingHttpClientV1.IsOpened());
        }

        public void It_Should_Clear_Async()
        {
            var clearCalled = false;
            _moqLoggingController.Setup(c => c.ClearAsync(Model.CorrelationId))
                .Callback(() => clearCalled = true);

            _loggingHttpClientV1.ClearAsync(Model.CorrelationId).Wait();

            Assert.True(clearCalled);
        }

        public void It_Should_Write_Message_Async()
        {
            var createCalled = false;
            _moqLoggingController.Setup(c => c.WriteMessageAsync(Model.CorrelationId, Model.SampleMessage1))
                .Callback(() => createCalled = true);

            _loggingHttpClientV1.WriteMessageAsync(Model.CorrelationId, Model.SampleMessage1).Wait();

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

            _loggingHttpClientV1.WriteMessagesAsync(Model.CorrelationId, new LogMessageV1[]
                {
                    Model.SampleMessage1, Model.SampleMessage2, Model.SampleMessage3, Model.SampleErrorMessage1
                }).Wait();

            Assert.True(createCalled);
        }

        public void It_Should_Read_Messages_Async()
        {
            var initialLogMessages = new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 };
            _moqLoggingController.Setup(c => c.ReadMessagesAsync(Model.CorrelationId, Model.FilterParams, Model.PagingParams))
                .Returns(Task.FromResult(initialLogMessages));

            var resultLogMessages = _loggingHttpClientV1.ReadMessagesAsync(Model.CorrelationId, Model.FilterParams, Model.PagingParams).Result;
            Assert.Equal(initialLogMessages.Length, resultLogMessages.Length);
        }

        public void It_Should_Read_Errors_Async()
        {
            var initialLogMessages = new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 };
            var initialErrorMessages = initialLogMessages.Where(m => m.Level <= LogLevel.Error).ToArray();

            _moqLoggingController.Setup(c => c.ReadErrorsAsync(Model.CorrelationId, Model.FilterParams, Model.PagingParams))
                .Returns(Task.FromResult(initialErrorMessages));

            var resultLogMessages = _loggingHttpClientV1.ReadErrorsAsync(Model.CorrelationId, Model.FilterParams, Model.PagingParams).Result;

            Assert.Equal(initialErrorMessages.Length, resultLogMessages.Length);
        }
    }
}