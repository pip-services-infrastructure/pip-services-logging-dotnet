using System;

using Microsoft.AspNetCore.Mvc;

using PipServices.Logging.Logic;

namespace PipServices.Logging.Controllers
{
    [Route("logging")]
    public class LoggingApiController : Controller
    {
        public LoggingApiController(ILoggingBusinessLogic controller)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            _controller = controller;
        }

        private readonly ILoggingBusinessLogic _controller;
    }
}
