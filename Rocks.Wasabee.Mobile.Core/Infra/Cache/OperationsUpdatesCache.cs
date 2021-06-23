using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Infra.Cache
{
    public static class OperationsUpdatesCache
    {
        private static Dictionary<string, bool> _data = new Dictionary<string, bool>();
        public static Dictionary<string, bool> Data => _data;
    }
}