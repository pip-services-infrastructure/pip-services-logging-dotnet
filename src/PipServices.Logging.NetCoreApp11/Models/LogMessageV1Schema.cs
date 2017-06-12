using PipServices.Commons.Convert;
using PipServices.Commons.Validate;

namespace PipServices.Logging.Models
{
    public class LogMessageV1Schema : ObjectSchema
    {
        public LogMessageV1Schema()
        {
            var errorSchema = new ObjectSchema()
                .WithOptionalProperty("code", TypeCode.String)
                .WithOptionalProperty("message", TypeCode.String)
                .WithOptionalProperty("stack_trace", TypeCode.String);
            WithOptionalProperty("time", null); 
            WithOptionalProperty("correlation_id", TypeCode.String);
            WithOptionalProperty("source", TypeCode.String);
            WithRequiredProperty("level", TypeCode.Long);
            WithOptionalProperty("message", TypeCode.String);
            WithOptionalProperty("error", errorSchema);
        }
    }
}