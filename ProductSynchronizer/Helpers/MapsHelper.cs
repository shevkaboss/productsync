using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using ProductSynchronizer.Entities;

namespace ProductSynchronizer.Helpers
{
    public static class MapsHelper
    {
        private static readonly List<Brand> SizeMap;

        private static readonly Dictionary<Currency, double> CurrencyMap;
        static MapsHelper()
        {
            SizeMap =
                JsonConvert.DeserializeObject<List<Brand>>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),
                    @"Config\sizes.json")));
            CurrencyMap = GetCurrencyMap();
        }

        public static Dictionary<string, string> GetSizesMap(Resource resource, string brand, Gender gender)
        {
            var mapNode = SizeMap
                .First(x => x.Name == brand).MapsByGender
                .First(x => x.Gender == gender).MapNodes;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (resource)
            {
                case Resource.Footasylum:
                    return mapNode.ToDictionary(mn => mn.UK, mn => mn.EU);
                case Resource.Goat:
                case Resource.JimmyJazz:
                    return mapNode.ToDictionary(mn => mn.US, mn => mn.EU);
                default:
                    return null;
            }
        }

        public static double GetCurrencyValue(Resource resource)
        {
            switch (resource)
            {
                case Resource.Footasylum:
                    return CurrencyMap[Currency.USD];
                case Resource.Goat:
                case Resource.JimmyJazz:
                    return CurrencyMap[Currency.GBP];
                default:
                    return -1;
            }
        }

        public static double GetCurrencyValue(Currency currency)
        {
            return CurrencyMap[currency];
        }

        private static Dictionary<Currency, double> GetCurrencyMap()
        {
            var result = new Dictionary<Currency, double>();

            var response = HttpRequestHelper.PerformGetRequest(
                ConfigHelper.Config.CurrencyApiUrl);

            if (response == null)
            {
                Logger.Logger.WriteLog($"Currency request response is null");
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

            Logger.Logger.WriteLog($"Currency map is: {JsonConvert.SerializeObject(result)}");

            return result;
        }
    }
}
