using APP.Domain;
using Microsoft.EntityFrameworkCore;

namespace APP.Configuration
{
    public class APPDbContext : DbContext
    {

        public DbSet<LinkMqtt> Links { get; set; }

        public APPDbContext(DbContextOptions options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LinkMqtt>(e =>
            {
                e.Property(e => e.Model).HasConversion(v => v.ToString(), v => (LinkModelEnum)Enum.Parse(typeof(LinkModelEnum), v));
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
