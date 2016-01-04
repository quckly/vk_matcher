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
            
            using (MySqlCommand queryGetTask = DbConnection.SqlQuery(@"SELECT response FROM task WHERE uid = @uid AND response IS NOT NULL LIMIT 1"))
            {
                queryGetTask.Parameters.AddWithValue("@uid", req.taskId);

                using (var reader = await queryGetTask.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        string taskResponse = reader.GetString(0);

                        await response.ResponseStringAsync(taskResponse);
                    }
                    else
                    {
                        response.StatusCode = 204;
                    }
                }
            }
        }
    }
}
