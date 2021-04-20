using MySql.Data.MySqlClient;
using ProductSynchronizer.Logger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace ProductSynchronizer.Helpers
{
    public class MySqlHelper
    {
        public static IEnumerable<Product> GetProducts()
        {
            var dataSet = ExecuteReadQuery(SqlQueries.GET_PRODUCTS_QUERY);

            var products = new List<Product>();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                var newProduct = (new Product
                {
                    InternalId = (int)row["product_id"],
                    ProductOptionId = (int)row["product_option_id"],
                    Location = (string)row["location"],
                    Brand = (string)row["name"],
                    Gender = (Gender)row["category_id"]
                });

                var existingProd = products.FirstOrDefault(x => x.InternalId == newProduct.InternalId && x.Gender == Gender.Female);

                if (existingProd != null && newProduct.Gender == Gender.Male)
                {
                    existingProd.Gender = Gender.Male;
                    continue;
                };

                products.Add(newProduct);
            }
            return products;
        }
        public static void UpdateProduct(Product product)
        {
            var updateCommonPriceQuery = string.Format(SqlQueries.UPDATE_PRODUCT_COMMON_PRICE, product.CommonPrice.ToString(CultureInfo.InvariantCulture), product.InternalId);
            ExecuteNonReadQuery(updateCommonPriceQuery);

            var cleanQuery = string.Format(SqlQueries.DELETE_EXISTING_OPTION_VALUES, product.InternalId);
            ExecuteNonReadQuery(cleanQuery);

            var updateQuery = string.Format(SqlQueries.UPDATE_PRODUCT_QUERY_BASE);

            var insertValues = new List<List<string>>();
            foreach (var n in product.ShoesSizeMap)
            {
                var values = new List<string>
                {
                    product.ProductOptionId.ToString(),
                    product.InternalId.ToString(),
                    "11",
                    n.OptionValueId.ToString(),
                    n.Quantity.ToString(),
                    "1",
                    n.InternalPrice.ToString(CultureInfo.InvariantCulture),
                    n.InternalPrice == 0? "\"+\"":"\"=\"",
                    "0",
                    "\"+\"",
                    "0",
                    "\"+\"",
                    "\"\"",
                    "\"\"",
                    "\"\""
                };
                insertValues.Add(values);

            }

            var valueQuery = string.Join(",", insertValues.Select(x => $"({string.Join(",", x)})"));

            updateQuery += valueQuery;
            ExecuteNonReadQuery(updateQuery);

            //Set quantity = 0 if no shoes available.
            if (!product.ShoesSizeMap.Any(x => x.Quantity > 0))
            {
                var updateTotalQuantityIfZeroQuery = string.Format(SqlQueries.UPDATE_PRODUCTS_TOTAL_QUANTITY_ZERO, product.InternalId);
                ExecuteNonReadQuery(updateTotalQuantityIfZeroQuery);
            }
        }
        public static Dictionary<int, string> GetDbSizesMap()
        {
            var query = string.Format(SqlQueries.GET_DB_SIZES_MAP);
            var dataSet = ExecuteReadQuery(query);

            var result = new Dictionary<int, string>();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                try
                {
                    var id = (int)row["option_value_id"];
                    var value = ((string)row["name"]).ToLower().Trim();

                    result.Add(id, value);
                }
                catch
                {
                }
            }
            return result;

        }
        public static void UpdateUnsuccessfulProducts(IEnumerable<string> ids)
        {
            var values = ids.ToList();
            if (!values.Any())
                return;

            Log.WriteLog($"Errors to update {values.Count()}");

            var idsString = string.Join(",", values);

            Log.WriteLog($"Updating unsuccessful products: {idsString}");

            var query = string.Format(SqlQueries.UPDATE_UNSUCCESSFUL_PRODUCTS, idsString);
            ExecuteNonReadQuery(query);
        }
        public static void UpdateProductsOnStart(IEnumerable<string> ids)
        {
            var values = ids.ToList();
            if (!values.Any())
                return;

            var idsString = string.Join(",", values);

            Log.WriteLog($"Updating products on start: {idsString}");

            var query = string.Format(SqlQueries.UPDATE_PRODUCTS_ON_START, idsString);
            ExecuteNonReadQuery(query);
        }
        public static DataSet ExecuteReadQuery(string query)
        {
            using (var conn = new MySqlConnection(ConfigHelper.Config.ConnectionString))
            {
                conn.Open();
                var dataSet = new DataSet();
                Log.WriteLog($"Executing read query {query}");
                var sqlCommand = new MySqlCommand(query, conn);
                var adapter = new MySqlDataAdapter { SelectCommand = sqlCommand };
                adapter.Fill(dataSet);
                return dataSet;
            }
        }
        public static void ExecuteNonReadQuery(string query)
        {
            using (var conn = new MySqlConnection(ConfigHelper.Config.ConnectionString))
            {
                conn.Open();
                Log.WriteLog($"Executing non-read query {query}");
                var sqlCommand = new MySqlCommand(query, conn);
                sqlCommand.ExecuteNonQuery();
            }
        }
    }
}
