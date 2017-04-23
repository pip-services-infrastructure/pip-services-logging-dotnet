using System;

using Xunit;

using PipServices.Logging.Persistence;
using PipServices.Commons.Config;
using PipServices.Logging.Models;
using PipServices.Commons.Log;
using PipServices.Commons.Errors;
using PipServices.Commons.Data;

namespace PipServices.Logging.NetCoreApp11.Test.Persistence
{

    public class LoggingMemoryPersistenceTest : AbstractTest
    {
        private int _maxPageSize;
        private int _maxErrorSize;
        private int _maxTotalSize;
        private ConfigParams _configParams;

        private string _correlationId;

        private LogMessageV1 _sampleMessage1;
        private LogMessageV1 _sampleMessage2;
        private LogMessageV1 _sampleMessage3;

        private LogMessageV1 _sampleErrorMessage1;
        private LogMessageV1 _sampleErrorMessage2;
        private LogMessageV1 _sampleErrorMessage3;

        private DateTime _fiveDaysAgo = DateTime.UtcNow.AddDays(-5);
        private DateTime _fourDaysAgo = DateTime.UtcNow.AddDays(-4);
        private DateTime _threeDaysAgo = DateTime.UtcNow.AddDays(-3);
        private DateTime _twoDaysAgo = DateTime.UtcNow.AddDays(-2);
        private DateTime _oneDayAgo = DateTime.UtcNow.AddDays(-1);

        protected override void Initialize()
        {
            _maxPageSize = 20;
            _maxErrorSize = 50;
            _maxTotalSize = 100;

            _configParams = new ConfigParams
            {
                { "options.max_page_size", _maxPageSize.ToString() },
                { "options.max_error_size", _maxErrorSize.ToString() },
                { "options.max_total_size", _maxTotalSize.ToString() }
            };

            _correlationId = "1";

            _sampleMessage1 = new LogMessageV1(_fiveDaysAgo, LogLevel.Info, "Persistence", _correlationId, null, "Test Message #1");
            _sampleMessage2 = new LogMessageV1(_threeDaysAgo, LogLevel.Warn, "Persistence", _correlationId, null, "Test Message #2");
            _sampleMessage3 = new LogMessageV1(_oneDayAgo, LogLevel.Debug, "Persistence", _correlationId, null, "Test Message #3");

            _sampleErrorMessage1 = new LogMessageV1(_fiveDaysAgo, LogLevel.Error, "Persistence", _correlationId, new ErrorDescription() { Code = "911" }, "Test Error Message #1");
            _sampleErrorMessage2 = new LogMessageV1(_threeDaysAgo, LogLevel.Fatal, "Persistence", _correlationId, new ErrorDescription() { }, "Test Error Message #2");
            _sampleErrorMessage3 = new LogMessageV1(_oneDayAgo, LogLevel.Fatal, "Persistence", _correlationId, new ErrorDescription() { Cause = "Bad luck" }, "Test Error Message #3");
        }

        protected override void Uninitialize()
        {
        }

        [Fact]
        public void It_Should_Configure_Parameters()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();

            loggingMemoryPersistence.Configure(_configParams);

            Assert.Equal(_maxPageSize, loggingMemoryPersistence.MaxPageSize);
            Assert.Equal(_maxErrorSize, loggingMemoryPersistence.MaxErrorSize);
            Assert.Equal(_maxTotalSize, loggingMemoryPersistence.MaxTotalSize);
        }

        [Fact]
        public void It_Should_Clear_Async()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();

            loggingMemoryPersistence.CreateAsync(_correlationId, _sampleMessage1).Wait();
            loggingMemoryPersistence.CreateAsync(_correlationId, _sampleErrorMessage1).Wait();

            Assert.NotEmpty(loggingMemoryPersistence.Messages);
            Assert.NotEmpty(loggingMemoryPersistence.ErrorMessages);

            loggingMemoryPersistence.ClearAsync(_correlationId).Wait();

            Assert.Empty(loggingMemoryPersistence.Messages);
            Assert.Empty(loggingMemoryPersistence.ErrorMessages);
        }

        [Fact]
        public void It_Should_Create_Async()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();

            loggingMemoryPersistence.CreateAsync(_correlationId, _sampleMessage1).Wait();
            loggingMemoryPersistence.CreateAsync(_correlationId, _sampleErrorMessage1).Wait();

            Assert.Equal(2, loggingMemoryPersistence.Messages.Count);
            Assert.Equal(1, loggingMemoryPersistence.ErrorMessages.Count);
        }

        [Fact]
        public void It_Should_Not_Exceed_Maximum_Size_When_Create_Async()
        {
            _maxErrorSize = 2;
            _maxTotalSize = 3;

            _configParams = new ConfigParams
            {
                { "options.max_error_size", _maxErrorSize.ToString() },
                { "options.max_total_size", _maxTotalSize.ToString() }
            };

            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(_configParams);

            CreateTestLogMessages(loggingMemoryPersistence);

            Assert.Equal(_maxTotalSize, loggingMemoryPersistence.Messages.Count);
            Assert.Equal(_maxErrorSize, loggingMemoryPersistence.ErrorMessages.Count);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_Search_Filter()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(_configParams);

            var filter = new FilterParams
            {
                { "search", "test" }
            };

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(_correlationId, filter, null).Result;

            Assert.Equal(loggingMemoryPersistence.Messages.Count, result.Length);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_Level_Filter()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(_configParams);

            var filter = new FilterParams
            {
                { "level", ((int)LogLevel.Fatal).ToString() }
            };

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(_correlationId, filter, null).Result;

            // 2 fatals
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_Max_Level_Filter()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(_configParams);

            var filter = new FilterParams
            {
                { "max_level", ((int)LogLevel.Error).ToString() }
            };

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(_correlationId, filter, null).Result;

            // 2 fatals and 1 error
            Assert.Equal(3, result.Length);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_DateTime_Filter()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(_configParams);

            var filter = new FilterParams
            {
                { "from_time", _fourDaysAgo.ToString() },
                { "to_time", _twoDaysAgo.ToString() }
            };

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(_correlationId, filter, null).Result;

            // Just 2 x 3 days ago messages
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_Errors_Only_Filter()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(_configParams);

            var filter = new FilterParams
            {
                { "errors_only", true.ToString() }
            };

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(_correlationId, filter, null).Result;

            // 3 error messages
            Assert.Equal(3, result.Length);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_Paging()
        {
            _maxPageSize = 2;

            _configParams = new ConfigParams
            {
                { "options.max_page_size", _maxPageSize.ToString() }
            };

            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(_configParams);

            var paging = new PagingParams();

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(_correlationId, null, paging).Result;

            // Take only 2 
            Assert.Equal(_maxPageSize, result.Length);
        }

        private void CreateTestLogMessages(ILoggingPersistence loggingMemoryPersistence)
        {
            loggingMemoryPersistence.CreateAsync(_correlationId, _sampleMessage1).Wait();
            loggingMemoryPersistence.CreateAsync(_correlationId, _sampleMessage2).Wait();
            loggingMemoryPersistence.CreateAsync(_correlationId, _sampleMessage3).Wait();

            loggingMemoryPersistence.CreateAsync(_correlationId, _sampleErrorMessage1).Wait();
            loggingMemoryPersistence.CreateAsync(_correlationId, _sampleErrorMessage2).Wait();
            loggingMemoryPersistence.CreateAsync(_correlationId, _sampleErrorMessage3).Wait();
        }
    }
}
