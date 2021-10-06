using DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public static class DatabaseService
    {
        private const string connString = "Server=DESKTOP-C4KM7DE;Database=Discord;Integrated Security=SSPI";

        public static List<Dictionary<string, object>> GetSQLResults(string sql)
        {
            var results = new List<Dictionary<string, object>>();

            try
            {
                using (var sqlConn = new SqlConnection(connString))
                {
                    using (var sqlComm = new SqlCommand(sql, sqlConn))
                    {
                        sqlConn.Open();

                        using (var reader = sqlComm.ExecuteReader())
                        {
                            while (reader.HasRows)
                            {
                                var dict = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var name = reader.GetName(i);
                                    var val = reader.GetValue(i);
                                    dict.Add(name, val);
                                }

                                results.Add(dict);

                                reader.Read();
                            }
                        }
                        
                        sqlConn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error getting sql results.", e);
            }

            return results;
        }

        public static int GetSQLCount(string tableName, string fieldName, string whereClause)
        {
            int count = 0;
            var sql = $"Select Count({fieldName}) From {tableName} WHERE {whereClause};";
            try
            {
                using (var sqlConn = new SqlConnection(connString))
                {
                    using (var sqlComm = new SqlCommand(sql, sqlConn))
                    {
                        sqlConn.Open();

                        count = (int)sqlComm.ExecuteScalar();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error getting sql count.", e);

            }

            return count;
        }

        public static int InsertSQL(string sql)
        {
            int rowsAffected = 0;

            try
            {
                using (var sqlConn = new SqlConnection(connString))
                {
                    using (var sqlComm = new SqlCommand(sql, sqlConn))
                    {
                        sqlConn.Open();

                        rowsAffected = sqlComm.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error inserting sql.", e);

            }

            return rowsAffected;
        }

    }
}
