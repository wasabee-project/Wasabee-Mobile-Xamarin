using Rocks.Wasabee.Core.Models.Operations;
using Rocks.Wasabee.Core.Resources.I18n;

namespace Rocks.Wasabee.Core.Extensions
{
    public static class MarkerTypeExtensions
    {
        public static string ToFriendlyString(this MarkerType markerType)
        {
            return markerType switch
            {
                MarkerType.DestroyPortal => Strings.MarkerType_Destroy,
                MarkerType.UseVirus => Strings.MarkerType_UseVirus,
                MarkerType.CapturePortal => Strings.MarkerType_Capture,
                MarkerType.FarmPortal => Strings.MarkerType_Farm,
                MarkerType.LetDecay => Strings.MarkerType_LetDecay,
                MarkerType.MeetAgent => Strings.MarkerType_MeetAgent,
                MarkerType.Other => Strings.MarkerType_Other,
                MarkerType.RechargePortal => Strings.MarkerType_Recharge,
                MarkerType.UpgradePortal => Strings.MarkerType_Upgrade,
                MarkerType.CreateLink => Strings.MarkerType_CreateLink,
                MarkerType.Exclude => Strings.MarkerType_Exclude,
                MarkerType.GetKey => Strings.MarkerType_GetKey,
                MarkerType.GoToPortal => Strings.MarkerType_GoTo,
                _ => throw new ArgumentOutOfRangeException(markerType.ToString())
            };
        }
    }
}