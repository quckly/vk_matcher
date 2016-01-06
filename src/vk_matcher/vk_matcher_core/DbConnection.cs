using System;
using System.Threading.Tasks;
using System.Data;

using MySql.Data.MySqlClient;

namespace VKMatcher.Core
{
    public sealed class DbConnection
    {
        #region Singleton implement
        private static volatile DbConnection instance;
        private static object syncRoot = new Object();

        public static DbConnection Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DbConnection();
                    }
                }

                return instance;
            }
        }
        #endregion

        MySqlConnectionStringBuilder mysqlCSB;
        //MySqlConnection connection;

        private DbConnection()
        {
            mysqlCSB = new MySqlConnectionStringBuilder();
            mysqlCSB.Server = "quckly.ru";
            mysqlCSB.Database = "vk_matcher";
            mysqlCSB.UserID = "vk_matcher";
            mysqlCSB.Password = "4Q51uC8f4kx2U3Vr2XL2";
            mysqlCSB.CharacterSet = "utf8";

            //connection = new MySqlConnection(mysqlCSB.ConnectionString);
            //connection.Open();
        }

        public static MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(Instance.mysqlCSB.ConnectionString);
            //connection.Open();
            return connection;
        }

        //public static async Task<MySqlConnection> GetConnectionAsync()
        //{
        //    var connection = new MySqlConnection(Instance.mysqlCSB.ConnectionString);
        //    await connection.OpenAsync();
        //    return connection;
        //}

        public static MySqlCommand SqlQuery(string query, MySqlConnection connection)
        {
            //MySqlCommand command = new MySqlCommand(query, Instance.connection);
            MySqlCommand command = new MySqlCommand(query, connection);

            return command;
        }

        public static MySqlCommand SqlProc(string procedure_name, MySqlConnection connection)
        {
            //MySqlCommand command = new MySqlCommand(procedure_name, Instance.connection);
            MySqlCommand command = new MySqlCommand(procedure_name, connection);

            command.CommandType = CommandType.StoredProcedure;
            return command;
        }

    }
}
