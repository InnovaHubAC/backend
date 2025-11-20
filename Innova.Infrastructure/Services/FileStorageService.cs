using Microsoft.Extensions.Logging;

namespace Innova.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(ILogger<FileStorageService> logger)
        {
            _logger = logger;
        }

        private string GetBasePath()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "assets");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public void RemoveFile(string fileUrl, string contentType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUrl))
                    return;

                var folderName = GetFolderNameFromContentType(contentType);
                var folderPath = Path.Combine(GetBasePath(), folderName);
                var fileName = Path.GetFileName(fileUrl);
                var filePath = Path.Combine(folderPath, fileName);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting file: {FileUrl} with message {Message}", fileUrl, e.Message);
            }
        }

        public async Task<string> SaveFileAsync(byte[] fileData, string fileName, string contentType)
        {
            try
            {
                var folderName = GetFolderNameFromContentType(contentType);
                var folderPath = Path.Combine(GetBasePath(), folderName);
                var filePath = Path.Combine(folderPath, fileName);
                await File.WriteAllBytesAsync(filePath, fileData);
                return $"/assets/{folderName}/{fileName}";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving file: {FileName} with message {Message}", fileName, e.Message);
                return string.Empty;
            }
        }

        private string GetFolderNameFromContentType(string contentType)
        {
            string folderName = contentType?.ToLower() switch
            {
                var ct when string.IsNullOrWhiteSpace(ct) => "others",
                var ct when ct.StartsWith("image/") => "images",
                "application/pdf" => "pdfs",
                var ct when ct.StartsWith("video/") => "videos",
                var ct when ct.StartsWith("audio/") => "audios",
                _ => "others"
            };

            var folderPath = Path.Combine(GetBasePath(), folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderName;
        }

    }
}
