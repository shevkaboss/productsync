﻿using Newtonsoft.Json;
using ProductSynchronizer.Entities;
using ProductSynchronizer.Helpers;
using ProductSynchronizer.Logger;
using ProductSynchronizer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ProductSynchronizer.Parsers
{
    public abstract class WorkerBase : IWorker
    {
        private HttpRequestHelper _httpHelper;
        public WorkerBase(bool withProxy = false)
        {
            _httpHelper = new HttpRequestHelper(withProxy);
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
            
            UpdateSizes(product);

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
            var dict = new Dictionary<double, ISizeMapNode>();

            try
            {
                nodes = ParseHtml(response);
                              
                foreach (var node in nodes)
                {
                    if (!dict.ContainsKey(node.ExternalSize))
                    {
                        dict.Add(node.ExternalSize, node);
                        continue;
                    }
                    else
                    {
                        if (dict[node.ExternalSize].ExternalPrice < node.ExternalPrice)
                            dict[node.ExternalSize].ExternalPrice = node.ExternalPrice;
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
        private static void UpdateSizes(Product product)
        {
            var sizesToRemove = new List<ISizeMapNode>();
            Log.WriteLog(
                $"Getting size map for: [Resource - {product.Resource}], [Brand - {product.Brand}], [Gender - {product.Gender.ToString()}]");

            var sizeMap = MapsHelper.GetSizesMap(product.Resource, product.Brand, product.Gender);

            foreach (var size in product.ShoesSizeMap)
            {
                if (sizeMap.ContainsKey(size.ExternalSize))
                {
                    size.InternalSize = sizeMap[size.ExternalSize];

                    var dbSizeId = MapsHelper.GetSizeDbId(size.InternalSize);

                    if (dbSizeId != default)
                    {
                        size.OptionValueId = dbSizeId;
                        continue;
                    }
                    else
                    {
                        throw new InnerException(product.InternalId, $"No OptionValueId (Id in DB) found for {size.InternalSize}");
                    }
                }

                Log.WriteLog($"[NO SIZE NODE FOUND] No size map node found for {size.ExternalSize}");
                sizesToRemove.Add(size);
            }

            //Remove not founded map nodes.
            if (sizesToRemove.Count > 0)
            {
                Log.WriteLog($"ERROR - Sizes not founded in size map: {string.Join(", ", sizesToRemove)}");
                sizesToRemove.ForEach(x => product.ShoesSizeMap.Remove(x));
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

                commonExternalPrice = product.ShoesSizeMap.Min(x => x.ExternalPrice);
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
                price += ConfigHelper.Config.PriceConfig.BelowThresholdIncreaseUsd * usdCurrencyValue;
            }
            else
            {
                Log.WriteLog($"Executing price's over the threshold logic.");
                price = (price * ConfigHelper.Config.PriceConfig.OverThresholdIncreasePercentage / 100) +
                        (ConfigHelper.Config.PriceConfig.OverThresholdIncreaseUsd * usdCurrencyValue);
            }

            return price;
        }
    }
}
