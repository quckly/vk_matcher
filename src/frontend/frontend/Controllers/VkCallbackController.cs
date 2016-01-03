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
        public async Task HandleAsync(HttpListenerRequest request, HttpListenerResponse responce)
        {
            string accessToken = null;
            var code = request.QueryString.Get("code");

            if (code == null)
            {
                await responce.ResponseErrorAsync(401);
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                string requestUrl = @"https://oauth.vk.com/access_token?client_id=5210253" +
                                    @"&client_secret=Kh4v7BmzWig4ROhNmQ8S" + // TODO: transfer to private config
                                    @"&redirect_uri=https://vk.quckly.ru/vk/callback" +
                                    @"&code=" + code;

                var vkHttpResponce = await client.GetAsync(requestUrl);
                var vkResponceString = await vkHttpResponce.Content.ReadAsStringAsync();

                var vkResponce = JObject.Parse(vkResponceString);

                JToken jTokenAccessToken;
                if (!vkResponce.TryGetValue("access_token", out jTokenAccessToken))
                {
                    await responce.ResponseErrorAsync(403);
                    return;
                }

                accessToken = jTokenAccessToken.Value<string>();
            }

            // Access token successfully taken.
            string taskID = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

            using (var command = DbConnection.SqlQuery("INSERT INTO task (uid, access_token) VALUES (@uid, @token)"))
            {
                command.Parameters.AddWithValue("@uid", taskID);
                command.Parameters.AddWithValue("@token", accessToken);
                await command.ExecuteNonQueryAsync();
            }

            // Return taskID to client
            responce.ResponseRedirect("https://vk.quckly.ru/#proccess/" + taskID);
        }
    }
}
