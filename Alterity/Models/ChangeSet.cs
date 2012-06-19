using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Alterity.Models
{
    public class ChangeSet
    {
        public int Id { get; set; }
        public User Owner { get; set; }
        public VoteBox VoteBox { get; set; }
        public DateTime LastModified { get; set; }
        public Document Document { get; set; }
        public virtual ICollection<ChangeSubset> ChangeSubsets { get; set; }

        public static ChangeSet Create(User user, Document document)
        {
            ChangeSet changeSet = new ChangeSet();
            changeSet.Owner = user;
            changeSet.Document = document;
            changeSet.LastModified = DateTime.Now;
            changeSet.VoteBox = VoteBox.Create();
            return EntityMappingContext.Current.ChangeSets.Add(changeSet);
        }

        public void Destroy()
        {
            EntityMappingContext.Current.ChangeSets.Remove(this);
        }
    }
}
