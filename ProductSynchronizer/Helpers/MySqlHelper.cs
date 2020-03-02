using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace ProductSynchronizer.Helpers
{
    public class MySqlHelper
    {
        private static readonly MySqlConnection MyConnection;
        static MySqlHelper()
        {
            MyConnection = new MySqlConnection(ConfigHelper.Config.ConnectionString);
            MyConnection.Open();
        }
        public static IEnumerable<Product> GetProducts()
        {
            var reader = GetTypedData(SqlQueries.GET_PRODUCTS_QUERY);

            var products = new List<Product>();

            while (reader.Read())
            {
                products.Add(new Product
                {
                    InternalId = (int)reader["product_id"],
                    Location = (string)reader["location"],
                    Brand = (string)reader["name"],
                    Gender = (Gender)reader["category_id"]
                });
            }

            return products;
        }
        public static void UpdateProduct(Product product)
        {
            var query = string.Format(SqlQueries.UPDATE_PRODUCT_QUERY_BASE);
            ExecuteQuery(query);
        }
        public static MySqlDataReader ExecuteQuery(string query)
        {
            var sqlCommand = new MySqlCommand(query, MyConnection);
            return sqlCommand.ExecuteReader();
        }
        public static void Dispose()
        {
            MyConnection.Dispose();
        }
        private static MySqlDataReader GetTypedData(string query)
        {
            return ExecuteQuery(query);
        }
    }
}
