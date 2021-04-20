using Newtonsoft.Json.Linq;
using ProductSynchronizer.Entities;
using ProductSynchronizer.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProductSynchronizer.Parsers
{
    public class SivasdescalzoWorker : WorkerBase
    {
        public SivasdescalzoWorker():base(Resource.Sivasdescalzo)
        {

        }
        protected override List<ISizeMapNode> ParseHtml(string response)
        {
            //https://www.sivasdescalzo.com/en/sb-zoom-blazer-mid-edge-ci3833-101
            var shoesSizeMap = new List<ISizeMapNode>();

            var regex = Regex.Match(response, "options\":(.*]}])").Groups[1].Value;

            var price = Regex.Match(response, "\"price_info\":{\"final_price\":(.*?),").Groups[1].Value;

            if (response.Contains("<span>Sold out</span>"))
                return shoesSizeMap;

            var sizesContainer = JArray.Parse(regex);

            foreach (var sizeNode in sizesContainer)
            {
                var shoeContext = new ShoeContext()
                {
                    ExternalSize = sizeNode["label"].ToObject<string>().Trim(),
                    ExternalPrice = Convert.ToDouble(price),
                    Quantity = sizeNode["products"].ToObject<IEnumerable<object>>().Count() != 0 ? 999 : 0
                };

                shoesSizeMap.Add(shoeContext);
            }

            return shoesSizeMap;
        }
    }
}
