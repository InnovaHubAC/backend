
namespace Innova.Infrastructure.Data.Configurations
{
    public class IdeaConfiguration : IEntityTypeConfiguration<Idea>
    {
        public void Configure(EntityTypeBuilder<Idea> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(i => i.Content)
                .IsRequired()
                .HasMaxLength(2500);

            builder.Property(i => i.IdeaStatus)
                .IsRequired();

            builder.Property(i => i.IsAnonymous)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(i => i.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne<AppUser>()                 
               .WithMany(u => u.Ideas)                        
               .HasForeignKey(i => i.AppUserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Department)
                .WithMany(i => i.Ideas)
                .HasForeignKey(i => i.DepartmentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.Attachments!)
                .WithOne(a => a.Idea)
                .HasForeignKey(a => a.IdeaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
