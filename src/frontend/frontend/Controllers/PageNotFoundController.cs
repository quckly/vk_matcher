using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VKMatcher.Frontend.Controllers
{
    class PageNotFoundController : IController
    {
        public async Task HandleAsync(HttpListenerRequest request, HttpListenerResponse responce)
        {
            responce.StatusCode = 404;
            await responce.ResponseStringAsync("<h1>Not found!</h1><br /><h4>Error 404</h4>");
        }
    }
}
