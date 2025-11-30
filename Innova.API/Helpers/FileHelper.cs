namespace Innova.API.Helpers
{
    public class FileHelper
    {
        public static async Task<byte[]> ConvertToBytesAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }

        public static async Task<List<FileAttachmentDto>> ConvertFilesToAttachments(IFormFile[] file)
        {
            List<FileAttachmentDto> attachments = new();
            if (file == null || file.Length == 0)
                return attachments;
            foreach (var item in file)
            {
                FileAttachmentDto attachment = new()
                {
                    FileName = item.FileName,
                    ContentType = item.ContentType,
                    Data = await ConvertToBytesAsync(item)
                };
                attachments.Add(attachment);
            }
            return attachments;
        }
    }
}
