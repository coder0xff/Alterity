using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class ChangeSubset
    {
        public int Id { get; set; }
        public VoteBox VoteBox { get; set; }
        public ChangeSet ChangeSet { get; set; }
        public SpringboardState SpringboardState { get; set; }
        public virtual ICollection<EditOperation> EditOperations { get; set; }

        public static ChangeSubset Create(ChangeSet changeSet, SpringboardState springboardState)
        {
            ChangeSubset changeSubset = new ChangeSubset();
            changeSubset.ChangeSet = changeSet;
            changeSubset.SpringboardState = springboardState;
            changeSubset.VoteBox = VoteBox.Create();
            return EntityMappingContext.Current.ChangeSubsets.Add(changeSubset);
        }

        public void Destroy()
        {
            EntityMappingContext.Current.ChangeSubsets.Remove(this);
        }
    }
}