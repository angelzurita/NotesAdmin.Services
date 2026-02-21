// Commented for local development - Azure Blob Storage not available
// Uncomment when Azure packages are re-enabled
/*
using AdminServices.Application.Common.Interfaces;
using AdminServices.Infrastructure.Options;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace AdminServices.Infrastructure.Services;

/// <summary>
/// Blob Storage Service implementation
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobStorageOptions _options;

    public BlobStorageService(BlobServiceClient blobServiceClient, IOptions<BlobStorageOptions> options)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadFileAsync(string fileName, string containerName, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        var response = await blobClient.DownloadAsync(cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteFileAsync(string fileName, string containerName, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> FileExistsAsync(string fileName, string containerName, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        return await blobClient.ExistsAsync(cancellationToken);
    }

    public string GetFileUrl(string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        return blobClient.Uri.ToString();
    }
}
*/
