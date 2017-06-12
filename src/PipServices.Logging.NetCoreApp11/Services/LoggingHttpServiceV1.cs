using PipServices.Commons.Refer;
using PipServices.Net.Rest;

namespace PipServices.Logging.Services
{
    public class LoggingHttpServiceV1 : CommandableHttpService
    {
        public LoggingHttpServiceV1()
            : base("logging")
        {
            _dependencyResolver.Put("controller", new Descriptor("pip-services-logging", "controller", "default", "*", "1.0"));
        }
    }
}

