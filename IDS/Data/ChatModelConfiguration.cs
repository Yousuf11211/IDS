using IDS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDS.Data;

public sealed class EmployeeDepartmentConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.Department).HasMaxLength(80).HasDefaultValue("General").IsRequired();
    }
}

public sealed class ChatPublicKeyConfiguration : IEntityTypeConfiguration<ChatPublicKey>
{
    public void Configure(EntityTypeBuilder<ChatPublicKey> builder)
    {
        builder.ToTable("ChatPublicKeys");
        builder.HasKey(key => key.UserId);
        builder.Property(key => key.UserId).HasMaxLength(450);
        builder.Property(key => key.SubjectPublicKeyInfo).HasMaxLength(512).IsRequired();
        builder.Property(key => key.Fingerprint).HasMaxLength(64).IsRequired();
        builder.HasOne<ApplicationUser>().WithOne().HasForeignKey<ChatPublicKey>(key => key.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.SenderId).HasMaxLength(450).IsRequired();
        builder.Property(message => message.RecipientId).HasMaxLength(450).IsRequired();
        builder.Property(message => message.ClientMessageId).HasMaxLength(36).IsRequired();
        builder.Property(message => message.SenderCiphertext).HasMaxLength(8192).IsRequired();
        builder.Property(message => message.RecipientCiphertext).HasMaxLength(8192).IsRequired();
        builder.Property(message => message.SenderNonce).HasMaxLength(24).IsRequired();
        builder.Property(message => message.RecipientNonce).HasMaxLength(24).IsRequired();
        builder.Property(message => message.SenderKeyFingerprint).HasMaxLength(64).IsRequired();
        builder.Property(message => message.RecipientKeyFingerprint).HasMaxLength(64).IsRequired();
        builder.HasIndex(message => new { message.SenderId, message.ClientMessageId }).IsUnique();
        builder.HasIndex(message => new { message.RecipientId, message.Id });
        builder.HasIndex(message => new { message.SenderId, message.RecipientId, message.Id });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(message => message.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(message => message.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
