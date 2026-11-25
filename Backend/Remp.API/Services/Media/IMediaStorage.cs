namespace Remp.API.Services.Media;

public interface IMediaStorage{
    public Task UploadAsync(string blobName, Stream content, string contentType, CancellationToken cancellationToken);
    public Task<Uri> GetReadUrlAsync(string blobName, CancellationToken cancellationToken);
    public Task DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken);
}