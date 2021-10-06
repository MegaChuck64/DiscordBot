using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ExchangeBot.Services
{
    public class DBService
    {
        private const string connString = "Server=DESKTOP-C4KM7DE;Database=Discord;Integrated Security=SSPI";
        public List<Dictionary<string, object>> GetSQLResults(string sql)
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


                            while (reader.Read())
                            {

                                var dict = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var name = reader.GetName(i);
                                    var val = reader.GetValue(i);
                                    dict.Add(name, val);
                                }

                                results.Add(dict);
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

        public int GetSQLCount(string tableName, string fieldName, string whereClause)
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

        public int InsertSQL(string sql)
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
