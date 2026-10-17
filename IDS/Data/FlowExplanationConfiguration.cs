using IDS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDS.Data;

public sealed class FlowExplanationConfiguration : IEntityTypeConfiguration<FlowExplanation>
{
    public void Configure(EntityTypeBuilder<FlowExplanation> builder)
    {
        builder.ToTable("FlowExplanations", table =>
        {
            table.HasCheckConstraint("CK_FlowExplanations_Stage", "[Stage] IN ('gatekeeper','multiclass')");
            table.HasCheckConstraint("CK_FlowExplanations_Status", "[Status] IN ('pending','processing','completed','failed')");
            table.HasCheckConstraint("CK_FlowExplanations_Attempts", "[Attempts] >= 0");
        });
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Stage).HasMaxLength(20).IsRequired();
        builder.Property(e => e.ModelVersion).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Fingerprint).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
        builder.Property(e => e.ContextJson).IsRequired();
        builder.Property(e => e.LastError).HasMaxLength(1000);
        builder.HasIndex(e => new { e.AttackTrafficId, e.Stage, e.ModelVersion }).IsUnique();
        builder.HasIndex(e => new { e.Status, e.Fingerprint, e.NextAttempt, e.Id });
        builder.HasOne<AttackTraffic>().WithMany().HasForeignKey(e => e.AttackTrafficId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RawPacket>().WithMany().HasForeignKey(e => e.RawPacketId).OnDelete(DeleteBehavior.Restrict);
    }
}
