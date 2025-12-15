namespace Innova.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.RecipientId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(n => n.TriggeredById)
            .HasMaxLength(450);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.NotificationType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(n => new { n.RecipientId, n.IsRead });

        builder.HasOne(n => n.Idea)
            .WithMany()
            .HasForeignKey(n => n.IdeaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(n => n.Vote)
            .WithMany()
            .HasForeignKey(n => n.VoteId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(n => n.TriggeredById)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
