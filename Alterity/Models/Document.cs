using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class Document
    {
        public int Id { get; set; }
        public virtual ICollection<EditOperation> Edits { get; set; }
        public UserAccount Owner { get; set; }
        public virtual ICollection<UserAccount> Administrators { get; set; }
        public virtual ICollection<UserAccount> Moderators { get; set; }
        public bool Locked { get; set; }
    }
}