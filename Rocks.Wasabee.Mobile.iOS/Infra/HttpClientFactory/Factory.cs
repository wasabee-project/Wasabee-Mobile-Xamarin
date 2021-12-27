using Rocks.Wasabee.Mobile.Core.Infra.HttpClientFactory;
using System.Net.Http;

namespace Rocks.Wasabee.Mobile.iOS.Infra.HttpClientFactory
{
    public class Factory : IFactory
    {
        public HttpClientHandler CreateHandler()
        {
            var handler = new HttpClientHandler();

            return handler;
        }
    }
}