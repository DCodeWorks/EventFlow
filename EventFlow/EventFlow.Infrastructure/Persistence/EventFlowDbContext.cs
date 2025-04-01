using EventFlow.Domain;
using EventFlow.Infrastructure.ReadModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Persistence
{
    public class EventFlowDbContext : DbContext
    {
        public EventFlowDbContext(DbContextOptions<EventFlowDbContext> options)
            : base(options)
        {
        }

        public DbSet<StoredEvent> StoredEvents { get; set; }
        public DbSet<TaskReadModel> TaskReadModels { get; set; }
        public DbSet<TaskSnapshot> TaskSnapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StoredEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired();
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<TaskReadModel>(entity =>
            {
                entity.HasKey(e => e.TaskId);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });
            modelBuilder.Entity<TaskSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AggregateId).IsRequired();
                entity.Property(e => e.SnapshotData).IsRequired();
                entity.Property(e => e.LastEventSequence).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }
    }
}
