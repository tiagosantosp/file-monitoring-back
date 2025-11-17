using Microsoft.Extensions.Configuration;

namespace FileMonitoring.Infrastructure.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _backupDirectory;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _backupDirectory = configuration["FileStorage:BackupPath"]
                          ?? Path.Combine(Directory.GetCurrentDirectory(), "Backups");

        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    public async Task<string> SaveFileAsync(string fileName, byte[] content)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var safeFileName = $"{timestamp}_{SanitizeFileName(fileName)}";
        var fullPath = Path.Combine(_backupDirectory, safeFileName);

        await File.WriteAllBytesAsync(fullPath, content);

        return fullPath;
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");
        }

        return await File.ReadAllBytesAsync(filePath);
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
        }
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}