using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ProductSynchronizer
{
    public static class Constants
    {
        public const string GOAT_URL = "https://www.goat.com";
        public const string FOOTASYLUM_URL = "https://www.footasylum.com";
        public const string JIMMY_JAZZ_URL = "https://www.jimmyjazz.com";
        public const string REGEX_FOR_DOMAIN = @"^(?:https?:\/\/)?(?:[^@\/\n]+@)?(?:www\.)?([^:\/?\n]+)";
    }

    public static class SqlQueries
    {
        public const string GET_PRODUCTS_QUERY =
            "Select p.product_id, p.location, m.name, pc.category_id from oc_product p INNER JOIN oc_manufacturer m on p.manufacturer_id = m.manufacturer_id INNER JOIN oc_product_to_category pc on p.product_id = pc.product_id where location is not null and location != \"\" and pc.category_id in (59,60)";

        public const string UPDATE_PRODUCT_QUERY_BASE = "";
    }

    public class Brand
    {
        public string Name { get; set; }
        public Map[] MapsByGender { get; set; }
    }

    public class Map
    {
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public Gender Gender { get; set; }
        public MapNode[] MapNodes { get; set; }
    }

    public class MapNode
    {
        public string EU { get; set; }
        public string UK { get; set; }
        public string US { get; set; }
    }

}
