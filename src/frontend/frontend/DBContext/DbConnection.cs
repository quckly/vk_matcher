using System;
using System.Data;

using MySql.Data.MySqlClient;

namespace VKMatcher.Frontend.DBContext
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

        MySqlConnection connection;

        private DbConnection()
        {
            var mysqlCSB = new MySqlConnectionStringBuilder();
            mysqlCSB.Server = "quckly.ru";
            mysqlCSB.Database = "vk_matcher";
            mysqlCSB.UserID = "vk_matcher";
            mysqlCSB.Password = "4Q51uC8f4kx2U3Vr2XL2";

            connection = new MySqlConnection(mysqlCSB.ConnectionString);
            connection.Open();
        }

        public static MySqlCommand SqlQuery(string query)
        {
            MySqlCommand command = new MySqlCommand(query, Instance.connection);
            return command;
        }

        public static MySqlCommand SqlProc(string procedure_name)
        {
            MySqlCommand command = new MySqlCommand(procedure_name, Instance.connection);
            command.CommandType = CommandType.StoredProcedure;
            return command;
        }

    }
}
