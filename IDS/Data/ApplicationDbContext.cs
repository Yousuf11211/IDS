using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IDS.Data.Models;

namespace IDS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LogFile> LogFiles { get; set; } = null!;
        public DbSet<SecurityAlert> SecurityAlerts { get; set; } = null!;
        public DbSet<NetworkEvent> NetworkEvents { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<DashboardMetrics> DashboardMetrics { get; set; } = null!;

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

            builder.Entity<SecurityAlert>(b =>
            {
                b.ToTable("SecurityAlerts");
                b.HasKey(a => a.Id);
                b.Property(a => a.Severity).HasMaxLength(20);
                b.Property(a => a.AlertType).HasMaxLength(100);
                b.Property(a => a.SourceIP).HasMaxLength(45);
                b.Property(a => a.DestinationIP).HasMaxLength(45);
                b.Property(a => a.Protocol).HasMaxLength(20);
                b.Property(a => a.Message).HasMaxLength(2000);
                b.HasIndex(a => a.Timestamp);
                b.HasIndex(a => a.Severity);
                b.HasIndex(a => a.IsAcknowledged);
            });

            builder.Entity<NetworkEvent>(b =>
            {
                b.ToTable("NetworkEvents");
                b.HasKey(e => e.Id);
                b.Property(e => e.SourceIP).HasMaxLength(45);
                b.Property(e => e.DestinationIP).HasMaxLength(45);
                b.Property(e => e.Protocol).HasMaxLength(20);
                b.Property(e => e.Classification).HasMaxLength(50);
                b.Property(e => e.AttackType).HasMaxLength(100);
                b.HasIndex(e => e.Timestamp);
                b.HasIndex(e => e.Classification);
            });

            builder.Entity<SystemSetting>(b =>
            {
                b.ToTable("SystemSettings");
                b.HasKey(s => s.Id);
                b.Property(s => s.Key).HasMaxLength(200);
                b.Property(s => s.Value).HasMaxLength(4000);
                b.HasIndex(s => s.Key).IsUnique();
            });

            builder.Entity<AuditLog>(b =>
            {
                b.ToTable("AuditLogs");
                b.HasKey(a => a.Id);
                b.Property(a => a.UserId).HasMaxLength(450);
                b.Property(a => a.UserEmail).HasMaxLength(256);
                b.Property(a => a.Action).HasMaxLength(100);
                b.Property(a => a.EntityType).HasMaxLength(100);
                b.Property(a => a.EntityId).HasMaxLength(450);
                b.Property(a => a.IpAddress).HasMaxLength(45);
                b.HasIndex(a => a.Timestamp);
                b.HasIndex(a => a.UserId);
            });

            builder.Entity<DashboardMetrics>(b =>
            {
                b.ToTable("DashboardMetrics");
                b.HasKey(m => m.Id);
                b.HasIndex(m => m.SnapshotTime);
            });
        }
    }
}
