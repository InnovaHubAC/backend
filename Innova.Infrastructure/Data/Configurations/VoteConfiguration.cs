namespace Innova.Infrastructure.Data.Configurations;

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.ToTable("Votes");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.IdeaId)
            .IsRequired();

        builder.Property(v => v.AppUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(v => v.VoteType)
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(v => v.WithdrawnAt);

        builder.HasOne(v => v.Idea)
            .WithMany(i => i.Votes)
            .HasForeignKey(v => v.IdeaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(v => v.AppUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        // Index to ensure one vote per user per idea
        builder.HasIndex(v => new { v.IdeaId, v.AppUserId })
            .IsUnique();

        builder.HasIndex(v => v.IdeaId);
        builder.HasIndex(v => v.AppUserId);
    }
}
