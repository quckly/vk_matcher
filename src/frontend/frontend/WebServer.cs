using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace VKMatcher.Frontend
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, HttpListenerResponse, Task> _responderMethod;

        public WebServer(string[] prefixes, Func<HttpListenerRequest, HttpListenerResponse, Task> method)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (method == null)
                throw new ArgumentException("method");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = method;
            _listener.Start();
        }

        public WebServer(Func<HttpListenerRequest, HttpListenerResponse, Task> method, params string[] prefixes)
            : this(prefixes, method)
        { }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        await _listener.GetContextAsync().ContinueWith(async (t) =>
                        {
                            var ctx = await t;

                            try
                            {
                                await _responderMethod(ctx.Request, ctx.Response);
                                return;
                            }
                            catch (Exception e) {
                                ctx.Response.ResponseError(500, e.Message); // Remove me in Production
                            } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        });
                        //ThreadPool.QueueUserWorkItem((c) =>
                        //{
                        //    var ctx = c as HttpListenerContext;
                        //    try
                        //    {
                        //        _responderMethod(ctx.Request, ctx.Response);
                        //    }
                        //    catch { } // suppress any exceptions
                        //    finally
                        //    {
                        //        // always close the stream
                        //        ctx.Response.OutputStream.Close();
                        //    }
                        //}, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
