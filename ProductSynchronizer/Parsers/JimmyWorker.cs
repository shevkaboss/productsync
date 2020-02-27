using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProductSynchronizer.Parsers
{
    public class JimmyWorker : WorkerBase
    {
        public override List<ISizeMapNode> ParseHtml(string response)
        {
            var jimmyShoesSizeMap = new List<ISizeMapNode>();

            try
            {
                var sizesConteiner = JObject.Parse(Regex.Match(response, "(?<=var meta =)(.*}})(?=;)").Groups[0].Value);

                var productId = sizesConteiner["product"]["id"].ToObject<string>();


                foreach (var sizeVariantsObject in sizesConteiner["product"]["variants"].ToObject<List<JObject>>())
                {
                    var jimmyShoeContext = new JimmyShoeContext
                    {
                        Id = sizeVariantsObject["id"].ToObject<string>(),
                        InternalSize = sizeVariantsObject["public_title"].ToObject<string>()
                    };

                    var shoesSizeQuantityString = Regex.Match(response, $"(?<=\\[\'{productId}\'\\]\\[{jimmyShoeContext.Id}\\] = )(.*)(?=;)").Groups[0].Value;

                    if (int.TryParse(shoesSizeQuantityString, out int shoesSizeQuantity))
                        jimmyShoeContext.Quantity = shoesSizeQuantity;

                    jimmyShoesSizeMap.Add(jimmyShoeContext);
                }
            }
            catch
            {

            }

            return jimmyShoesSizeMap;
        }
    }
}
