using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Alterity.Models
{
    public class EntityMappingContext : DbContext
    {
        public DbSet<UserData> UserData { get; set; }
        public DbSet<SpringboardState> Springboards { get; set; }
        public DbSet<SpringboardStateEntry> SpringboardEntries { get; set; }
        public DbSet<VoteBox> VoteBoxes { get; set; }
        public DbSet<VoteEntry> Votes { get; set; }
        public DbSet<ChangeSubset> ChangeSubsets { get; set; }
        public DbSet<ChangeSet> ChangeSets { get; set; }
        public DbSet<EditOperation> EditOperations { get; set; }
        public DbSet<Hunk> Hunks { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<SessionData> SessionDatas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserData>()
                .HasMany(x => x.AdministratorOf)
                .WithMany(x => x.Administrators)
                .Map(x =>
                    {
                        x.MapLeftKey("UserName");
                        x.MapRightKey("Document");
                        x.ToTable("DocumentAdministrators");
                    });
            modelBuilder.Entity<UserData>()
                .HasMany(x => x.ModeratorOf)
                .WithMany(x => x.Moderators)
                .Map(x =>
                    {
                        x.MapLeftKey("UserName");
                        x.MapRightKey("Document");
                        x.ToTable("DocumentModerators");
                    });

            base.OnModelCreating(modelBuilder);
        }
    }
}