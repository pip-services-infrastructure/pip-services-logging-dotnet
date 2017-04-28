using PipServices.Commons.Refer;
using PipServices.Logging.Logic;
using PipServices.Logging.Persistence;
using PipServices.Commons.Log;
using PipServices.Logging.Clients;
using PipServices.Logging.Models;

using Xunit;

using Moq;

using System.Threading.Tasks;
using System.Linq;

namespace PipServices.Logging.NetCoreApp11.Test.Clients
{
    public class LoggingDirectClientV1Test : AbstractTest
    {
        private LoggingDirectClientV1 _loggingDirectClient;

        private ILoggingBusinessLogic _loggingController;
        private ILoggingPersistence _loggingPersistence;

        private Mock<ILoggingBusinessLogic> _moqLoggingController;
        private Mock<ILoggingPersistence> _moqLoggingPersistence;

        private ConsoleLogger _consoleLogger;

        private TestModel Model { get; set; }

        protected override void Initialize()
        {
            Model = new TestModel();

            _moqLoggingController = new Mock<ILoggingBusinessLogic>();
            _loggingController = _moqLoggingController.Object;

            _moqLoggingPersistence = new Mock<ILoggingPersistence>();
            _loggingPersistence = _moqLoggingPersistence.Object;

            _consoleLogger = new ConsoleLogger();

            var references = new References();
            references.Put(new Descriptor("pip-services-commons", "logger", "console", "default", "1.0"), _consoleLogger);
            references.Put(new Descriptor("pip-services-logging", "persistence", "memory", "default", "1.0"), _loggingPersistence);
            references.Put(new Descriptor("pip-services-logging", "controller", "default", "default", "1.0"), _loggingController);

            _loggingDirectClient = new LoggingDirectClientV1();
            _loggingDirectClient.SetReferences(references);

            _loggingDirectClient.OpenAsync(Model.CorrelationId);
        }

        protected override void Uninitialize()
        {
            _loggingDirectClient.CloseAsync(Model.CorrelationId);
        }

        [Fact]
        public void It_Should_Be_Opened()
        {
            Assert.True(_loggingDirectClient.IsOpened());
        }

        [Fact]
        public void It_Should_Clear_Async()
        {
            var clearCalled = false;
            _moqLoggingController.Setup(c => c.ClearAsync(Model.CorrelationId))
                .Callback(() => clearCalled = true);

            _loggingDirectClient.ClearAsync(Model.CorrelationId);

            Assert.True(clearCalled);
        }

        [Fact]
        public void It_Should_Write_Message_Async()
        {
            var createCalled = false;
            _moqLoggingController.Setup(c => c.WriteMessageAsync(Model.CorrelationId, Model.SampleMessage1))
                .Callback(() => createCalled = true);

            _loggingDirectClient.WriteMessageAsync(Model.CorrelationId, Model.SampleMessage1);

            Assert.True(createCalled);
        }

        [Fact]
        public void It_Should_Read_Messages_Async()
        {
            var initialLogMessages = new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 };
            _moqLoggingController.Setup(c => c.ReadMessagesAsync(Model.CorrelationId, null, null))
                .Returns(Task.FromResult(initialLogMessages));

            var resultLogMessages = _loggingDirectClient.ReadMessagesAsync(Model.CorrelationId, null, null).Result;
            Assert.Equal(initialLogMessages.Length, resultLogMessages.Length);
        }

        [Fact]
        public void It_Should_Read_Errors_Async()
        {
            var initialLogMessages = new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 };
            var initialErrorMessages = initialLogMessages.Where(m => m.Level <= LogLevel.Error).ToArray();

            _moqLoggingController.Setup(c => c.ReadErrorsAsync(Model.CorrelationId, null, null))
                .Returns(Task.FromResult(initialErrorMessages));

            var resultLogMessages = _loggingController.ReadErrorsAsync(Model.CorrelationId, null, null).Result;

            Assert.Equal(initialErrorMessages.Length, resultLogMessages.Length);
        }

    }
}