using ProductSynchronizer.Helpers;
using ProductSynchronizer.Logger;
using Quartz;
using Quartz.Impl;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ProductSynchronizer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ProductSynchronizer()
            };
            ServiceBase.Run(ServicesToRun);
            //SyncJob.Start();
        }
    }
}
