using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Application.DermaImage.Validation;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
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
        [FromQuery] Guid? contributorId = null,
        [FromQuery] string? diagnosisContains = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new DermaImgFilter
        {
            ImageTypes = imageTypes,
            DiagnosisCategories = diagnosisCategories,
            InjuryTypes = injuryTypes,
            FotoTypes = fotoTypes,
            ContributorId = contributorId,
            Sexes = sexes,
            AnatomSites = anatomSites,
            IsPublic = true,
            ApprovalStatuses = [ImageApprovalStatus.Approved],
            DiagnosisContains = diagnosisContains
        };

        var (items, totalCount) = await _manager.GetPagedAsync(page, pageSize, filter, cancellationToken);
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

    [AllowAnonymous]
    [HttpPost("download")]
    public async Task<IActionResult> DownloadSelected([FromBody] DownloadImagesRequest request, CancellationToken cancellationToken)
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

        var fileName = BuildZipFileName("seleccion");
        return await BuildZipResultAsync(accessibleImages, fileName, cancellationToken);
    }

    [AllowAnonymous]
    [HttpGet("download")]
    public async Task<IActionResult> DownloadAll(
        [FromQuery] List<ImageType>? imageTypes = null,
        [FromQuery] List<DiagnosisCategory>? diagnosisCategories = null,
        [FromQuery] List<InjuryType>? injuryTypes = null,
        [FromQuery] List<PhotoType>? fotoTypes = null,
        [FromQuery] List<Sex>? sexes = null,
        [FromQuery] List<AnatomSiteGeneral>? anatomSites = null,
        [FromQuery] Guid? contributorId = null,
        [FromQuery] string? diagnosisContains = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new DermaImgFilter
        {
            ImageTypes = imageTypes,
            DiagnosisCategories = diagnosisCategories,
            InjuryTypes = injuryTypes,
            FotoTypes = fotoTypes,
            ContributorId = contributorId,
            Sexes = sexes,
            AnatomSites = anatomSites,
            IsPublic = true,
            ApprovalStatuses = [ImageApprovalStatus.Approved],
            DiagnosisContains = diagnosisContains
        };

        var images = await _manager.GetFilteredAsync(filter, cancellationToken);
        var accessibleImages = images.Where(CanAccessImage).ToList();

        if (accessibleImages.Count == 0)
        {
            return NotFound("No se encontraron imagenes para descargar con los filtros actuales.");
        }

        var fileName = BuildZipFileName("galeria");
        return await BuildZipResultAsync(accessibleImages, fileName, cancellationToken);
    }

    [Authorize(Roles = "Admin,Contributor")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DermaImgResponseDto>> Create(
        [FromForm] CreateDermaImgDto dto,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Debe seleccionar una imagen para subir.");
        }

        var savedFile = await _imageUploadManager.SaveUploadedFileAsync(file, cancellationToken);

        dto.FileName = savedFile.StoredFileName;
        dto.FilePath = savedFile.FullPath;
        dto.ContentType = savedFile.ContentType;
        dto.FileSize = savedFile.FileSize;

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var activeReviewers = await _userManager.GetActiveUsersByRoleAsync(UserRole.Reviewer, cancellationToken);
        if (activeReviewers.Count == 0)
        {
            return BadRequest(new { message = "No hay especialistas asignados con rol REVIEWER para revisar la subida. Contacta al administrador." });
        }

        // Contributors can only create images under their own identity.
        // Admins may optionally pass a specific contributor id.
        if (User.IsInRole("Admin") && dto.ContributorId != Guid.Empty)
        {
            // keep provided contributor id
        }
        else
        {
            dto.ContributorId = userId.Value;
        }

        var contributor = await _userManager.GetByIdAsync(dto.ContributorId, cancellationToken);
        if (contributor is null)
        {
            return NotFound("No se encontró el contribuidor asociado a la carga.");
        }

        dto.InstitutionId = contributor.InstitutionId;

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

    [Authorize(Roles = "Admin,Contributor")]
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
        if (!isAdmin && existing.ContributorId != userId.Value)
        {
            return Forbid();
        }

        dto.ContributorId = existing.ContributorId;
        dto.FileName = existing.FileName;
        dto.FilePath = existing.FilePath;
        dto.ContentType = existing.ContentType;
        dto.FileSize = existing.FileSize;

        var contributor = await _userManager.GetByIdAsync(existing.ContributorId, cancellationToken);
        dto.InstitutionId = contributor?.InstitutionId;

        if (!isAdmin)
        {
            existing.ApprovalStatus = ImageApprovalStatus.Pending;
            existing.ReviewedByUserId = null;
            existing.ReviewedAt = null;
            existing.ReviewComment = null;
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

        await _manager.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin,Contributor")]
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
        if (!isAdmin && existing.ContributorId != userId.Value)
        {
            return Forbid();
        }

        await _manager.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin,Contributor,Reviewer")]
    [HttpGet("review-requests/mine")]
    public async Task<ActionResult<PagedResponse<DermaImgResponseDto>>> GetMyReviewRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ImageApprovalStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);

        var filter = new DermaImgFilter
        {
            ContributorId = userId.Value,
            ApprovalStatuses = status.HasValue ? [status.Value] : null,
        };

        var (items, totalCount) = await _manager.GetPagedAsync(validPage, validPageSize, filter, cancellationToken);
        return Ok(new PagedResponse<DermaImgResponseDto>
        {
            Items = items.Select(MapToResponseDto),
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize,
        });
    }

    [Authorize(Roles = "Admin,Reviewer")]
    [HttpGet("review-requests/inbox")]
    public async Task<ActionResult<PagedResponse<DermaImgResponseDto>>> GetReviewInbox(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeReviewed = false,
        CancellationToken cancellationToken = default)
    {
        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);

        var filter = new DermaImgFilter
        {
            ApprovalStatuses = includeReviewed ? null : [ImageApprovalStatus.Pending],
        };

        var (items, totalCount) = await _manager.GetPagedAsync(validPage, validPageSize, filter, cancellationToken);
        return Ok(new PagedResponse<DermaImgResponseDto>
        {
            Items = items.Select(MapToResponseDto),
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize,
        });
    }

    [Authorize(Roles = "Admin,Reviewer")]
    [HttpPost("{id:guid}/review")]
    public async Task<ActionResult<DermaImgResponseDto>> ReviewUpload(Guid id, [FromBody] ReviewImageUploadDto dto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var image = await _manager.GetByIdAsync(id, cancellationToken);
        if (image is null)
        {
            return NotFound();
        }

        if (image.ApprovalStatus != ImageApprovalStatus.Pending)
        {
            return BadRequest(new { message = "La imagen ya fue revisada previamente." });
        }

        var nextStatus = dto.Approve ? ImageApprovalStatus.Approved : ImageApprovalStatus.Declined;
        await _manager.ReviewUploadAsync(id, userId.Value, nextStatus, dto.Comment, cancellationToken);

        var reviewed = await _manager.GetByIdAsync(id, cancellationToken);
        if (reviewed is null)
        {
            return NotFound();
        }

        return Ok(MapToResponseDto(reviewed));
    }

    private static string BuildZipFileName(string suffix)
    {
        var safeSuffix = string.IsNullOrWhiteSpace(suffix)
            ? "imagenes"
            : $"imagenes-{suffix.Trim().ToLowerInvariant()}";
        return $"dermauh-{safeSuffix}-{DateTime.UtcNow:yyyyMMdd-HHmm}.zip";
    }

    private static async Task<FileContentResult> BuildZipResultAsync(
        IReadOnlyList<DermaImg> images,
        string fileName,
        CancellationToken cancellationToken)
    {
        var missingFiles = new List<string>();
        await using var archiveStream = new MemoryStream();

        using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var csvEntry = archive.CreateEntry("metadata.csv", CompressionLevel.Fastest);
            await using (var entryStream = csvEntry.Open())
            await using (var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
            {
                var csv = ImageMetadataCatalog.BuildImagesCsv(images);
                await writer.WriteAsync(csv);
            }

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

            if (missingFiles.Count > 0)
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
        if (image.ApprovalStatus == ImageApprovalStatus.Approved && image.IsPublic)
        {
            return true;
        }

        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return false;
        }

        if (User.IsInRole("Admin") || User.IsInRole("Reviewer"))
        {
            return true;
        }

        var userId = GetCurrentUserId();
        return userId.HasValue && image.ContributorId == userId.Value;
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
            FileName = image.FileName,
            FilePath = image.FilePath,
            ContentType = image.ContentType,
            FileSize = image.FileSize,
            IsPublic = image.IsPublic,
            ApprovalStatus = image.ApprovalStatus,
            ReviewComment = image.ReviewComment,
            ReviewedAt = image.ReviewedAt,
            ReviewedByUserId = image.ReviewedByUserId,
            ReviewedByFullName = image.ReviewedByUser is null
                ? null
                : $"{image.ReviewedByUser.FirstName} {image.ReviewedByUser.LastName}".Trim(),
            ImageType = image.ImageType,
            ImageManipulation = image.ImageManipulation,
            DermoscopicType = image.DermoscopicType,
            AgeApprox = image.AgeApprox,
            Sex = image.Sex,
            FotoType = image.FotoType,
            PersonalHxMm = image.PersonalHxMm,
            FamilyHxMm = image.FamilyHxMm,
            AnatomSiteGeneral = image.AnatomSiteGeneral,
            AnatomSiteSpecial = image.AnatomSiteSpecial,
            ClinSizeLongDiamMm = image.ClinSizeLongDiamMm,
            Diagnosis = image.Diagnosis,
            DiagnosisCategory = image.DiagnosisCategory,
            InjuryType = image.InjuryType,
            DiagnosisConfirmType = image.DiagnosisConfirmType,
            MelThickMm = image.MelThickMm,
            MelMitoticIndex = image.MelMitoticIndex,
            MelUlcer = image.MelUlcer,
            ClinicalNotes = image.ClinicalNotes,
            CreatedAt = image.CreatedAt,
            ContributorId = image.ContributorId,
            ContributorFullName = image.Contributor?.FullName,
            InstitutionId = image.InstitutionId,
            InstitutionName = image.Institution?.Name
        };
    }
}
