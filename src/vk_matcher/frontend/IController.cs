using System.Net;
using System.Threading.Tasks;

namespace VKMatcher.Frontend
{
    interface IController
    {
        Task HandleAsync(HttpListenerRequest request, HttpListenerResponse response);
    }
}
