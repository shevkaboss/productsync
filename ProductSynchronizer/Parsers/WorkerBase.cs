using System;
using ProductSynchronizer.Helpers;
using System.Collections.Generic;
using System.Globalization;
using ProductSynchronizer.Entities;
using System.Linq;
using Newtonsoft.Json;

namespace ProductSynchronizer.Parsers
{
    public abstract class WorkerBase : IWorker
    {
        public Product GetSyncedData(Product product)
        {
            if (product == null || string.IsNullOrEmpty(product.Location) || string.IsNullOrEmpty(product.Brand))
            {
                return null;
            }

            var response = GetResponse(product.Location);

            product.ShoesSizeMap = ParseHtml(response);

            Logger.Logger.WriteLog(
                $"Synced sizes before convert: [Id - {product.InternalId}], {JsonConvert.SerializeObject(product.ShoesSizeMap)}");

            UpdateSizes(product);

            Logger.Logger.WriteLog(
                $"Synced sizes after convert: [Id - {product.InternalId}], {JsonConvert.SerializeObject(product.ShoesSizeMap)}");

            UpdatePrice(product);

            Logger.Logger.WriteLog(
                $"After price updating: [Id - {product.InternalId}], {JsonConvert.SerializeObject(product.ShoesSizeMap)}");

            return product;
        }

        public void UpdateProductInDb(Product product)
        {
            if (product != null)
                MySqlHelper.UpdateProduct(product);
        }

        protected string GetResponse(string url)
        {
            return HttpRequestHelper.PerformGetRequest(url);
        }

        protected abstract List<ISizeMapNode> ParseHtml(string response);

        private static void UpdateSizes(Product product)
        {
            Logger.Logger.WriteLog(
                $"Getting size map for: [Resource - {product.Resource}], [Brand - {product.Brand}], [Gender - {product.Gender.ToString()}]");
            var sizeMap = MapsHelper.GetSizesMap(product.Resource, product.Brand, product.Gender);
            foreach (var size in product.ShoesSizeMap)
            {
                size.InternalSize = sizeMap[size.ExternalSize];
            }
        }

        private static void UpdatePrice(Product product)
        {
            var currencyValue = MapsHelper.GetCurrencyValue(product.Resource);
            var usdCurrencyValue = MapsHelper.GetCurrencyValue(Currency.USD);

            foreach (var sizeMapNode in product.ShoesSizeMap)
            {
                var externalPrice = sizeMapNode.ExternalPrice.Replace(".", ",");
                var price = double.Parse(externalPrice) * currencyValue;
                if (price < ConfigHelper.Config.PriceConfig.PriceThreshold * usdCurrencyValue)
                {
                    price += ConfigHelper.Config.PriceConfig.BelowThresholdIncreaseUsd * usdCurrencyValue;
                }
                else
                {
                    price = (price * ConfigHelper.Config.PriceConfig.OverThresholdIncreasePercentage / 100) +
                            (ConfigHelper.Config.PriceConfig.OverThresholdIncreaseUsd * usdCurrencyValue);
                }

                sizeMapNode.InternalPrice = price.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
