using Newtonsoft.Json.Linq;
using ProductSynchronizer.Entities;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ProductSynchronizer.Parsers
{
    public class FootasylumWorker : WorkerBase
    {
        public FootasylumWorker() : base(Resource.Footasylum)
        {

        }
        #region constants
        private const string IN_STOCK_TEXT = "in stock";
        private const string URL_REGEX_PATTERN = "(?<=dataLayer = \\[)({.*})(?=\\];)";
        private const string VARIANTS_REGEX_PATTERN = "(?<=variants = )({.*} })";
        #endregion

        protected override List<ISizeMapNode> ParseHtml(string response)
        {
            var shoesSizeMap = new List<ISizeMapNode>();

            var urlRegex = Regex
                .Match(response, URL_REGEX_PATTERN).Groups[0].Value;
            var url = JObject.Parse(urlRegex)["eliteURL"].ToString();

            var variantsRegex = Regex
                .Match(response, VARIANTS_REGEX_PATTERN).Groups[0].Value;
            var sizesContainer = JObject.Parse(variantsRegex);

            foreach (var sizeVariantsObject in sizesContainer.Children().Children())
            {
                if (url.Contains(sizeVariantsObject["pf_url"].ToString()))
                {
                    var ShoeContext = new ShoeContext()
                    {
                        ExternalSize = sizeVariantsObject["option2"].ToObject<double>(),
                        ExternalPrice = double.Parse(sizeVariantsObject["price"].ToString().Replace("£", ""), CultureInfo.InvariantCulture),
                        Quantity = sizeVariantsObject["stock_status"].ToString() == IN_STOCK_TEXT ?
                      999 : 0
                    };
                    shoesSizeMap.Add(ShoeContext);
                }
            }

            return shoesSizeMap;
        }
    }
}
