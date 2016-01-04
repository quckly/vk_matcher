using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace VKMatcher.Frontend
{
    static class ResponseExtension
    {
        public static void ResponseRedirect(this HttpListenerResponse response, string url, int code = 301)
        {
            response.RedirectLocation = url;
            response.StatusCode = code;
        }

        //public static void ResponseString(this HttpListenerResponse response, string text)
        //{
        //    byte[] buf = Encoding.UTF8.GetBytes(text);
        //    response.ContentLength64 = buf.Length;
        //    response.OutputStream.Write(buf, 0, buf.Length);
        //}

        //public static void ResponseError(this HttpListenerResponse response, int errorCode, string text = null)
        //{
        //    response.StatusCode = errorCode;

        //    byte[] buf = Encoding.UTF8.GetBytes($"Error {errorCode}<br />{(text == null ? string.Empty : text)}");
        //    response.ContentLength64 = buf.Length;
        //    response.OutputStream.Write(buf, 0, buf.Length);
        //}

        public async static Task ResponseStringAsync(this HttpListenerResponse response, string text)
        {
            byte[] buf = Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = buf.Length;
            await response.OutputStream.WriteAsync(buf, 0, buf.Length);
        }

        public async static Task ResponseErrorAsync(this HttpListenerResponse response, int errorCode, string text = null)
        {
            response.StatusCode = errorCode;

            byte[] buf = Encoding.UTF8.GetBytes($"Error {errorCode}<br />{(text == null ? string.Empty : text)}");
            response.ContentLength64 = buf.Length;
            await response.OutputStream.WriteAsync(buf, 0, buf.Length);
        }

        public async static Task ResponseJson(this HttpListenerResponse response, object resposeObject, Formatting formatting = Formatting.Indented)
        {
            string json = await Task.Run(() => JsonConvert.SerializeObject(resposeObject, formatting));
            await response.ResponseStringAsync(json);
        }
    }
}
