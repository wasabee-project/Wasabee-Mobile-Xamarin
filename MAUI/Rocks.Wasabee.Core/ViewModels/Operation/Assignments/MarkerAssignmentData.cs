using CommunityToolkit.Mvvm.Input;
using Rocks.Wasabee.Core.Models.Operations;

namespace Rocks.Wasabee.Core.ViewModels.Operation.Assignments
{
    public partial class MarkerAssignmentData : AssignmentData
    {
        public MarkerAssignmentData(string opId, int order) : base(opId, order)
        {

        }

        public PortalModel? Portal { get; set; }

        public string PortalName => Portal?.Name ?? Portal?.Id ?? string.Empty;

        [RelayCommand]
        public void ShowMarker()
        {
            if (Marker != null)
            {
                //Mvx.IoCProvider.Resolve<IMvxMessenger>().Publish(new ShowMarkerOnMapMessage(this, Marker));
            }
        }
    }
}