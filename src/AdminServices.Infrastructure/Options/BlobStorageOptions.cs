namespace AdminServices.Infrastructure.Options;

/// <summary>
/// Blob Storage configuration options
/// </summary>
public class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    public string BaseUrl { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "adminservices";
}
