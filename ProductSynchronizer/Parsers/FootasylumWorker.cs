using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using ProductSynchronizer.Entities;

namespace ProductSynchronizer.Parsers
{
    public class FootasylumWorker : WorkerBase
    {
        #region constants
        private const string IN_STOCK_TEXT = "in stock";
        private const string URL_REGEX_PATTERN = "(?<=dataLayer = \\[)({.*})(?=\\];)";
        private const string VARIANTS_REGEX_PATTERN = "(?<=variants = )({.*} })";
        #endregion

        public override List<ISizeMapNode> ParseHtml(string response)
        {
            var shoesSizeMap = new List<ISizeMapNode>();

            try
            {
                var urlRegex = Regex
                    .Match(response, URL_REGEX_PATTERN).Groups[0].Value;
                var url = JObject.Parse(urlRegex)["eliteURL"].ToString();

                var variantsRegex = Regex
                    .Match(response, VARIANTS_REGEX_PATTERN).Groups[0].Value;
                var sizesContainer = JObject.Parse(variantsRegex);

                foreach (var sizeVariantsObject in sizesContainer.ToObject<Dictionary<string,JObject>>().Values)
                {
                    if (url.Contains(sizeVariantsObject["pf_url"].ToString()))
                    {
                        var jimmyShoeContext = new ShoeContext()
                        {
                            ExternalSize = sizeVariantsObject["option2"].ToString(),
                            Price = sizeVariantsObject["price"].ToString().Replace("£", ""),
                            Quantity = sizeVariantsObject["stock_status"].ToString() == IN_STOCK_TEXT ?
                          999 : 0
                        };
                        shoesSizeMap.Add(jimmyShoeContext);
                    }
                }
            }
            catch
            {
                // ignored
            }

            return shoesSizeMap;
        }
    }
}
