using Xunit;

using PipServices.Commons.Refer;
using PipServices.Logging.Logic;
using PipServices.Commons.Config;
using PipServices.Logging.Persistence;
using PipServices.Logging.Models;
using PipServices.Commons.Log;
using System.Collections.Generic;

namespace PipServices.Logging.NetCoreApp11.Test.Logic
{
    public class LoggingBusinessLogicTest : AbstractTest
    {
        private LoggingController _loggingController;
        private LoggingMemoryPersistence _loggingMemoryPersistence;

        private TestModel Model { get; set; }

        protected override void Initialize()
        {
            Model = new TestModel();

            var references = new References();
            _loggingController = new LoggingController();
            _loggingMemoryPersistence = new LoggingMemoryPersistence();

            references.Put(new Descriptor("pip-services-logging", "persistence", "memory", "default", "1.0"), _loggingMemoryPersistence);
            references.Put(new Descriptor("pip-services-logging", "controller", "default", "default", "1.0"), _loggingController);

            _loggingController.SetReferences(references);
        }

        protected override void Uninitialize()
        {
        }

        [Fact]
        public void It_Should_Initialize_Read_And_Write_Persistences()
        {
            Assert.NotNull(_loggingController.ReadPersistence);
            Assert.NotNull(_loggingController.WritePersistence);
        }

        [Fact]
        public void It_Should_Clear_Async()
        {
            _loggingController.WriteMessageAsync(Model.CorrelationId, Model.SampleMessage1).Wait();
            _loggingController.WriteMessageAsync(Model.CorrelationId, Model.SampleErrorMessage1).Wait();

            Assert.NotEmpty(_loggingController.ReadPersistence.Messages);
            Assert.NotEmpty(_loggingController.ReadPersistence.ErrorMessages);
            Assert.NotEmpty(_loggingController.WritePersistence.Messages);
            Assert.NotEmpty(_loggingController.WritePersistence.ErrorMessages);

            _loggingController.ClearAsync(Model.CorrelationId).Wait();

            Assert.Empty(_loggingController.ReadPersistence.Messages);
            Assert.Empty(_loggingController.ReadPersistence.ErrorMessages);
            Assert.Empty(_loggingController.WritePersistence.Messages);
            Assert.Empty(_loggingController.WritePersistence.ErrorMessages);
        }


        [Fact]
        public void It_Should_Write_Message_Async()
        {
            _loggingController.WriteMessageAsync(Model.CorrelationId, Model.SampleMessage1).Wait();
            _loggingController.WriteMessageAsync(Model.CorrelationId, Model.SampleErrorMessage1).Wait();

            Assert.Equal(2, _loggingController.ReadPersistence.Messages.Count);
            Assert.Equal(1, _loggingController.ReadPersistence.ErrorMessages.Count);
            Assert.Equal(2, _loggingController.WritePersistence.Messages.Count);
            Assert.Equal(1, _loggingController.WritePersistence.ErrorMessages.Count);
        }

        [Fact]
        public void It_Should_Write_Messages_Async()
        {
            _loggingController.WriteMessagesAsync(Model.CorrelationId, 
                new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 }).Wait();

            Assert.Equal(2, _loggingController.ReadPersistence.Messages.Count);
            Assert.Equal(1, _loggingController.ReadPersistence.ErrorMessages.Count);
            Assert.Equal(2, _loggingController.WritePersistence.Messages.Count);
            Assert.Equal(1, _loggingController.WritePersistence.ErrorMessages.Count);
        }

        [Fact]
        public void It_Should_Read_Messages_Async()
        {
            var initialLogMessages = new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 };
            _loggingController.WriteMessagesAsync(Model.CorrelationId, initialLogMessages).Wait();

            var resultLogMessages = _loggingController.ReadMessagesAsync(Model.CorrelationId, null, null).Result;

            Assert.Equal(initialLogMessages.Length, resultLogMessages.Length);
        }

        [Fact]
        public void It_Should_Read_Errors_Async()
        {
            var initialLogMessages = new LogMessageV1[] { Model.SampleMessage1, Model.SampleErrorMessage1 };
            _loggingController.WriteMessagesAsync(Model.CorrelationId, initialLogMessages).Wait();

            var resultLogMessages = _loggingController.ReadErrorsAsync(Model.CorrelationId, null, null).Result;

            Assert.Equal(1, resultLogMessages.Length);
        }
    }
}
