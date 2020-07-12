using Newtonsoft.Json.Linq;
using ProductSynchronizer.Entities;
using ProductSynchronizer.Logger;
using ProductSynchronizer.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ProductSynchronizer.Parsers
{
    public class JimmyWorker : WorkerBase
    {
        protected override List<ISizeMapNode> ParseHtml(string response)
        {
            var jimmyShoesSizeMap = new List<ISizeMapNode>();

            var json = Regex.Match(response, "(?<=var meta =)(.*}})(?=;)").Groups[0].Value;

            var sizesContainer = JObject.Parse(json);

            var productId = sizesContainer["product"]["id"].ToObject<string>();

            foreach (var sizeVariantsObject in sizesContainer["product"]["variants"].AsJEnumerable())
            {
                var price = sizeVariantsObject["price"].ToObject<string>();
                var jimmyShoeContext = new JimmyShoeContext
                {
                    Id = sizeVariantsObject["id"].ToObject<string>(),
                    ExternalSize = sizeVariantsObject["public_title"].ToObject<double>(),
                    ExternalPrice = double.Parse(price.Insert(price.Length - 2, "."), CultureInfo.InvariantCulture)
                };

                var shoesSizeQuantityString = Regex.Match(response, $"(?<=\\[\'{productId}\'\\]\\[{jimmyShoeContext.Id}\\] = )(.*)(?=;)").Groups[0].Value;

                if (int.TryParse(shoesSizeQuantityString, out var shoesSizeQuantity))
                    jimmyShoeContext.Quantity = shoesSizeQuantity;

                jimmyShoesSizeMap.Add(jimmyShoeContext);
            }

            return jimmyShoesSizeMap;
        }
    }
}
