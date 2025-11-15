namespace Innova.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private string GetBasePath()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "assets");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        public async Task<string> SaveFileAsync(byte[] fileData, string fileName, string contentType)
        {
            var folderName = GetFolderNameFromContentType(contentType);
            var folderPath = Path.Combine(GetBasePath(), folderName);
            var filePath = Path.Combine(folderPath, fileName);
            await File.WriteAllBytesAsync(filePath, fileData);
            return $"/assets/{folderName}/{fileName}";
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
