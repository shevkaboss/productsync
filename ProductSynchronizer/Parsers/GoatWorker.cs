using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using ProductSynchronizer.Entities;

namespace ProductSynchronizer.Parsers
{
    public class GoatWorker : WorkerBase
    {
        public override List<ISizeMapNode> ParseHtml(string response)
        {
            var jimmyShoesSizeMap = new List<ISizeMapNode>();

            try
            {
                var sizesContainer = JObject.Parse(Regex
                    .Match(response, "(?<=\"formatted available_sizes_new_v2\":)(.*}])(?=,)").Groups[0].Value);

                foreach (var sizeVariantsObject in sizesContainer.ToObject<List<JObject>>())
                {
                    var price = sizeVariantsObject["price_cents"].ToObject<string>();
                    var jimmyShoeContext = new ShoeContext()
                    {
                        InternalSize = sizeVariantsObject["size"].ToObject<string>(),
                        Price = price.Insert(price.Length - 2, "."),
                        Quantity = 999
                    };
                    jimmyShoesSizeMap.Add(jimmyShoeContext);
                }
            }
            catch
            {
                // ignored
            }

            return jimmyShoesSizeMap;
        }
    }
}
