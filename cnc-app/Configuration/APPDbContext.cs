using APP.Domain;
using APP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace APP.Configuration
{
    public class APPDbContext : DbContext
    {

        public DbSet<LinkMqtt> Links { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Adaptor> Adaptors { get; set; }

        public APPDbContext(DbContextOptions options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LinkMqtt>(e =>
            {
                e.Property(e => e.Model).HasConversion(v => v.ToString(), v => (LinkModelEnum)Enum.Parse(typeof(LinkModelEnum), v));
            });

            modelBuilder.Entity<Device>(e =>
            {
                e.Property(e => e.Model).HasConversion(v => v.ToString(), v => (DeviceModelEnum)Enum.Parse(typeof(DeviceModelEnum), v));
                e.Property(e => e.Kind).HasConversion(v => v.ToString(), v => (DeviceKindEnum)Enum.Parse(typeof(DeviceKindEnum), v));
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
