using FFMP.Data;

namespace FFMP.Models
{
    public class InspectionModel
    {
        public uint Id { get; set; }
        public string UserLogin { get; set; } = null!;
        public uint ObjectId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Reason { get; set; } = null!;
        public string? Observations { get; set; }
        public bool? ChangeOfState { get; set; }
        public string? Inspectioncol { get; set; }

        public virtual ObjectToCheck? Object { get; set; }
        public virtual User? UserLoginNavigation { get; set; }
    }
}
