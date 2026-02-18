using Microsoft.AspNetCore.Http;

namespace WebApi.DermaImage.Managers;

public interface IImageUploadManager
{
    Task<ImageUploadResult> SaveUploadedFileAsync(IFormFile file, CancellationToken cancellationToken = default);
}

public sealed record ImageUploadResult(
    string FullPath,
    string StoredFileName,
    string ContentType,
    long FileSize);
