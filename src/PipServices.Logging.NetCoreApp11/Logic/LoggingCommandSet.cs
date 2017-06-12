using System;
using System.Threading.Tasks;

using PipServices.Commons.Commands;
using PipServices.Commons.Run;
using PipServices.Commons.Validate;
using PipServices.Logging.Models;
using PipServices.Commons.Data;
using PipServices.Commons.Log;
using PipServices.Commons.Errors;
using System.Collections.Generic;

namespace PipServices.Logging.Logic.Commands
{
    public partial class LoggingCommandSet : CommandSet
    {
        private ILoggingController _loggingBusinessLogic;

        public LoggingCommandSet(ILoggingController loggingBusinessLogic)
        {
            _loggingBusinessLogic = loggingBusinessLogic;

            AddCommand(MakeReadMessagesCommand());
            AddCommand(MakeReadErrorsCommand());
            AddCommand(MakeWriteMessageCommand());
            AddCommand(MakeWriteMessagesCommand());
            AddCommand(MakeClearCommand());
        }

        private ICommand MakeReadMessagesCommand()
        {
            return new Command(
                "read_messages",
                new ObjectSchema()
                    .WithOptionalProperty("filter", new FilterParamsSchema())
                    .WithOptionalProperty("paging", new PagingParamsSchema()),
                ReadMessages);
        }

        private ICommand MakeReadErrorsCommand()
        {
            return new Command(
                "read_errors",
                new ObjectSchema()
                    .WithOptionalProperty("filter", new FilterParamsSchema())
                    .WithOptionalProperty("paging", new PagingParamsSchema()),
                ReadErrors);
        }

        private ICommand MakeWriteMessageCommand()
        {
            return new Command(
                "write_message",
                new ObjectSchema()
                    .WithRequiredProperty("message", new LogMessageV1Schema()),
                WriteMessage);
        }

        private ICommand MakeWriteMessagesCommand()
        {
            return new Command(
                "write_messages",
                new ObjectSchema()
                    .WithRequiredProperty("messages", new ArraySchema(new LogMessageV1Schema())),
                WriteMessages);
        }

        private ICommand MakeClearCommand()
        {
            return new Command(
                "clear", 
                null, 
                Clear);
        }

        private async Task<object> ReadMessages(string correlationId, Parameters args)
        {
            var filter = FilterParams.FromValue(args.Get("filter"));
            var paging = PagingParams.FromValue(args.Get("paging"));

            return await _loggingBusinessLogic.ReadMessagesAsync(correlationId, filter, paging);
        }

        private async Task<object> ReadErrors(string correlationId, Parameters args)
        {
            var filter = FilterParams.FromValue(args.Get("filter"));
            var paging = PagingParams.FromValue(args.Get("paging"));

            return await _loggingBusinessLogic.ReadErrorsAsync(correlationId, filter, paging);
        }

        private Task<object> WriteMessage(string correlationId, Parameters args)
        {
            var message = ExtractLogMessage(args);

            return Convert(_loggingBusinessLogic.WriteMessageAsync(correlationId, message));
        }

        private Task<object> WriteMessages(string correlationId, Parameters args)
        {
            var messages = ExtractLogMessages(args);

            return Convert(_loggingBusinessLogic.WriteMessagesAsync(correlationId, messages));
        }

        private Task<object> Clear(string correlationId, Parameters args)
        {
            return Convert(_loggingBusinessLogic.ClearAsync(correlationId));
        }

        private LogMessageV1[] ExtractLogMessages(Parameters args)
        {
            var result = new List<LogMessageV1>();

            foreach(var parameters in args.GetAsParameters("messages").Values)
            {
                var message = ExtractLogMessage(AnyValueMap.FromValue(parameters));

                result.Add(message);
            }

            return result.ToArray();
        }

        private static LogMessageV1 ExtractLogMessage(Parameters args)
        {
            var map = args.GetAsMap("message");

            return ExtractLogMessage(map);
        }

        private static LogMessageV1 ExtractLogMessage(AnyValueMap map)
        {
            var time = map.GetAsDateTimeWithDefault("time", DateTime.UtcNow);
            var level = map.GetAsEnum<LogLevel>("level");
            var source = map.GetAsStringWithDefault("source", string.Empty);
            var correlationId = map.GetAsStringWithDefault("correlation_id", string.Empty);
            var error = ExtractError(map.GetAsMap("error"));
            var message = map.GetAsStringWithDefault("message", string.Empty);

            return new LogMessageV1(time, level, source, correlationId, error, message);
        }

        private static ErrorDescription ExtractError(AnyValueMap map)
        {
            if (map.Count == 0)
            {
                return null;
            }

            var code = map.GetAsStringWithDefault("code", string.Empty);
            var message = map.GetAsStringWithDefault("message", string.Empty);
            var stack_trace = map.GetAsStringWithDefault("stack_trace", string.Empty);

            return new ErrorDescription()
            {
                Code = code,
                Message = message,
                StackTrace = stack_trace
            };
        }

        private async Task<object> Convert(Task task)
        {
            var result = new object();

            if (task != null)
            {
                return await task.ContinueWith(obj => result);
            }

            return await Task.FromResult(result);
        }
    }
}