using HtmlAgilityPack;
using ProductSynchronizer.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using ProductSynchronizer.Helpers;

namespace ProductSynchronizer.Parsers
{
    public class StockXWorker : WorkerBase
    {
        public StockXWorker() :base(Resource.StockX, HttpClientVpnType.MixProxy)
        {

        }
        protected override List<ISizeMapNode> ParseHtml(string response)
        {
             //https://stockx.com/nike-air-huarache-drift-black-sail

            var shoesSizeMap = new List<ISizeMapNode>();

            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            var sizesContainer = doc.DocumentNode.SelectNodes("//div[@id='market-summary']//li[contains(@class,'select-option') and not(contains(@class, 'all'))]");

            foreach (var sizeNode in sizesContainer)
            {
                var priceNodeText = sizeNode.SelectSingleNode(".//div[@class='subtitle']");
                var isSizeBid = priceNodeText.InnerText.Trim().ToLower() == "bid";
                var shoeContext = new ShoeContext()
                {
                    ExternalSize = Convert.ToDouble(sizeNode.SelectSingleNode(".//div[@class='title']")
                        .InnerText
                        .Replace("us ", "")
                        .Replace("W",""), CultureInfo.InvariantCulture),
                    ExternalPrice = isSizeBid ? 0 : Convert.ToDouble(priceNodeText
                        .InnerText
                        .Replace("$", ""), CultureInfo.InvariantCulture),
                    Quantity = isSizeBid ? 0 : 999
                };

                shoesSizeMap.Add(shoeContext);
            }

            return shoesSizeMap;
        }
    }
}
