﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VKMatcher.Frontend.Controllers
{
    class PageNotFoundController : IController
    {
        public async Task HandleAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            response.StatusCode = 404;
            await response.ResponseStringAsync("<h1>Not found!</h1><br /><h2>Error 404</h2>");
        }
    }
}
