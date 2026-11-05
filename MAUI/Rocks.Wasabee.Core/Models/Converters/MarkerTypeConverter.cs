using Rocks.Wasabee.Core.Models.Operations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.Models.Converters
{
    public class MarkerTypeConverter : JsonConverter<MarkerType>
    {
        public override void Write(Utf8JsonWriter writer, MarkerType value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override MarkerType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
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
    }
}
