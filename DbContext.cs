using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using ŽVPAIS_API.Models;

namespace ŽVPAIS_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Specialist> Specialists { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<DamageEvaluation> DamageEvaluations { get; set; }
        public DbSet<EventObject> EventObjects { get; set; }
        public DbSet<EnvironmentObject> Objects { get; set; }
        public DbSet<ObjectMaterial> ObjectMaterials { get; set; }
        public DbSet<IndexingCoefficient> IndexingCoefficients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .Property(e => e.EventType)
                .HasColumnType("text");
            modelBuilder.Entity<ObjectMaterial>()
                .HasKey(om => new { om.ObjectId, om.MaterialId });
            modelBuilder.Entity<EventObject>()
                .HasOne(eo => eo.Event)
                .WithMany(e => e.EventObjects)
                .HasForeignKey(eo => eo.EventId);
            modelBuilder.Entity<EventObject>()
                .HasOne(eo => eo.Object)
                .WithMany(o => o.EventObjects)
                .HasForeignKey(eo => eo.ObjectId);
            modelBuilder.Entity<DamageEvaluation>()
                .HasOne(de => de.Event)
                .WithMany(e => e.DamageEvaluations)
                .HasForeignKey(de => de.EventId);
        }
    }
}