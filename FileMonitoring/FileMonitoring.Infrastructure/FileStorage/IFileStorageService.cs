namespace FileMonitoring.Infrastructure.FileStorage;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string fileName, byte[] content);
    Task<byte[]> GetFileAsync(string filePath);
    Task DeleteFileAsync(string filePath);
    bool FileExists(string filePath);
}