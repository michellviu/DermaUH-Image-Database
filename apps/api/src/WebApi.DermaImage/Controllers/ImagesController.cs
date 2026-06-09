using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Application.DermaImage.Validation;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using WebApi.DermaImage.DTOs;
using WebApi.DermaImage.Managers;
using WebApi.DermaImage.Metadata;

namespace WebApi.DermaImage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IDermaImgManager _manager;
    private readonly IUserManager _userManager;
    private readonly IImageUploadManager _imageUploadManager;

    public ImagesController(
        IDermaImgManager manager,
        IUserManager userManager,
        IImageUploadManager imageUploadManager)
    {
        _manager = manager;
        _userManager = userManager;
        _imageUploadManager = imageUploadManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResponse<DermaImgResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] List<ImageType>? imageTypes = null,
        [FromQuery] List<DiagnosisCategory>? diagnosisCategories = null,
        [FromQuery] List<InjuryType>? injuryTypes = null,
        [FromQuery] List<PhotoType>? fotoTypes = null,
        [FromQuery] List<Sex>? sexes = null,
        [FromQuery] List<AnatomSiteGeneral>? anatomSites = null,
        [FromQuery] List<DiagnosisConfirmType>? diagnosisConfirmTypes = null,
        [FromQuery] string? diagnosisContains = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new DermaImgFilter
        {
            ImageTypes = imageTypes,
            DiagnosisCategories = diagnosisCategories,
            InjuryTypes = injuryTypes,
            FotoTypes = fotoTypes,
            DiagnosisConfirmTypes = diagnosisConfirmTypes,
            Sexes = sexes,
            AnatomSites = anatomSites,
            IsPublic = true,
            DiagnosisContains = diagnosisContains
        };

        var (items, totalCount) = await _manager.GetPagedAsync(page, pageSize, filter, cancellationToken);
        
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        if (!isAuthenticated)
        {
            if (page == 1)
            {
                items = items.Take(10).ToList();
            }
            else
            {
                items = new List<DermaImg>();
            }
        }

        return Ok(new PagedResponse<DermaImgResponseDto>
        {
            Items = items.Select(MapToResponseDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DermaImgResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var image = await _manager.GetByIdAsync(id, cancellationToken);
        if (image is not null && !CanAccessImage(image))
        {
            return NotFound();
        }
        return image is null ? NotFound() : Ok(MapToResponseDto(image));
    }

    [AllowAnonymous]
    [HttpGet("public/{publicId}")]
    public async Task<ActionResult<DermaImgResponseDto>> GetByPublicId(string publicId, CancellationToken cancellationToken)
    {
        var image = await _manager.GetByPublicIdAsync(publicId, cancellationToken);
        if (image is not null && !CanAccessImage(image))
        {
            return NotFound();
        }
        return image is null ? NotFound() : Ok(MapToResponseDto(image));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> GetPreview(Guid id, CancellationToken cancellationToken)
    {
        var image = await _manager.GetByIdAsync(id, cancellationToken);
        if (image is null)
        {
            return NotFound();
        }

        if (!CanAccessImage(image))
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(image.FilePath) || !System.IO.File.Exists(image.FilePath))
        {
            return NotFound("No se encontró el archivo físico de la imagen.");
        }

        var contentType = string.IsNullOrWhiteSpace(image.ContentType)
            ? "application/octet-stream"
            : image.ContentType;

        return PhysicalFile(image.FilePath, contentType, enableRangeProcessing: true);
    }


    [HttpPost("download")]
    public async Task<IActionResult> DownloadSelected(
        [FromBody] DownloadImagesRequest request,
        [FromQuery] ImageDownloadMode mode = ImageDownloadMode.ImagesAndMetadata,
        CancellationToken cancellationToken = default)
    {
        if (request is null || request.ImageIds is null || request.ImageIds.Count == 0)
        {
            return BadRequest("Debe seleccionar al menos una imagen para descargar.");
        }

        var images = await _manager.GetByIdsAsync(request.ImageIds, cancellationToken);
        var accessibleImages = images.Where(CanAccessImage).ToList();

        if (accessibleImages.Count == 0)
        {
            return NotFound("No se encontraron imagenes disponibles para descargar.");
        }

        var (includeImages, includeMetadata) = ResolveDownloadOptions(mode);
        if (!includeImages && !includeMetadata)
        {
            return BadRequest("Debe seleccionar imagenes o metadatos para descargar.");
        }

        var fileName = BuildZipFileName("seleccion", mode);
        return await BuildZipResultAsync(accessibleImages, fileName, includeImages, includeMetadata, cancellationToken);
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadAll(
        [FromQuery] List<ImageType>? imageTypes = null,
        [FromQuery] List<DiagnosisCategory>? diagnosisCategories = null,
        [FromQuery] List<InjuryType>? injuryTypes = null,
        [FromQuery] List<PhotoType>? fotoTypes = null,
        [FromQuery] List<Sex>? sexes = null,
        [FromQuery] List<AnatomSiteGeneral>? anatomSites = null,
        [FromQuery] List<DiagnosisConfirmType>? diagnosisConfirmTypes = null,
        [FromQuery] string? diagnosisContains = null,
        [FromQuery] ImageDownloadMode mode = ImageDownloadMode.ImagesAndMetadata,
        CancellationToken cancellationToken = default)
    {
        var filter = new DermaImgFilter
        {
            ImageTypes = imageTypes,
            DiagnosisCategories = diagnosisCategories,
            InjuryTypes = injuryTypes,
            FotoTypes = fotoTypes,
            DiagnosisConfirmTypes = diagnosisConfirmTypes,
            Sexes = sexes,
            AnatomSites = anatomSites,
            IsPublic = true,
            DiagnosisContains = diagnosisContains
        };

        var images = await _manager.GetFilteredAsync(filter, cancellationToken);
        var accessibleImages = images.Where(CanAccessImage).ToList();

        if (accessibleImages.Count == 0)
        {
            return NotFound("No se encontraron imagenes para descargar con los filtros actuales.");
        }

        var (includeImages, includeMetadata) = ResolveDownloadOptions(mode);
        if (!includeImages && !includeMetadata)
        {
            return BadRequest("Debe seleccionar imagenes o metadatos para descargar.");
        }

        var fileName = BuildZipFileName("galeria", mode);
        return await BuildZipResultAsync(accessibleImages, fileName, includeImages, includeMetadata, cancellationToken);
    }

    [Authorize(Roles = "Admin, Contributor")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DermaImgResponseDto>> Create(
        [FromForm] CreateDermaImgFormDto formDto,
        CancellationToken cancellationToken)
    {
        if (formDto.File is null || formDto.File.Length == 0)
        {
            return BadRequest("Debe seleccionar una imagen para subir.");
        }

        var savedFile = await _imageUploadManager.SaveUploadedFileAsync(formDto.File, cancellationToken);

        // Map form DTO to internal DTO with server-calculated properties
        var dto = new CreateDermaImgDto
        {
            FileName = savedFile.StoredFileName,
            FilePath = savedFile.FullPath,
            ContentType = savedFile.ContentType,
            FileSize = savedFile.FileSize,
            IsPublic = formDto.IsPublic,
            ImageType = formDto.ImageType,
            ImageManipulation = formDto.ImageManipulation,
            DermoscopicType = formDto.DermoscopicType,
            PatientName = formDto.PatientName,
            ClinicalHistoryNumber = formDto.ClinicalHistoryNumber,
            AgeApprox = formDto.AgeApprox,
            Sex = formDto.Sex,
            SkinColor = formDto.SkinColor,
            FotoType = formDto.FotoType,
            PersonalHistory = formDto.PersonalHistory,
            PersonalHxMm = formDto.PersonalHxMm,
            FamilyHxMm = formDto.FamilyHxMm,
            SunExposure = formDto.SunExposure,
            AnatomSiteGeneral = formDto.AnatomSiteGeneral,
            AnatomSiteSpecial = formDto.AnatomSiteSpecial,
            ClinSizeLongDiamMm = formDto.ClinSizeLongDiamMm,
            Diagnosis = formDto.Diagnosis,
            HistopathologicalDiagnosis = formDto.HistopathologicalDiagnosis,
            DiagnosisCategory = formDto.DiagnosisCategory,
            InjuryType = formDto.InjuryType,
            DiagnosisConfirmType = formDto.DiagnosisConfirmType,
            MelThickMm = formDto.MelThickMm,
            MelMitoticIndex = formDto.MelMitoticIndex,
            MelUlcer = formDto.MelUlcer,
            ClinicalNotes = formDto.ClinicalNotes,
            DermoscopicComments = formDto.DermoscopicComments,
            InformedConsent = formDto.InformedConsent,
            InformedConsentDate = formDto.InformedConsentDate,
            InformedConsentText = formDto.InformedConsentText,
            ContributorId = formDto.ContributorId,
            InstitutionName = formDto.InstitutionName,
            InstitutionDescription = formDto.InstitutionDescription,
            InstitutionCountry = formDto.InstitutionCountry
        };

        if (dto.ContributorId == Guid.Empty)
        {
            return BadRequest("Debe proporcionar un ContributorId válido.");
        }

        var businessValidationErrors = DermaImgValidationRules.Validate(dto);
        if (businessValidationErrors.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(businessValidationErrors)
            {
                Title = "Reglas de validacion de imagen incumplidas.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var created = await _manager.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponseDto(created));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateDermaImgDto dto, CancellationToken cancellationToken)
    {
        var existing = await _manager.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin)
        {
            return Forbid();
        }

        dto.ContributorId = existing.ContributorId;
        dto.FileName = existing.FileName;
        dto.FilePath = existing.FilePath;
        dto.ContentType = existing.ContentType;
        dto.FileSize = existing.FileSize;

        var businessValidationErrors = DermaImgValidationRules.Validate(dto);
        if (businessValidationErrors.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(businessValidationErrors)
            {
                Title = "Reglas de validacion de imagen incumplidas.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        await _manager.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _manager.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin)
        {
            return Forbid();
        }

        await _manager.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static string BuildZipFileName(string suffix)
    {
        var safeSuffix = string.IsNullOrWhiteSpace(suffix)
            ? "imagenes"
            : $"imagenes-{suffix.Trim().ToLowerInvariant()}";
        return $"dermauh-{safeSuffix}-{DateTime.UtcNow:yyyyMMdd-HHmm}.zip";
    }

    private static string BuildZipFileName(string suffix, ImageDownloadMode mode)
    {
        var baseName = BuildZipFileName(suffix);
        if (mode == ImageDownloadMode.ImagesAndMetadata)
        {
            return baseName;
        }

        var modeSuffix = mode == ImageDownloadMode.MetadataOnly
            ? "metadatos"
            : "solo-imagenes";

        return baseName.Replace(".zip", $"-{modeSuffix}.zip");
    }

    private static async Task<FileContentResult> BuildZipResultAsync(
        IReadOnlyList<DermaImg> images,
        string fileName,
        bool includeImages,
        bool includeMetadata,
        CancellationToken cancellationToken)
    {
        var missingFiles = new List<string>();
        await using var archiveStream = new MemoryStream();

        using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            if (includeMetadata)
            {
                var csvEntry = archive.CreateEntry("metadata.csv", CompressionLevel.Fastest);
                await using (var entryStream = csvEntry.Open())
                await using (var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
                {
                    var csv = ImageMetadataCatalog.BuildImagesCsv(images);
                    await writer.WriteAsync(csv);
                }
            }

            if (includeImages)
            {
                foreach (var image in images)
                {
                    if (string.IsNullOrWhiteSpace(image.FilePath) || !System.IO.File.Exists(image.FilePath))
                    {
                        missingFiles.Add($"{image.PublicId} | {image.FileName}");
                        continue;
                    }

                    var extension = ResolveImageExtension(image);
                    var entryName = $"images/{SanitizeFileName(image.PublicId)}{extension}";
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);

                    await using var fileStream = System.IO.File.OpenRead(image.FilePath);
                    await using var entryStream = entry.Open();
                    await fileStream.CopyToAsync(entryStream, cancellationToken);
                }
            }

            if (includeImages && missingFiles.Count > 0)
            {
                var errorsEntry = archive.CreateEntry("errores.txt", CompressionLevel.Fastest);
                await using var errorStream = errorsEntry.Open();
                await using var writer = new StreamWriter(errorStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                await writer.WriteLineAsync("No se encontraron los siguientes archivos:");
                foreach (var missing in missingFiles)
                {
                    await writer.WriteLineAsync(missing);
                }
            }
        }

        archiveStream.Position = 0;
        return new FileContentResult(archiveStream.ToArray(), "application/zip")
        {
            FileDownloadName = fileName
        };
    }

    private static (bool IncludeImages, bool IncludeMetadata) ResolveDownloadOptions(ImageDownloadMode mode)
    {
        return mode switch
        {
            ImageDownloadMode.MetadataOnly => (false, true),
            ImageDownloadMode.ImagesOnly => (true, false),
            _ => (true, true)
        };
    }

    private static string ResolveImageExtension(DermaImg image)
    {
        var extension = Path.GetExtension(image.FileName);
        if (!string.IsNullOrWhiteSpace(extension))
        {
            return extension.StartsWith('.') ? extension : $".{extension}";
        }

        if (!string.IsNullOrWhiteSpace(image.ContentType) && ContentTypeMappings.TryGetValue(image.ContentType, out var mapped))
        {
            return mapped;
        }

        extension = Path.GetExtension(image.FilePath);
        return string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension;
    }

    private static string SanitizeFileName(string value)
    {
        var filtered = new string(value.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').ToArray());
        return string.IsNullOrWhiteSpace(filtered) ? "imagen" : filtered;
    }

    private static readonly IReadOnlyDictionary<string, string> ContentTypeMappings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/jpg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/gif"] = ".gif",
            ["image/webp"] = ".webp",
            ["image/bmp"] = ".bmp",
            ["image/tiff"] = ".tiff"
        };

    private bool CanAccessImage(DermaImg image)
    {
        if (image.IsPublic)
        {
            return true;
        }

        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return false;
        }

        if (User.IsInRole("Admin"))
        {
            return true;
        }

        if(!image.IsPublic)
        {
            return false;
        }

        return true;
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static DermaImgResponseDto MapToResponseDto(DermaImg image)
    {
        return new DermaImgResponseDto
        {
            Id = image.Id,
            PublicId = image.PublicId,
            ContentType = image.ContentType,
            FileSize = image.FileSize,
            IsPublic = image.IsPublic,
            ImageType = image.ImageType,
            ImageManipulation = image.ImageManipulation,
            DermoscopicType = image.DermoscopicType,
            PatientName = image.PatientName,
            ClinicalHistoryNumber = image.ClinicalHistoryNumber,
            AgeApprox = image.AgeApprox,
            Sex = image.Sex,
            SkinColor = image.SkinColor,
            FotoType = image.FotoType,
            PersonalHistory = image.PersonalHistory,
            PersonalHxMm = image.PersonalHxMm,
            FamilyHxMm = image.FamilyHxMm,
            SunExposure = image.SunExposure,
            AnatomSiteGeneral = image.AnatomSiteGeneral,
            AnatomSiteSpecial = image.AnatomSiteSpecial,
            ClinSizeLongDiamMm = image.ClinSizeLongDiamMm,
            Diagnosis = image.Diagnosis,
            HistopathologicalDiagnosis = image.HistopathologicalDiagnosis,
            DiagnosisCategory = image.DiagnosisCategory,
            InjuryType = image.InjuryType,
            DiagnosisConfirmType = image.DiagnosisConfirmType,
            MelThickMm = image.MelThickMm,
            MelMitoticIndex = image.MelMitoticIndex,
            MelUlcer = image.MelUlcer,
            ClinicalNotes = image.ClinicalNotes,
            DermoscopicComments = image.DermoscopicComments,
            InformedConsent = image.InformedConsent,
            InformedConsentDate = image.InformedConsentDate,
            InformedConsentText = image.InformedConsentText,
            CreatedAt = image.CreatedAt,
            ContributorId = image.ContributorId,
            ContributorFullName = null,
            InstitutionName = image.InstitutionName,
            InstitutionDescription = image.InstitutionDescription,
            InstitutionCountry = image.InstitutionCountry
        };
    }

    public enum ImageDownloadMode
    {
        ImagesAndMetadata,
        MetadataOnly,
        ImagesOnly
    }
}
