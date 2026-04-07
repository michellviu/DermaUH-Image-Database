using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Application.DermaImage.Validation;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DermaImage.Managers;

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
        if (image is not null && !image.IsPublic && !CanReadPrivateImages())
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
        if (image is not null && !image.IsPublic && !CanReadPrivateImages())
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

        if (!image.IsPublic && !CanReadPrivateImages())
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

    private bool CanReadPrivateImages()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return false;
        }

        return User.IsInRole("Admin") || User.IsInRole("Reviewer") || User.IsInRole("Contributor");
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
