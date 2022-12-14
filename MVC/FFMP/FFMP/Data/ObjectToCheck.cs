using System;
using System.Collections.Generic;

namespace FFMP.Data
{
    public partial class ObjectToCheck
    {
        public ObjectToCheck()
        {
            AuditingLogs = new HashSet<AuditingLog>();
            Inspections = new HashSet<Inspection>();
        }

        public uint Id { get; set; }
        public string UserLogin { get; set; } = null!;
        public uint TargetGroupId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Location { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Model { get; set; } = null!;
        public bool? State { get; set; }
        public DateTime Created { get; set; }

        public virtual User UserLoginNavigation { get; set; } = null!;
        public virtual TargetGroup TargetGroup { get; set; } = null!;
        public virtual ICollection<AuditingLog> AuditingLogs { get; set; }
        public virtual ICollection<Inspection> Inspections { get; set; }
    }
}
