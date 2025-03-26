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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StoredEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e=>e.EventType).IsRequired();
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }
    }
}
