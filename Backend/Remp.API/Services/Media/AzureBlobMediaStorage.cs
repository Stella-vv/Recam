
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;

namespace Remp.API.Services.Media;

public class AzureBlobMediaStorage : IMediaStorage
{
  private readonly BlobContainerClient _containerClient;
  private readonly AzureBlobStorageSettings _settings;
  private readonly BlobServiceClient _serviceClient;

  public AzureBlobMediaStorage(BlobServiceClient serviceClient, IOptions<AzureBlobStorageSettings> options)
  {
    _settings = options.Value;
    _containerClient = serviceClient.GetBlobContainerClient(_settings.ContainerName);
    _serviceClient = serviceClient;
  }

  public async Task<Uri> GetReadUrlAsync(string blobName, CancellationToken cancellationToken)
  {
    var blobClient = _containerClient.GetBlobClient(blobName);
    var expiresOn = DateTimeOffset.UtcNow.AddMinutes(_settings.ReadUrlLifetimeMinutes);
    var sasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, expiresOn)
    {
        BlobContainerName = _containerClient.Name,
        BlobName = blobClient.Name,
        Resource = "b"
    };

    if (blobClient.CanGenerateSasUri)
    {
        return blobClient.GenerateSasUri(sasBuilder);
    }

    var keyResponse = await _serviceClient.GetUserDelegationKeyAsync(startsOn: null, expiresOn: expiresOn, cancellationToken: cancellationToken);
    var sasParameters = sasBuilder.ToSasQueryParameters(keyResponse.Value, _serviceClient.AccountName);
    var uriBuilder = new BlobUriBuilder(blobClient.Uri)
                     {
                        Sas = sasParameters
                     };

    return uriBuilder.ToUri();
  }

  public async Task UploadAsync(string blobName, Stream content, string contentType, CancellationToken cancellationToken)
  {
    var blobClient = _containerClient.GetBlobClient(blobName);
    var uploadOptions = new BlobUploadOptions();
    uploadOptions.HttpHeaders = new BlobHttpHeaders 
                                { 
                                    ContentType = contentType 
                                };

    await blobClient.UploadAsync(content, uploadOptions, cancellationToken);
  }

  public async Task DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken)
  {
      var blobClient = _containerClient.GetBlobClient(blobName);
      await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
  }
}