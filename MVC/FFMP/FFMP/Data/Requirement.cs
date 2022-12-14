using System;
using System.Collections.Generic;

namespace FFMP.Data
{
    public partial class Requirement
    {
        public uint ReqId { get; set; }
        public uint AuditingAuditingId { get; set; }
        public string Description { get; set; } = null!;
        public bool Must { get; set; }

        public virtual AuditingForm AuditingAuditing { get; set; } = null!;
    }
}
