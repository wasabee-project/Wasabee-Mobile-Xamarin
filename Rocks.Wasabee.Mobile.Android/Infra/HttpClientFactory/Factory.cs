using Rocks.Wasabee.Mobile.Core.Infra.HttpClientFactory;
using System.Net;
using System.Net.Http;

namespace Rocks.Wasabee.Mobile.Droid.Infra.HttpClientFactory
{
    public class Factory : IFactory
    {
        public HttpClientHandler CreateHandler(CookieContainer cookieContainer = null)
        {
            var handler = new Xamarin.Android.Net.AndroidClientHandler();
            if (cookieContainer != null)
                handler.CookieContainer = cookieContainer;
            
            return handler;
        }
    }
}