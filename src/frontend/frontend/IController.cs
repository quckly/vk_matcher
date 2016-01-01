using System.Net;
using System.Threading.Tasks;

namespace VKMatcher.Frontend
{
    interface IController
    {
        Task Handle(HttpListenerRequest request, HttpListenerResponse responce);
    }
}
