using System.Collections.Generic;
using System.Linq;
using ProductSynchronizer.Entities;

namespace ProductSynchronizer.Parsers
{
    public class FootasylumWorker : WorkerBase
    {
        #region html constants
        private const string PRICE_XPATH = "//span[@data-bind='css:nowpricecss,text: price']";
        private const string SIZE_BLOCK_XPATH = "//div[contains(@class,'pointer option2dropElm')]";
        private const string SIZE_VALUE_XPATH = "//span[@data-bind='html: option2_js']";
        private const string QUANTITY_XPATH = "//span[@data-bind='html: stock_status, css: opt2stockcss']";
        private const string IN_STOCK_TEXT = "in stock";
        #endregion

        public override List<ISizeMapNode> ParseHtml(string response)
        {
            var shoesSizeMap = new List<ISizeMapNode>();

            var t = new HtmlAgilityPack.HtmlDocument();
            t.Load(response);

            var price = t.DocumentNode.SelectSingleNode(PRICE_XPATH).InnerText
                .Replace("£", "");

            var sizeBlocks = t.DocumentNode.SelectNodes(SIZE_BLOCK_XPATH);

            foreach (var node in sizeBlocks)
            {
                var shoeContext = new ShoeContext
                {
                    InternalSize = node.SelectSingleNode(SIZE_VALUE_XPATH).InnerText,
                    Quantity = node.SelectSingleNode(QUANTITY_XPATH).InnerText == IN_STOCK_TEXT
                        ? 999
                        : 0,
                    Price = price
                };
                shoesSizeMap.Add(shoeContext);
            }

            return shoesSizeMap;
        }
    }
}
