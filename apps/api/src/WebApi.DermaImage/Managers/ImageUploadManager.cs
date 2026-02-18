using Microsoft.AspNetCore.Http;

namespace WebApi.DermaImage.Managers;

public class ImageUploadManager : IImageUploadManager
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public ImageUploadManager(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<ImageUploadResult> SaveUploadedFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var configuredRoot = _configuration["ImageStorage:RootPath"];
        var storageRoot = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(_environment.ContentRootPath, "ImageStorage")
            : configuredRoot;

        Directory.CreateDirectory(storageRoot);

        var originalFileName = Path.GetFileName(file.FileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);

        var finalFileName = originalFileName;
        var finalPath = Path.Combine(storageRoot, finalFileName);

        if (File.Exists(finalPath))
        {
            finalFileName = $"{fileNameWithoutExtension}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            finalPath = Path.Combine(storageRoot, finalFileName);
        }

        await using var targetStream = new FileStream(finalPath, FileMode.CreateNew);
        await file.CopyToAsync(targetStream, cancellationToken);

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;

        return new ImageUploadResult(finalPath, finalFileName, contentType, file.Length);
    }
}
