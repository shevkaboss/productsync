using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace ProductSynchronizer.Helpers
{
    public class MySqlHelper : IDisposable
    {
        private readonly MySqlConnection myConnection;
        public MySqlHelper()
        {
            myConnection = new MySqlConnection("Server=werare.mysql.tools;database=werare_db;UID=werare_db;password=vAaQe4hH");
            myConnection.Open();
        }
        private MySqlDataReader GetTypedData(string query)
        {
            return ExecuteQuery(query);
        }

        public IEnumerable<Product> GetProducts()
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

        public MySqlDataReader ExecuteQuery(string query)
        {
            var sqlCommand = new MySqlCommand(query, myConnection);
            return sqlCommand.ExecuteReader();
        }

        public void Dispose()
        {
            myConnection.Dispose();
        }
    }
}
