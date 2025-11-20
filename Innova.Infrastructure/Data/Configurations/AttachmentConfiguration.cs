
namespace Innova.Infrastructure.Data.Configurations
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(75);

            builder.Property(a => a.FileType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.FileUrl)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(a => a.UploadedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(a => a.Idea)
                .WithMany(i => i.Attachments!)
                .HasForeignKey(a => a.IdeaId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
