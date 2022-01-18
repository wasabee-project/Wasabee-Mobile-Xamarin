using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using System;

namespace Rocks.Wasabee.Mobile.Core.Models.JsonConverters
{
    public class MarkerTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
                return MarkerType.Other;

            return value switch
            {
                "DestroyPortalAlert" => MarkerType.DestroyPortal,
                "UseVirusPortalAlert" => MarkerType.UseVirus,
                "CapturePortalMarker" => MarkerType.CapturePortal,
                "FarmPortalMarker" => MarkerType.FarmPortal,
                "LetDecayPortalAlert" => MarkerType.LetDecay,
                "MeetAgentPortalMarker" => MarkerType.MeetAgent,
                "OtherPortalAlert" => MarkerType.Other,
                "RechargePortalAlert" => MarkerType.RechargePortal,
                "UpgradePortalAlert" => MarkerType.UpgradePortal,
                "CreateLinkAlert" => MarkerType.CreateLink,
                "ExcludeMarker" => MarkerType.Exclude,
                "GetKeyPortalMarker" => MarkerType.GetKey,
                "GotoPortalMarker" => MarkerType.GoToPortal,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}