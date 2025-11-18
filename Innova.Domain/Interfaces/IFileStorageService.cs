namespace Innova.Domain.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(byte[] fileData, string fileName, string contentType);
        public void RemoveFile(string fileUrl, string contentType);
    }
}