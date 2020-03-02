using ProductSynchronizer.Helpers;
using ProductSynchronizer.Parsers;
using Quartz;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductSynchronizer
{
    public class SyncJob : IJob
    {
        private const int THREADS_PER_RUNNER = 1;

        public Task Execute(IJobExecutionContext context)
        {
            return new Task(() =>
            {
                //Debugger.Launch();
                var products = MySqlHelper.GetProducts();

                var runners = CreateRunners(products);

                StartSynchronization(runners);
            });
        }

        #region private Methods
        private static IEnumerable<SyncRunner> CreateRunners(IEnumerable<Product> products)
        {
            var prodList = products.ToList();
            return new List<SyncRunner>
            {
                new SyncRunner(prodList.Where(x => x.Resource == Resource.JimmyJazz), new JimmyWorker()),

                new SyncRunner(prodList.Where(x => x.Resource == Resource.Goat), new GoatWorker()),

                new SyncRunner(prodList.Where(x => x.Resource == Resource.Footasylum), new FootasylumWorker())
            };
        }

        private static void StartSynchronization(IEnumerable<SyncRunner> runners)
        {
            var threads = new List<Thread>();

            try
            {
                foreach (var runner in runners)
                {
                    for (var i = 0; i < ConfigHelper.Config.ThreadsPerResource; i++)
                    {
                        var thread = new Thread(() => runner.Run());
                        threads.Add(thread);
                    }
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }
            finally
            {
                MySqlHelper.Dispose();
            }
        }
        #endregion
    }
}
