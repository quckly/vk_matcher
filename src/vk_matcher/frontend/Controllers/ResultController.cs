using System.Net;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

using VKMatcher.Frontend.DBContext;

namespace VKMatcher.Frontend.Controllers
{
    class ResultRequest
    {
        public string taskId = null;
    }

    class ResultController : IController
    {
        public async Task HandleAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            var req = await HttpHelpers.ReadJsonRequest<ResultRequest>(request);

            if (req == null || req.taskId == null)
            {
                await response.ResponseErrorAsync(401);
                return;
            }

            using (var connection = DbConnection.GetConnection())
            {
                await connection.OpenAsync();

                using (MySqlCommand queryGetTask = DbConnection.SqlQuery(@"SELECT response, responsed FROM task WHERE uid = @uid LIMIT 1", connection))
                {
                    queryGetTask.Parameters.AddWithValue("@uid", req.taskId);

                    using (var reader = await queryGetTask.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            bool isResponsed = reader.GetBoolean(1);

                            if (isResponsed)
                            {
                                string taskResponse = reader.GetString(0);

                                await response.ResponseJsonStringAsync(taskResponse);
                            }
                            else
                            {
                                response.StatusCode = 204;
                            }
                        }
                        else
                        {
                            await response.ResponseJsonAsync(new { error = "Invalid task id." });
                        }
                    }
                }
            }
        }
    }
}
