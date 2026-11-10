using System;
using System.Collections.Generic;
using System.Data.Common;
using OOP10FEXLab.Core.Models;

namespace OOP10FEXLab.Core.Data
{
    /// <summary>
    /// Database version of the meal source. Use this when Prof. Raymond gives the connection + query code.
    ///
    /// It accepts ANY ADO.NET connection (MySQL, SQL Server, Access...), so you only paste the connection line.
    /// Example (MySQL, after installing the MySql.Data NuGet package):
    ///
    ///   new DatabaseMealRepository(
    ///       () => new MySqlConnection("server=localhost;database=kiosk;uid=root;pwd=;"),
    ///       "SELECT code, name, category, price, inclusions FROM meals");
    ///
    /// The query must return the columns in this order: code, name, category, price, inclusions.
    /// </summary>
    public class DatabaseMealRepository : IMealRepository
    {
        private readonly Func<DbConnection> _connectionFactory;
        private readonly string _query;

        public DatabaseMealRepository(Func<DbConnection> connectionFactory, string query)
        {
            if (connectionFactory == null) throw new ArgumentNullException("connectionFactory");
            _connectionFactory = connectionFactory;
            _query = query;
        }

        public List<Meal> GetAll()
        {
            var meals = new List<Meal>();

            using (DbConnection conn = _connectionFactory())
            {
                conn.Open();
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = _query;
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                meals.Add(new Meal(
                                    Convert.ToString(reader[0]),
                                    Convert.ToString(reader[1]),
                                    Convert.ToString(reader[2]),
                                    Convert.ToDecimal(reader[3]),
                                    reader.FieldCount > 4 ? Convert.ToString(reader[4]) : ""));
                            }
                            catch (ArgumentException)
                            {
                                // skip invalid rows, same behaviour as the text file version
                            }
                        }
                    }
                }
            }

            return meals;
        }
    }
}
