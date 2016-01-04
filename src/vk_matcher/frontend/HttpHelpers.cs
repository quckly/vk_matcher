using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using Newtonsoft.Json;

namespace VKMatcher.Frontend
{
    public class HttpHelpers
    {
        public static async Task<string> ReadStringRequest(HttpListenerRequest request)
        {
            if (request.InputStream.CanRead)
            {
                using (var ms = new MemoryStream())
                {
                    await request.InputStream.CopyToAsync(ms);

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }

            return null;
        }

        public static async Task<T> ReadJsonRequest<T>(HttpListenerRequest request)
        {
            var requestString = await ReadStringRequest(request);

            return await Task.Run(() => JsonConvert.DeserializeObject<T>(requestString));
        }
    }
}
