using Rocks.Wasabee.Core.Models.Operations;

namespace Rocks.Wasabee.Core.ViewModels.Operation.Assignments
{
    public class LinkAssignmentData : AssignmentData
    {
        public LinkAssignmentData(string opId, int order) : base(opId, order)
        {

        }

        public PortalModel? FromPortal { get; set; }
        public PortalModel? ToPortal { get; set; }

        public string FromPortalName => FromPortal?.Name ?? FromPortal?.Id ?? string.Empty;
        public string ToPortalName => ToPortal?.Name ?? ToPortal?.Id ?? string.Empty;

        public Color Color { get; set; }
    }
}