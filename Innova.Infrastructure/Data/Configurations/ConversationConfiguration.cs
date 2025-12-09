namespace Innova.Infrastructure.Data.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ParticipantOneId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(c => c.ParticipantTwoId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.LastMessageAt);

        // relationships with AppUser
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(c => c.ParticipantOneId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(c => c.ParticipantTwoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        // index for efficient querying
        builder.HasIndex(c => new { c.ParticipantOneId, c.ParticipantTwoId })
            .IsUnique();

        builder.HasIndex(c => c.ParticipantOneId);
        builder.HasIndex(c => c.ParticipantTwoId);
    }
}
