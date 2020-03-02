using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using ProductSynchronizer.Entities;

namespace ProductSynchronizer.Parsers
{
    public class GoatWorker : WorkerBase
    {
        private const string VARIANTS_REGEX = "(?<=\"formatted_available_sizes_new_v2\":)(\\[.*shoe_condition\":\"[0-9]\"}\\])(?=,)";
        protected override List<ISizeMapNode> ParseHtml(string response)
        {
            var shoesSizeMap = new List<ISizeMapNode>();

            try
            {
                var regex = Regex
                    .Match(response, VARIANTS_REGEX).Groups[0].Value;
                var sizesContainer = JArray.Parse(regex);

                foreach (var sizeVariantsObject in sizesContainer)
                {
                    var price = sizeVariantsObject["price_cents"].ToObject<string>();
                    var jimmyShoeContext = new ShoeContext()
                    {
                        ExternalSize = sizeVariantsObject["size"].ToObject<string>(),
                        ExternalPrice = price.Insert(price.Length - 2, "."),
                        Quantity = 999
                    };
                    shoesSizeMap.Add(jimmyShoeContext);
                }
            }
            catch
            {
            }

            return shoesSizeMap;
        }
    }
}
