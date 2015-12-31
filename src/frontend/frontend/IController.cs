using System.Net;

namespace VKMatcher.Frontend
{
    interface IController
    {
        void Handle(HttpListenerRequest request, HttpListenerResponse responce);
    }
}
