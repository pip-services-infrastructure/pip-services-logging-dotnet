using System.Threading.Tasks;
using System.Collections.Generic;

using PipServices.Commons.Config;
using PipServices.Commons.Data;
using PipServices.Logging.Models;
using PipServices.Commons.Log;

namespace PipServices.Logging.Persistence
{
    public class LoggingMemoryPersistence : ILoggingPersistence
    {
        public int MaxPageSize { get; private set; }
        public int MaxErrorSize { get; private set; }
        public int MaxTotalSize { get; private set; }

        public List<LogMessageV1> Messages { get; private set; }
        public List<LogMessageV1> ErrorMessages { get; private set; }

        public LoggingMemoryPersistence()
        {
            MaxPageSize = 100;
            MaxErrorSize = 1000;
            MaxTotalSize = 10000;

            Messages = new List<LogMessageV1>();
            ErrorMessages = new List<LogMessageV1>();
        }

        public void Configure(ConfigParams config)
        {
            MaxPageSize = config.GetAsIntegerWithDefault("options.max_page_size", MaxPageSize);
            MaxErrorSize = config.GetAsIntegerWithDefault("options.max_error_size", MaxErrorSize);
            MaxTotalSize = config.GetAsIntegerWithDefault("options.max_total_size", MaxTotalSize);
        }

        public Task ClearAsync(string correlationId)
        {
            return Task.Run( () => 
            {
                Messages = new List<LogMessageV1>();
                ErrorMessages = new List<LogMessageV1>();
            });
        }

        public Task CreateAsync(string correlationId, LogMessageV1 message)
        {
            return Task.Run(() =>
            {
                // Add to all messages
                TruncateMessages(Messages, MaxTotalSize);
                InsertMessage(message, Messages);

                // Add to errors separately
                if (message.Level <= LogLevel.Error)
                {
                    TruncateMessages(ErrorMessages, MaxErrorSize);
                    InsertMessage(message, ErrorMessages);
                }
            });
        }

        public Task<LogMessageV1[]> GetPageByFilterAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            filter = filter ?? new FilterParams();

            var search = filter.GetAsNullableString("search");
            var level = filter.GetAsNullableInteger("level");
            var maxLevel = filter.GetAsNullableInteger("max_level");
            var fromTime = filter.GetAsNullableDateTime("from_time");
            var toTime = filter.GetAsNullableDateTime("to_time");
            var errorsOnly = filter.GetAsBooleanWithDefault("errors_only", false);

            paging = paging ?? new PagingParams();
            var skip = paging.GetSkip(0);
            var take = paging.GetTake(MaxPageSize);
            var data = new List<LogMessageV1>();

            var messages = errorsOnly ? ErrorMessages : Messages;
            for (var index = 0; index < messages.Count; index++)
            {
                var message = messages[index];
                if (search != null && !MessageContains(message, search))
                {
                    continue;
                }

                if (level != null && level != (int)message.Level)
                {
                    continue;
                }

                if (maxLevel != null && maxLevel < (int)message.Level)
                {
                    continue;
                }

                if (fromTime != null && fromTime > message.Time)
                {
                    continue;
                }

                if (toTime != null && toTime <= message.Time)
                {
                    continue;
                }

                skip--;
                if (skip >= 0)
                {
                    continue;
                }

                data.Insert(0, message);

                take--;
                if (take <= 0)
                {
                    break;
                }
            }

            return Task.FromResult(data.ToArray());
        }

        private bool MessageContains(LogMessageV1 message, string search)
        {
            if (MatchString(message.Message, search))
            {
                return true;
            }

            if (MatchString(message.Correlation_Id, search))
            {
                return true;
            }

            if (message.Error != null)
            {
                if (MatchString(message.Error.Message, search))
                {
                    return true;
                }

                if (MatchString(message.Error.StackTrace, search))
                {
                    return true;
                }

                if (MatchString(message.Error.Code, search))
                {
                    return true;
                }
            }

            return false;
        }

        private bool MatchString(string value, string search)
        {
            if (value == null && search == null)
            {
                return true;
            }

            if (value == null || search == null)
            {
                return false;
            }

            return value.ToLower().IndexOf(search) >= 0;
        }

        private void InsertMessage(LogMessageV1 message, IList<LogMessageV1> messages)
        {
            var index = 0;

            // Find index to keep messages sorted by time
            while (index < messages.Count)
            {
                if (message.Time >= messages[index].Time)
                {
                    break;
                }

                index++;
            }

            messages.Insert(index < messages.Count ? index : 0, message);
        }

        private void TruncateMessages(List<LogMessageV1> messages, int maxTotalSize)
        {
            if (messages.Count >= maxTotalSize)
            {
                messages.RemoveRange(maxTotalSize - 1, messages.Count - maxTotalSize + 1);
            }
        }
    }
}
