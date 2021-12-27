using Rocks.Wasabee.Mobile.Core.Infra.HttpClientFactory;
using System.Net.Http;

namespace Rocks.Wasabee.Mobile.Droid.Infra.HttpClientFactory
{
    public class Factory : IFactory
    {
        public HttpClientHandler CreateHandler()
        {
            var handler = new Xamarin.Android.Net.AndroidClientHandler();

            return handler;
        }
    }
}