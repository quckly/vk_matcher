﻿using System;
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
        public static IController notFoundController = new PageNotFoundController();

        public static Dictionary<string, IController> routeControllers = new Dictionary<string, IController>()
        {
            // METHOD:URL_PATH_WITHOUT_SLASH_AT_END

            { "POST:/api/result", new ResultController() },
            { "GET:/vk/callback", new VkCallbackController() },
        };

        public async Task HandleAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            string urlPath = request.Url.AbsolutePath;

            if (urlPath.Length <= 0)
            {
                throw new Exception("Wrong request.");
            }

            string httpMethod = request.HttpMethod;

            if (request.HttpMethod == "OPTIONS")
            {
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                response.AddHeader("Access-Control-Max-Age", "1728000");
                response.AppendHeader("Access-Control-Allow-Origin", "*");

                return;
            }

            if (urlPath.Last() == '/')
            {
                urlPath = urlPath.Substring(0, urlPath.Length - 1);
            }

            // Find controller
            string controllerPath = httpMethod + ":" + urlPath;

            IController controller = null;
            if (!routeControllers.TryGetValue(controllerPath, out controller))
            {
                controller = notFoundController;
            }

            await controller.HandleAsync(request, response);
        }
    }
}
