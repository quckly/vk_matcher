using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VKMatcher.Frontend.Controllers
{
    class VkCallbackController : IController
    {
        public async Task HandleAsync(HttpListenerRequest request, HttpListenerResponse responce)
        {
            var code = request.QueryString.Get("code");

            if (code == null)
            {
                await responce.ResponseErrorAsync(401);
                return;
            }

            
        }
    }
}
