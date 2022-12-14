using System;
using System.Collections.Generic;

namespace FFMP.Data
{
    public partial class RequirementResult
    {
        public uint RequirementId { get; set; }
        public uint AuditingLogsId { get; set; }
        public string Description { get; set; } = null!;
        public bool Must { get; set; }
        public bool? Result { get; set; }

        public virtual AuditingLog AuditingLogs { get; set; } = null!;
    }
}
