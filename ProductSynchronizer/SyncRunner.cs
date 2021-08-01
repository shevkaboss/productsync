using Newtonsoft.Json;
using ProductSynchronizer.Logger;
using ProductSynchronizer.Parsers;
using ProductSynchronizer.Utils;
using System;
using System.Collections.Generic;

namespace ProductSynchronizer
{
    public class SyncRunner
    {
        private readonly object _lockStack = new object();
        private Stack<Product> Products { get; }
        private IWorker Worker { get; }
        private readonly string _workerType;

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
                Product product;
                try
                {

                    lock (_lockStack)
                    {
                        Log.WriteLog($"{_workerType} stack length {Products.Count}");
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
                    Log.WriteLog($"Error while POPing data from stack: {e.Message}");
                    continue;
                }

                Log.WriteLog($"Starting sync for product: {JsonConvert.SerializeObject(product)}");

                try
                {
                    var resultProduct = Worker.GetSyncedData(product);

                    if (resultProduct == null)
                    {
                        Log.WriteLog($"Synced product reference is null.");
                        throw new Exception("Synced product reference is null.");
                    }

                    Worker.UpdateProductInDb(product);
                    Log.WriteLog($"Updated product: {JsonConvert.SerializeObject(product)}");
                }
                catch (InnerException e)
                {
                    UnsuccessfulItemsHandler.AddError(e.Error);
                    Log.WriteLog($"Inner error while syncing data: {e.Message} {Environment.NewLine} Stack trace: {e.StackTrace}");
                }
                catch (Exception e)
                {
                    UnsuccessfulItemsHandler.AddUnsuccessfulProduct(product.InternalId, "Техническая ошибка");
                    Log.WriteLog($"Unknown error while sync data: {e.Message} {Environment.NewLine} Stack trace: {e.StackTrace}");
                }
            }
        }

    }
}
