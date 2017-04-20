using System;
using System.Threading;
using PipServices.Logging.Run;

namespace PipServices.Logging
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var task = (new LoggingProcess()).RunAsync(args, CancellationToken.None);
                task.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
