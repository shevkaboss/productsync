using ProductSynchronizer.Helpers;
using System.Collections.Generic;
using ProductSynchronizer.Entities;
using System.Linq;

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
            foreach (var size in product.ShoesSizeMap)
            {
                var mapNode = Constants.SizeMap
                    .First(x => x.Name == product.Brand).MapsByGender
                    .First(x => x.Gender == product.Gender).MapNodes;
                switch (product.Resource)
                {
                    case (Resource.Footasylum):
                        size.InternalSize = mapNode.First(x => x.UK == size.ExternalSize).EU;
                        break;
                    case (Resource.Goat):
                    case (Resource.JimmyJazz):
                        size.InternalSize = mapNode.First(x => x.US == size.ExternalSize).EU;
                        break;
                }
            }
            return product;
        }
        public Product GetSyncedData(Product product)
        {
            if (product == null || string.IsNullOrEmpty(product.Location) || string.IsNullOrEmpty(product.Brand))
                return null;

            var response = GetResponse(product.Location);

            product.ShoesSizeMap = ParseHtml(response);

            SyncSizesUnits(product);

            return product;
        }
    }
}
