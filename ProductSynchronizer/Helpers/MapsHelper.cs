using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductSynchronizer.Entities;
using ProductSynchronizer.Logger;
using ProductSynchronizer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProductSynchronizer.Helpers
{
    public static class MapsHelper
    {
        private static readonly List<Brand> SizeMap;
        private static readonly Dictionary<Currency, double> CurrencyMap;
        private static readonly Dictionary<int, double> SizesDbMap;
        static MapsHelper()
        {
            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigHelper.SizesConfigFilePath);

            SizeMap =
                JsonConvert.DeserializeObject<List<Brand>>(File.ReadAllText(configFilePath));
            CurrencyMap = GetCurrencyMap();
            SizesDbMap = MySqlHelper.GetDbSizesMap();
        }
        public static Dictionary<double, double> GetSizesMap(Resource resource, string brand, Gender gender)
        {
            MapNode[] mapNode = null;
            try
            {
                mapNode = SizeMap
                    .First(x => x
                        .Name
                        .Split(new string[] { "||" }, StringSplitOptions.None)
                        .Select(b => b.ToLower())
                        .Contains(brand.ToLower()))
                    .MapsByGender
                    .First(x => x.Gender == gender)
                    .MapNodes;
            }
            catch
            {
                throw new InnerException($"Size map for brand [{brand}] is not defined");
            }

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault

            try
            {
                switch (resource)
                {
                    case Resource.Footasylum:
                        return mapNode.ToDictionary(mn => mn.UK, mn => mn.EU);
                    case Resource.Goat:
                    case Resource.JimmyJazz:
                    case Resource.Sivasdescalzo:
                    case Resource.StockX:
                        return mapNode.ToDictionary(mn => mn.US, mn => mn.EU);
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                throw new InnerException($"Failed to create Sizes Map from sizes.json, error: {e.Message}");
            }
        }
        public static double GetCurrencyValue(Resource resource)
        {
            switch (resource)
            {
                case Resource.Goat:
                case Resource.JimmyJazz:
                case Resource.StockX:
                    return CurrencyMap[Currency.USD];
                case Resource.Footasylum:
                    return CurrencyMap[Currency.GBP];
                case Resource.Sivasdescalzo:
                    return CurrencyMap[Currency.EUR];
                default:
                    return -1;
            }
        }
        public static double GetCurrencyValue(Currency currency)
        {
            return CurrencyMap[currency];
        }
        public static int GetSizeDbId(double size)
        {
            return SizesDbMap.FirstOrDefault(x => x.Value == size).Key;
        }
        private static Dictionary<Currency, double> GetCurrencyMap()
        {
            var result = new Dictionary<Currency, double>();

            var response = HttpRequestHelper.PerformGetRequestStatic(
                ConfigHelper.Config.CurrencyApiUrl);

            if (response == null)
            {
                Log.WriteLog($"Currency request response is null");
                return result;
            }

            var obj = JObject.Parse(response);

            foreach (var child in obj.Children().Children())
            {
                var currencyValue = child["ask"].ToObject<double>();
                var currencyName = child.Parent.ToObject<JProperty>().Name;
                if (Enum.TryParse(currencyName, true, out Currency currency))
                {
                    result.Add(currency, currencyValue);
                }
            }

            Log.WriteLog($"Currency map is: {JsonConvert.SerializeObject(result)}");

            return result;
        }
    }
}
