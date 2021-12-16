using System.Net.Http;

namespace Rocks.Wasabee.Mobile.Core.Infra.HttpClientFactory
{
    public interface IFactory
    {
        public HttpClientHandler CreateHandler();
    }
}