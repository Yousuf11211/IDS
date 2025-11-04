using IDS.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IDS.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LogFile> LogFiles { get; set; } = null!;
        public DbSet<RoleEntry> RolesList { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<LogFile>(b =>
            {
                b.ToTable("LogFiles");
                b.HasKey(l => l.Id);
                b.Property(l => l.Level).HasMaxLength(50);
                b.Property(l => l.Message).HasMaxLength(4000);
                b.Property(l => l.Exception).HasMaxLength(4000);
                b.HasIndex(l => l.Timestamp);
            });

            builder.Entity<RoleEntry>(b =>
            {
                b.ToTable("Roles");
                b.HasKey(r => r.Id);
                b.Property(r => r.Name).HasMaxLength(256).IsRequired();
                b.Property(r => r.CreatedAt);
            });
        }
    }
}
