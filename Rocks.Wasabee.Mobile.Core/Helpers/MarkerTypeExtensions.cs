using Rocks.Wasabee.Mobile.Core.Helpers.Xaml;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using System;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class MarkerTypeExtensions
    {
        public static string ToFriendlyString(this MarkerType markerType)
        {
            return markerType switch
            {
                MarkerType.DestroyPortal => TranslateExtension.GetValue("MarkerType_Destroy"),
                MarkerType.UseVirus => TranslateExtension.GetValue("MarkerType_UseVirus"),
                MarkerType.CapturePortal => TranslateExtension.GetValue("MarkerType_Capture"),
                MarkerType.FarmPortal => TranslateExtension.GetValue("MarkerType_Farm"),
                MarkerType.LetDecay => TranslateExtension.GetValue("MarkerType_LetDecay"),
                MarkerType.MeetAgent => TranslateExtension.GetValue("MarkerType_MeetAgent"),
                MarkerType.Other => TranslateExtension.GetValue("MarkerType_Other"),
                MarkerType.RechargePortal => TranslateExtension.GetValue("MarkerType_Recharge"),
                MarkerType.UpgradePortal => TranslateExtension.GetValue("MarkerType_Upgrade"),
                MarkerType.CreateLink => TranslateExtension.GetValue("MarkerType_CreateLink"),
                MarkerType.Exclude => TranslateExtension.GetValue("MarkerType_Exclude"),
                MarkerType.GetKey => TranslateExtension.GetValue("MarkerType_GetKey"),
                MarkerType.GoToPortal => TranslateExtension.GetValue("MarkerType_GoTo"),
                _ => throw new ArgumentOutOfRangeException(markerType.ToString())
            };
        }
    }
}