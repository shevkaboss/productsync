using Newtonsoft.Json.Linq;
using ProductSynchronizer.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ProductSynchronizer.Parsers
{
    public class GoatWorker : WorkerBase
    {
        public GoatWorker():base(true)
        {

        }
        #region constants
        private const string GOAT_GET_SNEAKERS_NAME_REGEX = "(?<=sneakers\\/)(.*)";
        private const string GOAT_API_URL = "https://www.goat.com/web-api/v1/product_variants?productTemplateId=";
        #endregion
        protected override List<ISizeMapNode> ParseHtml(string response)
        {
            var shoesSizeMap = new List<ISizeMapNode>();

            var sizesContainer = JArray.Parse(response);

            foreach (var sizeNode in sizesContainer)
            {
                var price = sizeNode["lowestPriceCents"]["amount"].ToObject<string>();
                var shoeContext = new ShoeContext()
                {
                    ExternalSize = sizeNode["size"].ToObject<double>(),
                    ExternalPrice = double.Parse(price.Substring(0, price.Length - 2), CultureInfo.InvariantCulture),
                    Quantity = 999
                };

                shoesSizeMap.Add(shoeContext);
            }

            return shoesSizeMap;
        }

        protected override void UpdateProductLocation(Product product)
        {
            var sneakersName = Regex.Match(product.Location, GOAT_GET_SNEAKERS_NAME_REGEX);
            product.Location = GOAT_API_URL + sneakersName;
        }
    }
}
