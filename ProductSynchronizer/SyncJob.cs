using ProductSynchronizer.Helpers;
using ProductSynchronizer.Logger;
using ProductSynchronizer.Parsers;
using ProductSynchronizer.Utils;
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
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
                Debugger.Launch();
                Log.WriteLog("Creating runners");

                var products = MySqlHelper.GetProducts().ToList();

                var productIds = products.Select(x => x.InternalId.ToString()).ToList();

                Log.WriteLog($"Running sync job for products: {string.Join(", ", productIds)}");

                MySqlHelper.UpdateProductsOnStart(productIds);

                var runners = CreateRunners(products);

                StartSynchronization(runners);
            });
        }

        public static void Start()
        {
            Log.WriteLog("Creating runners");

            var products = MySqlHelper.GetProducts().Where(x => x.Resource == Resource.StockX && x.InternalId == 11910).Take(5);

            var productIds = products.Select(x => x.InternalId.ToString());

            Log.WriteLog($"Running sync job for products: {string.Join(", ", productIds)}");

            MySqlHelper.UpdateProductsOnStart(productIds);

            var runners = CreateRunnersTest(products);

            StartSynchronization(runners);
        }

        private static IEnumerable<SyncRunner> CreateRunnersTest(IEnumerable<Product> products)
        {
            var prodList = products.ToList();
            return new List<SyncRunner>
            {
                //new SyncRunner(prodList.Where(x => x.Resource == Resource.JimmyJazz), new JimmyWorker()),

                //new SyncRunner(prodList.Where(x => x.Resource == Resource.Goat), new GoatWorker()),

                //new SyncRunner(prodList.Where(x => x.Resource == Resource.Footasylum), new FootasylumWorker()),

                new SyncRunner(prodList.Where(x => x.Resource == Resource.StockX), new StockXWorker()),

                //new SyncRunner(prodList.Where(x => x.Resource == Resource.Sivasdescalzo), new SivasdescalzoWorker())
            };
        }

        #region private Methods
        private static IEnumerable<SyncRunner> CreateRunners(IEnumerable<Product> products)
        {
            var prodList = products.ToList();
            return new List<SyncRunner>
            {
                new SyncRunner(prodList.Where(x => x.Resource == Resource.JimmyJazz), new JimmyWorker()),

                new SyncRunner(prodList.Where(x => x.Resource == Resource.Goat), new GoatWorker()),

                new SyncRunner(prodList.Where(x => x.Resource == Resource.Footasylum), new FootasylumWorker()),

                new SyncRunner(prodList.Where(x => x.Resource == Resource.StockX), new StockXWorker()),

                new SyncRunner(prodList.Where(x => x.Resource == Resource.Sivasdescalzo), new SivasdescalzoWorker())
            };
        }

        private static void StartSynchronization(IEnumerable<SyncRunner> runners)
        {
            var threads = new List<Thread>();

            MapsHelper.UpdateMaps();

            try
            {
                var sw = Stopwatch.StartNew();

                foreach (var runner in runners)
                {
                    for (var i = 0; i < ConfigHelper.Config.ThreadsPerResource; i++)
                    {
                        var thread = new Thread(() => runner.Run());
                        thread.Start();
                        threads.Add(thread);
                    }
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }
                sw.Stop();

                Log.ResultLog(sw.Elapsed.ToString(), UnsuccessfulItemsHandler.GetErrors());

                MySqlHelper.UpdateUnsuccessfulProducts(UnsuccessfulItemsHandler.GetUnsuccessfulProductIdsToUpdate());
            }
            catch
            {
                Log.WriteLog("Unknown error when running sync processes");
            }

            UnsuccessfulItemsHandler.ClearErrors();
        }
        #endregion
    }
}
