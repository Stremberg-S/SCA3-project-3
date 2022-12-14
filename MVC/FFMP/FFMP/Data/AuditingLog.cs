using System;
using System.Collections.Generic;

namespace FFMP.Data
{
    public partial class AuditingLog
    {
        public AuditingLog()
        {
            RequirementResults = new HashSet<RequirementResult>();
        }

        public uint Id { get; set; }
        public string UserLogin { get; set; } = null!;
        public uint ObjectId { get; set; }
        public DateTime? Created { get; set; }
        public string? Description { get; set; }
        public string? Result { get; set; }

        public virtual User UserLoginNavigation { get; set; } = null!;
        public virtual ObjectToCheck Object { get; set; } = null!;
        public virtual ICollection<RequirementResult> RequirementResults { get; set; }
    }
}
