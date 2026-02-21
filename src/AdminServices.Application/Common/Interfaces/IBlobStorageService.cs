namespace AdminServices.Application.Common.Interfaces;

/// <summary>
/// Blob Storage Service interface
/// </summary>
public interface IBlobStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string fileName, string containerName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileName, string containerName, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string fileName, string containerName, CancellationToken cancellationToken = default);
    string GetFileUrl(string fileName, string containerName);
}
