using Newtonsoft.Json.Linq;
using ProductSynchronizer.Entities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProductSynchronizer.Parsers
{
    public class JimmyWorker : WorkerBase
    {
        protected override List<ISizeMapNode> ParseHtml(string response)
        {
            var jimmyShoesSizeMap = new List<ISizeMapNode>();

            try
            {
                var sizesContainer = JObject.Parse(Regex.Match(response, "(?<=var meta =)(.*}})(?=;)").Groups[0].Value);

                var productId = sizesContainer["product"]["id"].ToObject<string>();


                foreach (var sizeVariantsObject in sizesContainer["product"]["variants"].ToObject<List<JObject>>())
                {
                    var price = sizeVariantsObject["price"].ToObject<string>();
                    var jimmyShoeContext = new JimmyShoeContext
                    {
                        Id = sizeVariantsObject["id"].ToObject<string>(),
                        ExternalSize = sizeVariantsObject["public_title"].ToObject<string>(),
                        ExternalPrice = price.Insert(price.Length - 2, ".")
                    };

                    var shoesSizeQuantityString = Regex.Match(response, $"(?<=\\[\'{productId}\'\\]\\[{jimmyShoeContext.Id}\\] = )(.*)(?=;)").Groups[0].Value;

                    if (int.TryParse(shoesSizeQuantityString, out var shoesSizeQuantity))
                        jimmyShoeContext.Quantity = shoesSizeQuantity;

                    jimmyShoesSizeMap.Add(jimmyShoeContext);
                }
            }
            catch (Exception e)
            {
                Logger.Logger.WriteLog($"Error parsing html: {e.Message}");
            }

            return jimmyShoesSizeMap;
        }
    }
}
