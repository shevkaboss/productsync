using ProductSynchronizer.Helpers;
using System.Collections.Generic;
using ProductSynchronizer.Entities;

namespace ProductSynchronizer.Parsers
{
    public abstract class WorkerBase : IWorker
    {
        protected string GetResponse(string url)
        {
            return HttpRequestHelper.PerformGetRequest(url);
        }
        public abstract List<ISizeMapNode> ParseHtml(string response);
        public Product SyncSizesUnits(Product product)
        {
            return product;
        }
        public void GetSyncedData(Product product)
        {
            if (product == null)
                return;

            var response = GetResponse(product.Location);

            product.ShoesSizeMap = ParseHtml(response);

            SyncSizesUnits(product);
        }
    }
}
