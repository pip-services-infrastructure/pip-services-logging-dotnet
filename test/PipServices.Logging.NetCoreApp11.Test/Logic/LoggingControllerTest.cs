using Xunit;

using Moq;

using PipServices.Commons.Refer;
using PipServices.Logging.Logic;
using PipServices.Logging.Persistence;
using PipServices.Logging.Models;
using PipServices.Commons.Data;
using PipServices.Commons.Log;

using System.Threading.Tasks;
using System.Linq;

namespace PipServices.Logging.Test.Logic
{
    public class LoggingControllerTest : AbstractTest
    {
        private LoggingController _loggingController;

        private ILoggingPersistence _loggingPersistence;
        private Mock<ILoggingPersistence> _moqLoggingPersistence;

        private TestModel Model { get; set; }

        protected override void Initialize()
        {
            Model = new TestModel();

            var references = new References();
            _loggingController = new LoggingController();

            _moqLoggingPersistence = new Mock<ILoggingPersistence>();
            _loggingPersistence = _moqLoggingPersistence.Object;

            references.Put(new Descriptor("pip-services-logging", "persistence", "memory", "default", "1.0"), _loggingPersistence);
            references.Put(new Descriptor("pip-services-logging", "controller", "default", "default", "1.0"), _loggingController);

            _loggingController.SetReferences(references);
        }

        protected override void Uninitialize()
        {
        }

        [Fact]
        public void It_Should_Clear_Async()
        {
            var clearCalled = false;
            _moqLoggingPersistence.Setup(p => p.ClearAsync(Model.CorrelationId)).Callback(() => clearCalled = true );

            _loggingController.ClearAsync(Model.CorrelationId);

            Assert.True(clearCalled);
        }

        [Fact]
        public void It_Should_Write_Message_Async()
        {
            var createCalled = false;
            _moqLoggingPersistence.Setup(p => p.CreateAsync(Model.CorrelationId, Model.SampleMessage1)).Callback(() => createCalled = true);

            _loggingController.WriteMessageAsync(Model.CorrelationId, Model.SampleMessage1);

            Assert.True(createCalled);
        }

        [Fact]
        public void It_Should_Write_Messages_Async()
        {
            var createMessageCalled = false;
            var createErrorCalled = false;

            _moqLoggingPersistence.Setup(p => p.CreateAsync(Model.CorrelationId, Model.SampleMessage1))
                .Callback(() => createMessageCalled = true)
                .Returns(Task.FromResult(true));
            _moqLoggingPersistence.Setup(p => p.CreateAsync(Model.CorrelationId, Model.SampleErrorMessage1))
                .Callback(() => createErrorCalled = true)
                .Returns(Task.FromResult(true));

            _loggingController.WriteMessagesAsync(Model.CorrelationId,
                new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 });

            Assert.True(createMessageCalled);
            Assert.True(createErrorCalled);
        }

        [Fact]
        public void It_Should_Read_Messages_Async()
        {
            var initialLogMessages = new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 };
            _moqLoggingPersistence.Setup(p => p.GetPageByFilterAsync(Model.CorrelationId, null, null)).Returns(Task.FromResult(initialLogMessages));

            var resultLogMessages = _loggingController.ReadMessagesAsync(Model.CorrelationId, null, null).Result;
            Assert.Equal(initialLogMessages.Length, resultLogMessages.Length);
        }

        [Fact]
        public void It_Should_Read_Errors_Async()
        {
            var initialLogMessages = new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 };
            var initialErrorMessages = initialLogMessages.Where(m => m.Level <= LogLevel.Error).ToArray();

            var filter = new FilterParams();
            filter.SetAsObject("errors_only", true);

            _moqLoggingPersistence.Setup(p => p.GetPageByFilterAsync(Model.CorrelationId, filter, null)).Returns(Task.FromResult(initialErrorMessages));
            _moqLoggingPersistence.Setup(p => p.GetPageByFilterAsync(Model.CorrelationId, null, null)).Returns(Task.FromResult(initialLogMessages));

            var resultLogMessages = _loggingController.ReadErrorsAsync(Model.CorrelationId, null, null).Result;

            Assert.Equal(initialErrorMessages.Length, resultLogMessages.Length);
        }
    }
}