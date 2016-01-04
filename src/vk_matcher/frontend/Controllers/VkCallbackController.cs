using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using VKMatcher.Frontend.DBContext;

namespace VKMatcher.Frontend.Controllers
{
    class VkCallbackController : IController
    {
        public async Task HandleAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            string accessToken = null;
            uint userId = 0;

            var code = request.QueryString.Get("code");

            if (code == null)
            {
                await response.ResponseErrorAsync(401);
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                string requestUrl = @"https://oauth.vk.com/access_token?client_id=5210253" +
                                    @"&client_secret=Kh4v7BmzWig4ROhNmQ8S" + // TODO: transfer to private config
                                    @"&redirect_uri=https://vk.quckly.ru/vk/callback" +
                                    @"&code=" + code;

                var vkHttpResponse = await client.GetAsync(requestUrl);
                var vkResponseString = await vkHttpResponse.Content.ReadAsStringAsync();

                try {
                    dynamic vkResponse = JObject.Parse(vkResponseString);

                    userId = vkResponse.user_id;
                    accessToken = vkResponse.access_token;

                    //JToken jTokenAccessToken;
                    //if (!vkResponse.TryGetValue("access_token", out jTokenAccessToken))
                    //{
                    //    await response.ResponseErrorAsync(403);
                    //    return;
                    //}

                    //accessToken = jTokenAccessToken.Value<string>();
                }
                catch {
                    await response.ResponseErrorAsync(403);
                    return;
                }
            }

            // Access token successfully taken.
            string taskId = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

            using (var command = DbConnection.SqlQuery("INSERT INTO task (uid, user_id, access_token) VALUES (@uid, @user_id, @token)"))
            {
                command.Parameters.AddWithValue("@uid", taskId);
                command.Parameters.AddWithValue("@user_id", userId);
                command.Parameters.AddWithValue("@token", accessToken);
                await command.ExecuteNonQueryAsync();
            }

            // Return taskID to client
            response.ResponseRedirect("https://vk.quckly.ru/#/wait/" + taskId);
        }
    }
}
