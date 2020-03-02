using ProductSynchronizer.Parsers;
using System.Collections.Generic;

namespace ProductSynchronizer
{
    public class SyncRunner
    {
        private readonly object _lockStack = new object();
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
                Product product;
                lock (_lockStack)
                {
                    if (Products.Count > 0)
                        product = Products.Pop();
                    else
                        break;
                }
                var resultProduct = Worker.GetSyncedData(product);
                Worker.UpdateProductInDb(resultProduct);
            }
        }

    }
}
