using Rocks.Wasabee.Mobile.Core.Infra.HttpClientFactory;
using System.Net;
using System.Net.Http;

namespace Rocks.Wasabee.Mobile.iOS.Infra.HttpClientFactory
{
    public class Factory : IFactory
    {
        public HttpClientHandler CreateHandler(CookieContainer cookieContainer = null)
        {
            var handler = new HttpClientHandler();
            if (cookieContainer != null)
                handler.CookieContainer = cookieContainer;
            
            return handler;
        }
    }
}