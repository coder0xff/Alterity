using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Alterity.Models
{
    public class EntityMappingContext : DbContext
    {
        public DbSet<UserAccount> Users { get; set; }
        public DbSet<SpringboardState> Springboards { get; set; }
        public DbSet<SpringboardStateEntry> SpringboardEntries { get; set; }
        public DbSet<VoteBox> VoteBoxes { get; set; }
        public DbSet<VoteEntry> Votes { get; set; }
        public DbSet<ChangeSubset> ChangeSubsets { get; set; }
        public DbSet<ChangeSet> ChangeSets { get; set; }
        public DbSet<EditOperation> EditOperations { get; set; }
        public DbSet<Hunk> Hunks { get; set; }
    }
}