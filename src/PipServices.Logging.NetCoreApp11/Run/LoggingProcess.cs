using System.Threading;
using System.Threading.Tasks;

using PipServices.Commons.Refer;
using PipServices.Container;
using PipServices.Logging.Build;

namespace PipServices.Logging.Run
{
    public class LoggingProcess : ProcessContainer
    {
        protected override void InitReferences(IReferences references)
        {
            base.InitReferences(references);

            // Factory to statically resolve echo components
            references.Put(Descriptors.LoggingFactory, new LoggingFactory());
        }

        public Task RunAsync(string[] args, CancellationToken token)
        {
            var configPath = args.Length > 0 ? args[0] : "../../config/config.yaml";
            return RunWithConfigFileAsync("logging", args, configPath, token);
        }
    }
}
