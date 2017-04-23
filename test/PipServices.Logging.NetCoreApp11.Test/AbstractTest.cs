using System;

namespace PipServices.Logging.NetCoreApp11.Test
{
    public abstract class AbstractTest : IDisposable
    {
        protected AbstractTest()
        {
            Initialize();
        }

        public void Dispose()
        {
            Uninitialize();
        }

        protected abstract void Initialize();

        protected abstract void Uninitialize();
    }
}
