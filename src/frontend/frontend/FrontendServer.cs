using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKMatcher.Frontend
{
    class FrontendServer
    {
        WebServer ws;

        public FrontendServer()
        {
            ws = new WebServer(RequestHandler, "http://*:9010/");
        }

        public void Run()
        {
            ws.Run();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void RequestHandler(HttpListenerRequest request, HttpListenerResponse responce)
        {
            responce.ResponseString(
                string.Format("<HTML><BODY><h1>VK Matcher </h1><br />Page genarated by c# mono.<br />{0}</BODY></HTML>", DateTime.Now));
        }
    }
}
