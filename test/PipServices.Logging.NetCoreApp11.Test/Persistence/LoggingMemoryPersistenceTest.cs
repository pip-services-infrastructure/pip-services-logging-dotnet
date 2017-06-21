using System;

using Xunit;

using PipServices.Logging.Persistence;
using PipServices.Commons.Config;
using PipServices.Logging.Models;
using PipServices.Commons.Log;
using PipServices.Commons.Errors;
using PipServices.Commons.Data;

namespace PipServices.Logging.Test.Persistence
{

    public class LoggingMemoryPersistenceTest : AbstractTest
    {
        private TestModel Model { get; set; }

        protected override void Initialize()
        {
            Model = new TestModel();
        }

        protected override void Uninitialize()
        {
        }

        [Fact]
        public void It_Should_Configure_Parameters()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();

            loggingMemoryPersistence.Configure(Model.ConfigParams);

            Assert.Equal(Model.MaxPageSize, loggingMemoryPersistence.MaxPageSize);
            Assert.Equal(Model.MaxErrorSize, loggingMemoryPersistence.MaxErrorSize);
            Assert.Equal(Model.MaxTotalSize, loggingMemoryPersistence.MaxTotalSize);
        }

        [Fact]
        public void It_Should_Clear_Async()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();

            loggingMemoryPersistence.CreateAsync(Model.CorrelationId, Model.SampleMessage1).Wait();
            loggingMemoryPersistence.CreateAsync(Model.CorrelationId, Model.SampleErrorMessage1).Wait();

            Assert.NotEmpty(loggingMemoryPersistence.Messages);
            Assert.NotEmpty(loggingMemoryPersistence.ErrorMessages);

            loggingMemoryPersistence.ClearAsync(Model.CorrelationId).Wait();

            Assert.Empty(loggingMemoryPersistence.Messages);
            Assert.Empty(loggingMemoryPersistence.ErrorMessages);
        }

        [Fact]
        public void It_Should_Create_Async()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();

            loggingMemoryPersistence.CreateAsync(Model.CorrelationId, Model.SampleMessage1).Wait();
            loggingMemoryPersistence.CreateAsync(Model.CorrelationId, Model.SampleErrorMessage1).Wait();

            Assert.Equal(2, loggingMemoryPersistence.Messages.Count);
            Assert.Equal(1, loggingMemoryPersistence.ErrorMessages.Count);
        }

        [Fact]
        public void It_Should_Not_Exceed_Maximum_Size_When_Create_Async()
        {
            Model.MaxErrorSize = 2;
            Model.MaxTotalSize = 3;

            Model.ConfigParams = new ConfigParams
            {
                { "options.max_error_size", Model.MaxErrorSize.ToString() },
                { "options.max_total_size", Model.MaxTotalSize.ToString() }
            };

            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(Model.ConfigParams);

            CreateTestLogMessages(loggingMemoryPersistence);

            Assert.Equal(Model.MaxTotalSize, loggingMemoryPersistence.Messages.Count);
            Assert.Equal(Model.MaxErrorSize, loggingMemoryPersistence.ErrorMessages.Count);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_Search_Filter()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(Model.ConfigParams);

            var filter = new FilterParams
            {
                { "search", "test" }
            };

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(Model.CorrelationId, filter, null).Result;

            Assert.Equal(loggingMemoryPersistence.Messages.Count, result.Length);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_Level_Filter()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(Model.ConfigParams);

            var filter = new FilterParams
            {
                { "level", ((int)LogLevel.Fatal).ToString() }
            };

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(Model.CorrelationId, filter, null).Result;

            // 2 fatals
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_Max_Level_Filter()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(Model.ConfigParams);

            var filter = new FilterParams
            {
                { "max_level", ((int)LogLevel.Error).ToString() }
            };

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(Model.CorrelationId, filter, null).Result;

            // 2 fatals and 1 error
            Assert.Equal(3, result.Length);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_DateTime_Filter()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(Model.ConfigParams);

            var filter = new FilterParams
            {
                { "from_time", Model.FourDaysAgo.ToString() },
                { "to_time", Model.TwoDaysAgo.ToString() }
            };

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(Model.CorrelationId, filter, null).Result;

            // Just 2 x 3 days ago messages
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_Errors_Only_Filter()
        {
            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(Model.ConfigParams);

            var filter = new FilterParams
            {
                { "errors_only", true.ToString() }
            };

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(Model.CorrelationId, filter, null).Result;

            // 3 error messages
            Assert.Equal(3, result.Length);
        }

        [Fact]
        public void It_Should_Get_Page_Async_By_Paging()
        {
            Model.MaxPageSize = 2;

            Model.ConfigParams = new ConfigParams
            {
                { "options.max_page_size", Model.MaxPageSize.ToString() }
            };

            var loggingMemoryPersistence = new LoggingMemoryPersistence();
            loggingMemoryPersistence.Configure(Model.ConfigParams);

            var paging = new PagingParams();

            CreateTestLogMessages(loggingMemoryPersistence);

            var result = loggingMemoryPersistence.GetPageByFilterAsync(Model.CorrelationId, null, paging).Result;

            // Take only 2 
            Assert.Equal(Model.MaxPageSize, result.Length);
        }

        private void CreateTestLogMessages(ILoggingPersistence loggingMemoryPersistence)
        {
            loggingMemoryPersistence.CreateAsync(Model.CorrelationId, Model.SampleMessage1).Wait();
            loggingMemoryPersistence.CreateAsync(Model.CorrelationId, Model.SampleMessage2).Wait();
            loggingMemoryPersistence.CreateAsync(Model.CorrelationId, Model.SampleMessage3).Wait();

            loggingMemoryPersistence.CreateAsync(Model.CorrelationId, Model.SampleErrorMessage1).Wait();
            loggingMemoryPersistence.CreateAsync(Model.CorrelationId, Model.SampleErrorMessage2).Wait();
            loggingMemoryPersistence.CreateAsync(Model.CorrelationId, Model.SampleErrorMessage3).Wait();
        }
    }
}
