using PipServices.Commons.Errors;
using PipServices.Commons.Log;
using PipServices.Logging.Models;

using System;

using Xunit;

namespace PipServices.Logging.NetCoreApp11.Test.Models
{
    public class LogMessageV1Test
    {
        [Fact]
        public void It_Should_Create_Log_Message_V1()
        {
            LogLevel level = default(Commons.Log.LogLevel);
            string source = "Test Source";
            string correlationId = "Test Correlation ID";
            ErrorDescription error = new ErrorDescription();
            string message = "Test Message";

            var logMessage = new LogMessageV1(level, source, correlationId, error, message);

            Assert.Equal(level, logMessage.Level);
            Assert.Equal(source, logMessage.Source);
            Assert.Equal(correlationId, logMessage.CorrelationId);
            Assert.Equal(error, logMessage.Error);
            Assert.Equal(message, logMessage.Message);
        }
    }
}
