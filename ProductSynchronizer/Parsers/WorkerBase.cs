using Newtonsoft.Json;
using ProductSynchronizer.Entities;
using ProductSynchronizer.Helpers;
using ProductSynchronizer.Logger;
using ProductSynchronizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductSynchronizer.Parsers
{
    public abstract class WorkerBase : IWorker
    {
        private HttpRequestHelper _httpHelper;
        public WorkerBase(Resource resource, HttpClientVpnType vpnType = HttpClientVpnType.NoProxy)
        {
            _httpHelper = new HttpRequestHelper(resource, vpnType);
        }
        public Product GetSyncedData(Product product)
        {
            if (string.IsNullOrEmpty(product.Location) || string.IsNullOrEmpty(product.Brand))
            {
                throw new InnerException(product.InternalId, "Bad product data.");
            }

            UpdateProductLocation(product);

            string response;

            try
            {
                response = GetResponse(product.Location);
            }
            catch (Exception e)
            {
                Log.WriteLog($"Bad request for {product.Location}");
                throw new InnerException(product.InternalId, $"Bad request for url: [{product.Location}], {e.Message}", true);
            }

            ParseSizesFromResponse(product, response);

            Log.WriteLog(
                $"Synced sizes before convert: [Id - {product.InternalId}], {JsonConvert.SerializeObject(product.ShoesSizeMap)}");
            
            UpdateSizesWithExternal(product);

            Log.WriteLog(
                $"Synced sizes after convert: [Id - {product.InternalId}], {JsonConvert.SerializeObject(product.ShoesSizeMap)}");

            UpdatePrice(product);

            Log.WriteLog(
                $"After price updating: [Id - {product.InternalId}], {JsonConvert.SerializeObject(product.ShoesSizeMap)}");

            return product;
        }

        private void ParseSizesFromResponse(Product product, string response)
        {
            List<ISizeMapNode> nodes;
            var dict = new Dictionary<string, ISizeMapNode>();

            try
            {
                nodes = ParseHtml(response);
                              
                foreach (var node in nodes)
                {
                    if (dict.ContainsKey(node.ExternalSize))
                    {
                        if (dict[node.ExternalSize].ExternalPrice < node.ExternalPrice)
                            dict[node.ExternalSize].ExternalPrice = node.ExternalPrice;
                    }
                    else
                    {
                        dict.Add(node.ExternalSize, node);
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteLog($"Error parsing html: {e.Message}");
                throw new InnerException(product.InternalId, $"Error while parsing html: {e.Message}", true);
            }

            if (nodes.Count == 0)
            {
                throw new InnerException(product.InternalId, "No sizes found while parsing html", true);
            }

            product.ShoesSizeMap = dict.Values.ToList();

        }

        public void UpdateProductInDb(Product product)
        {
            if (product == null ||
                product.InternalId <= 0 ||
                product.ProductOptionId <= 0 ||
                product.ShoesSizeMap.FirstOrDefault() == null)
            {
                return;
            }

            MySqlHelper.UpdateProduct(product);
        }
        protected string GetResponse(string url)
        {
            return _httpHelper.PerformGetRequest(url);
        }
        protected abstract List<ISizeMapNode> ParseHtml(string response);
        protected virtual void UpdateProductLocation(Product product)
        {
            return;
        }
        // private static void UpdateSizes(Product product)
        // {
        //     var sizesToRemove = new List<ISizeMapNode>();
        //     Log.WriteLog(
        //         $"Getting size map for: [Resource - {product.Resource}], [Brand - {product.Brand}], [Gender - {product.Gender}]");
        //
        //     var sizeMap = MapsHelper.GetSizesMap(product.Resource, product.Brand, product.Gender);
        //
        //     foreach (var size in product.ShoesSizeMap)
        //     {
        //         if (sizeMap.ContainsKey(size.ExternalSize))
        //         {
        //             size.InternalSize = size.ExternalSize;
        //
        //             var dbSizeId = MapsHelper.GetSizeDbId(size.InternalSize);
        //
        //             if (dbSizeId == default)
        //             {
        //                 throw new InnerException(product.InternalId,
        //                     $"No OptionValueId (Id in DB) found for {size.InternalSize}");
        //             }
        //
        //             size.OptionValueId = dbSizeId;
        //             continue;
        //         }
        //
        //         Log.WriteLog($"[NO SIZE NODE FOUND] No size map node found for {size.ExternalSize}");
        //         sizesToRemove.Add(size);
        //     }
        //
        //     //Remove not founded map nodes.
        //     if (sizesToRemove.Count > 0)
        //     {
        //         Log.WriteLog($"ERROR - Sizes not founded in size map: {string.Join(", ", sizesToRemove.Select(x => x.ExternalSize))}");
        //         sizesToRemove.ForEach(x => product.ShoesSizeMap.Remove(x));
        //     }
        // }

        private static void UpdateSizesWithExternal(Product product)
        {
            foreach (var size in product.ShoesSizeMap)
            {
                size.InternalSize = size.ExternalSize;

                var dbSizeId = MapsHelper.GetSizeDbId(size.InternalSize);

                if (dbSizeId == default)
                {
                    throw new InnerException(product.InternalId,
                        $"No OptionValueId (Id in DB) found for {size.InternalSize}");
                }

                size.OptionValueId = dbSizeId;
            }
        }
        private static void UpdatePrice(Product product)
        {
            var currencyValue = MapsHelper.GetCurrencyValue(product.Resource);

            Log.WriteLog(
                $"{product.Resource} currency to UAH value: {currencyValue}");

            var usdCurrencyValue = MapsHelper.GetCurrencyValue(Currency.USD);

            double commonExternalPrice;

            if (product.Resource == Resource.Goat || product.Resource == Resource.StockX)
            {

                foreach (var sizeMapNode in product.ShoesSizeMap)
                {
                    var price = GetUpdatedPrice(currencyValue, usdCurrencyValue, sizeMapNode.ExternalPrice);

                    sizeMapNode.InternalPrice = price;
                }   

                commonExternalPrice = product.ShoesSizeMap.Where(x => x.Quantity > 0).Min(x => x.ExternalPrice);
            }
            else
            {               
                commonExternalPrice = product.ShoesSizeMap.FirstOrDefault().ExternalPrice;
            }

            UpdateCommonPrice(product, currencyValue, usdCurrencyValue, commonExternalPrice);
        }

        private static void UpdateCommonPrice(Product product, double currencyValue, double usdCurrencyValue, double commonPrice)
        {
            var price = GetUpdatedPrice(currencyValue, usdCurrencyValue, commonPrice);

            product.CommonPrice = price;
        }
        private static double GetUpdatedPrice(double currencyValue, double usdCurrencyValue, double externalPrice)
        {
            var price = externalPrice * currencyValue;
            Log.WriteLog($"Product's parsed price: [{externalPrice}] in EXT - [{price}] in UAH.");
            if (price < ConfigHelper.Config.PriceConfig.PriceThreshold * usdCurrencyValue)
            {
                Log.WriteLog($"Executing price's below the threshold logic.");
                price = ConfigHelper.Config.PriceConfig.BelowThresholdIncreaseUsd * (price + 25 * usdCurrencyValue);
            }
            else
            {
                Log.WriteLog($"Executing price's over the threshold logic.");
                price = ConfigHelper.Config.PriceConfig.OverThresholdIncreaseUsd * (price + 25 * usdCurrencyValue);
            }

            return price;
        }
    }
}
