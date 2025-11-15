using Innova.Application.DTOs.Common;
using System.Threading.Tasks;

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

        public static async Task<List<AttachmentDto>> ConvertFilesToAttachments(IFormFile[] file)
        {
            List<AttachmentDto> attachments = new List<AttachmentDto>();
            if(file == null || file.Length == 0)
                return attachments;
            foreach (var item in file)
            {
                AttachmentDto attachment = new AttachmentDto
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
