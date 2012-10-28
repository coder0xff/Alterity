using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Alterity.Models
{
    public class EntityMappingContext : DbContext
    {
        [ThreadStatic]
        static EntityMappingContext _current;

        public static EntityMappingContext Current
        {
            get
            {
                if (_current == null)
                    throw new ApplicationException("EntityMappingContext.Current can only be used inside a method passed to EntityMappingContext.AccessDataBase.");
                return _current;
            }
        }

        public static void Access(Action action)
        {
            bool ownsContext = false;
            if (_current == null) //thread safety is inherent because it's a thread static variable
            {
                _current = new EntityMappingContext();
                _current.Database.Initialize(false);
                ownsContext = true;
            }
            action();
            _current.SaveChanges();
            if (ownsContext)
            {
                _current.Dispose();
                _current = null;
                ownsContext = false;
            }
        }
        public DbSet<User> Users { get; set; }
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
            modelBuilder.Entity<User>()
                .HasMany(x => x.AdministratorOf)
                .WithMany(x => x.Administrators)
                .Map(x =>
                    {
                        x.MapLeftKey("UserId");
                        x.MapRightKey("Document");
                        x.ToTable("DocumentAdministrators");
                    });
            modelBuilder.Entity<User>()
                .HasMany(x => x.ModeratorOf)
                .WithMany(x => x.Moderators)
                .Map(x =>
                    {
                        x.MapLeftKey("UserId");
                        x.MapRightKey("Document");
                        x.ToTable("DocumentModerators");
                    });
            base.OnModelCreating(modelBuilder);
        }
    }
}