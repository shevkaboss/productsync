using ProductSynchronizer.Parsers;
using System.Collections.Generic;

namespace ProductSynchronizer
{
    public class SyncRunner
    {
        private readonly object lockStack = new object();
        private Stack<Product> Products { get; }
        private IWorker Worker { get; }

        public SyncRunner(IEnumerable<Product> products, IWorker worker)
        {
            Products = new Stack<Product>(products);
            Worker = worker;
        }

        public void Run()
        {
            while (true)
            {
                Product product = null;
                lock (lockStack)
                {
                    if (Products.Count > 0)
                        product = Products.Pop();
                    else
                        break;
                }
                Worker.GetSyncedData(product);
            }
        }

    }
}
