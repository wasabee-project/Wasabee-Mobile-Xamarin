using Rocks.Wasabee.Core.Models.Operations;

namespace Rocks.Wasabee.Core.ViewModels.Operation.Assignments
{
    public abstract class AssignmentData
    {
        public string OpId { get; }
        public int Order { get; }

        protected AssignmentData(string opId, int order)
        {
            OpId = opId;
            Order = order;
        }

        public LinkModel? Link { get; set; }
        public MarkerModel? Marker { get; set; }

        public string Assignments { get; set; } = string.Empty;
        public bool ShowAssignee { get; set; } = false;
    }
}