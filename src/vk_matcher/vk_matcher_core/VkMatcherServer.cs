using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using VkNet;
using VkNet.Enums.Filters;
using MySql.Data.MySqlClient;

namespace VKMatcher.Core
{
    public class VkMatcherServer
    {
        MultithreadingServer mts;

        public VkMatcherServer()
        {
            mts = new MultithreadingServer(DoWork);
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

        void DoWork()
        {
            MySqlCommand queryGetFreeTask = null, querySetResponse = null;

            try
            {
                Random rand = new Random();
                queryGetFreeTask = DbConnection.SqlQuery(@"SELECT id, user_id, access_token FROM task WHERE response IS NULL LIMIT 1");
                querySetResponse = DbConnection.SqlQuery(@"UPDATE task SET response = @response WHERE id = @id");

                querySetResponse.Parameters.Add("@id", MySqlDbType.UInt32);
                querySetResponse.Parameters.Add("@response", MySqlDbType.MediumText);

                while (true)
                {
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

                            querySetResponse.Parameters["@id"].Value = id;
                            querySetResponse.Parameters["@response"].Value = response;
                            querySetResponse.ExecuteNonQuery();
                        }
                        else
                        {
                            // If no result in select don't spam a DB for nothing.
                            Thread.Sleep(90 + rand.Next(20));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // TODO:
                // Log it
            }
            finally
            {
                if (queryGetFreeTask != null)
                {
                    queryGetFreeTask.Dispose();
                }

                if (querySetResponse != null)
                {
                    querySetResponse.Dispose();
                }
            }
        }
    }
}
