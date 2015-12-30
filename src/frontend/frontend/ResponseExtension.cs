using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKMatcher.Frontend
{
    static class ResponseExtension
    {
        public static void ResponseString(this HttpListenerResponse response, string text)
        {
            byte[] buf = Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = buf.Length;
            response.OutputStream.Write(buf, 0, buf.Length);
        }
    }
}
