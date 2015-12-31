using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VKMatcher.Frontend.Controllers;

namespace VKMatcher.Frontend
{
    class FrontendRouter
    {
        public static Dictionary<string, IController> routeControllers = new Dictionary<string, IController>()
        {
            { "POST:api/findmatch", new FindMatchController() }
        };

        public void Handle(HttpListenerRequest request, HttpListenerResponse responce)
        {
            string urlPath = request.Url.Fragment;

            string controllerPath = request.HttpMethod + ":" + urlPath;
        }
    }
}
