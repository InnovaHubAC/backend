namespace Innova.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.SenderId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(m => m.ReceiverId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(m => m.SentAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(m => m.ReadAt);

        builder.Property(m => m.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationship with Conversation
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Relationships with AppUser (shadow navigation)
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes for efficient querying
        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => m.ReceiverId);
        builder.HasIndex(m => new { m.ReceiverId, m.IsRead });
        builder.HasIndex(m => m.SentAt);
    }
}
