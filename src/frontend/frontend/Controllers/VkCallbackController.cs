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
        public async Task Handle(HttpListenerRequest request, HttpListenerResponse responce)
        {
            await Task.Run(() =>
            {
                var code = request.QueryString.Get("code");

                if (code == null)
                {
                    responce.ResponseError(401);
                    return;
                }


            });
        }
    }
}
