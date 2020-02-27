using ProductSynchronizer.Helpers;
using ProductSynchronizer.Parsers;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace ProductSynchronizer
{
    public partial class ProductSynchronizer : ServiceBase
    {
        private const int THREADS_PER_RUNNER = 1;
        public ProductSynchronizer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            System.Diagnostics.Debugger.Launch();

            IEnumerable<Product> products;

            using (var sql = new MySqlHelper())
            {
                products = sql.GetProducts();
            }

            var runners = CreateRunners(products);

            StartSynchronization(runners);
        }

        protected override void OnStop()
        {
        }

        #region private Methods
        private IEnumerable<SyncRunner> CreateRunners(IEnumerable<Product> products)
        {
            var prodList = products.ToList();
            return new List<SyncRunner>
            {
                new SyncRunner(prodList.Where(x => x.Resource == Resource.JimmyJazz), new JimmyWorker()),

                new SyncRunner(prodList.Where(x => x.Resource == Resource.Goat), new StockWorker()),

                new SyncRunner(prodList.Where(x => x.Resource == Resource.Footasylum), new FootasylumWorker())
            };
        }

        private void StartSynchronization(IEnumerable<SyncRunner> runners)
        {
            foreach (var runner in runners)
            {
                for (int i = 0; i < THREADS_PER_RUNNER; i++)
                    new Thread(() => runner.Run());
            }
        }
        #endregion
    }
}
