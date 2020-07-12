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
        public const string STOCKX_URL = "https://www.stockx.com";
        public const string SIVASDESCALZO_URL = "https://sivasdescalzo.com";
        public const string REGEX_FOR_DOMAIN = @"^(?:https?:\/\/)?(?:[^@\/\n]+@)?(?:www\.)?([^:\/?\n]+)";
    }

    public static class SqlQueries
    {
        public const string GET_PRODUCTS_QUERY =
            @"Select p.product_id, o.product_option_id, p.location, m.name, pc.category_id
                from oc_product p
                    INNER JOIN oc_manufacturer m on p.manufacturer_id = m.manufacturer_id
                    INNER JOIN oc_product_to_category pc on p.product_id = pc.product_id
                    INNER JOIN oc_product_option o on p.product_id = o.product_id
                where location is not null
                    and location != """"
                    and pc.category_id in (59,60)";
        public const string UPDATE_PRODUCT_QUERY_BASE = 
            @"INSERT INTO oc_product_option_value(
                product_option_id,
                product_id,
                option_id,
                option_value_id,
                quantity,subtract,
                price,price_prefix,
                points,points_prefix,
                weight,weight_prefix,
                sku,model,o_v_image
            ) VALUES";
        public const string UPDATE_PRODUCT_COMMON_PRICE = "UPDATE oc_product SET price = {0} WHERE product_id = {1}";
        public const string DELETE_EXISTING_OPTION_VALUES = "DELETE FROM oc_product_option_value WHERE product_id = {0}";    
        public const string GET_DB_SIZES_MAP = "SELECT * FROM oc_option_value_description ORDER BY option_id DESC";
        public const string UPDATE_UNSUCCESSFUL_PRODUCTS = 
            @"UPDATE oc_product p inner join oc_product_option_value v on p.product_id = v.product_id
                SET p.quantity = 0,
                    v.quantity = 0,
                    v.price = 0,
                    p.price = 0,
                    stock_status_id = 8
                WHERE p.product_id IN ({0})";
        public const string UPDATE_PRODUCTS_ON_START= "UPDATE oc_product SET quantity = 999, stock_status_id = 7 WHERE product_id IN ({0})";
        public const string UPDATE_PRODUCTS_TOTAL_QUANTITY_ZERO = "UPDATE oc_product SET quantity = 0, stock_status_id = 8, price = 0 WHERE product_id = {0}";
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
        public double EU { get; set; }
        public double UK { get; set; }
        public double US { get; set; }
    }

}
