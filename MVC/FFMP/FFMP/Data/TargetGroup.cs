using System;
using System.Collections.Generic;

namespace FFMP.Data
{
    public partial class TargetGroup
    {
        public TargetGroup()
        {
            AuditingForms = new HashSet<AuditingForm>();
            ObjectToChecks = new HashSet<ObjectToCheck>();
        }

        public uint Id { get; set; }
        public string Description { get; set; } = null!;

        public virtual ICollection<AuditingForm> AuditingForms { get; set; }
        public virtual ICollection<ObjectToCheck> ObjectToChecks { get; set; }
    }
}
