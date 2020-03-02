using System;
using ProductSynchronizer.Parsers;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ProductSynchronizer
{
    public class SyncRunner
    {
        private readonly object _lockStack = new object();
        private Stack<Product> Products { get; }
        private IWorker Worker { get; }
        private string _workerType;

        public SyncRunner(IEnumerable<Product> products, IWorker worker)
        {
            Products = new Stack<Product>(products);
            Worker = worker;
            _workerType = worker.ToString();
        }

        public void Run()
        {
            while (true)
            {
                Product product = null;
                try
                {

                    lock (_lockStack)
                    {
                        Logger.Logger.WriteLog($"{_workerType} stack length {Products.Count}");
                        if (Products.Count > 0)
                        {
                            product = Products.Pop();
                        }
                        else
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.Logger.WriteLog($"Error while POPing data from stack: {e.Message}");
                    continue;
                }


                Logger.Logger.WriteLog($"Starting sync for product: {JsonConvert.SerializeObject(product)}");

                Product resultProduct;

                try
                {
                    resultProduct = Worker.GetSyncedData(product);
                }
                catch (Exception e)
                {
                    Logger.Logger.WriteLog($"Error while syncing data: {e.Message}");
                    continue;
                }

                Logger.Logger.WriteLog($"Updated product: {JsonConvert.SerializeObject(product)}");

                Worker.UpdateProductInDb(resultProduct);
            }
        }

    }
}
