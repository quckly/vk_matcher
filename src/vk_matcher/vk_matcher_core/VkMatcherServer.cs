using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using VkNet;
using VkNet.Enums.Filters;
using MySql.Data.MySqlClient;
using QUtils;

namespace VKMatcher.Core
{
    public class VkMatcherServer
    {
        MultithreadingServer mts;
        QLogger logger;

        public VkMatcherServer(QLogger logger)
        {
            this.logger = logger;
            mts = new MultithreadingServer(DoWork, 1);
        }

        public void Run()
        {
            mts.Run();
        }

        string CalcVkApiMatching(string accessToken, uint userId)
        {
            try
            {
                VkApi vkApi = new VkApi();
                vkApi.Authorize(accessToken, userId);

                return JsonConvert.SerializeObject(vkApi.Users.Get(userId, ProfileFields.All));
            }
            catch {
                return null;
            }
        }

        private MySqlCommand getQueryGetFreeTask()
        {
            return DbConnection.SqlQuery(@"SELECT id, user_id, access_token FROM task WHERE responsed = 0 LIMIT 1", DbConnection.GetConnection());
        }

        private MySqlCommand getQuerySetResponse()
        {
            var querySetResponse =  DbConnection.SqlQuery(@"UPDATE task SET response = @response, responsed = 1 WHERE id = @id", DbConnection.GetConnection());

            querySetResponse.Parameters.Add("@id", MySqlDbType.UInt32);
            querySetResponse.Parameters.Add("@response", MySqlDbType.MediumText);

            return querySetResponse;
        }

        void DoWork()
        {
            MySqlCommand queryGetFreeTask = null, querySetResponse = null;

            try
            {
                Random rand = new Random();
                queryGetFreeTask = getQueryGetFreeTask();
                queryGetFreeTask.Connection.Open();
                querySetResponse = getQuerySetResponse();
                querySetResponse.Connection.Open();

                int idleTime = 0;

                while (true)
                {
                    if (queryGetFreeTask.Connection.State != System.Data.ConnectionState.Open)
                    {
                        queryGetFreeTask = getQueryGetFreeTask();
                        queryGetFreeTask.Connection.Open();
                    }

                    using (var reader = queryGetFreeTask.ExecuteReader())
                    {
                        if (reader.Read()) // if - because only one line in result, otherwise must use while
                        {
                            int id = reader.GetInt32("id");
                            uint user_id = reader.GetUInt32("user_id");
                            string access_token = reader.GetString("access_token");

                            string response = CalcVkApiMatching(access_token, user_id);

                            if (response == null)
                            {
                                // log it, set record error state
                                continue;
                            }

                            if (querySetResponse.Connection.State != System.Data.ConnectionState.Open)
                            {
                                querySetResponse = getQuerySetResponse();
                                querySetResponse.Connection.Open();
                            }

                            querySetResponse.Parameters["@id"].Value = id;
                            querySetResponse.Parameters["@response"].Value = response;
                            querySetResponse.ExecuteNonQuery();

                            idleTime = 0;
                        }
                        else
                        {
                            // If no result in select don't spam a DB for nothing.
                            idleTime = Math.Min(idleTime + 90, 2000);
                            Thread.Sleep(idleTime + rand.Next(20));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(e.Message);
            }
            finally
            {
                if (queryGetFreeTask != null)
                {
                    queryGetFreeTask.Dispose();
                    queryGetFreeTask.Connection.Dispose();
                }

                if (querySetResponse != null)
                {
                    querySetResponse.Dispose();
                    querySetResponse.Connection.Dispose();
                }
            }
        }
    }
}
