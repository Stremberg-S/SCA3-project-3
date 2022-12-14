using System;
using System.Collections.Generic;

namespace FFMP.Data
{
    public partial class User
    {
        public User()
        {
            AuditingForms = new HashSet<AuditingForm>();
            AuditingLogs = new HashSet<AuditingLog>();
            Inspections = new HashSet<Inspection>();
            ObjectToChecks = new HashSet<ObjectToCheck>();
        }

        public string Name { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime? Created { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool? Admin { get; set; }
        public bool? Active { get; set; }

        public virtual ICollection<AuditingForm> AuditingForms { get; set; }
        public virtual ICollection<AuditingLog> AuditingLogs { get; set; }
        public virtual ICollection<Inspection> Inspections { get; set; }
        public virtual ICollection<ObjectToCheck> ObjectToChecks { get; set; }
    }
}
