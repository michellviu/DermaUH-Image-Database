using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebApi.DermaImage.Managers;

namespace WebApi.DermaImage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IDermaImgManager _manager;
    private readonly IImageUploadManager _imageUploadManager;

    public ImagesController(
        IDermaImgManager manager,
        IImageUploadManager imageUploadManager)
    {
        _manager = manager;
        _imageUploadManager = imageUploadManager;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<DermaImg>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] List<ImageType>? imageTypes = null,
        [FromQuery] List<DiagnosisCategory>? diagnosisCategories = null,
        [FromQuery] List<Sex>? sexes = null,
        [FromQuery] List<AnatomSiteGeneral>? anatomSites = null,
        [FromQuery] bool? isPublic = null,
        [FromQuery] string? diagnosisContains = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new DermaImgFilter
        {
            ImageTypes = imageTypes,
            DiagnosisCategories = diagnosisCategories,
            Sexes = sexes,
            AnatomSites = anatomSites,
            IsPublic = isPublic,
            DiagnosisContains = diagnosisContains
        };

        var (items, totalCount) = await _manager.GetPagedAsync(page, pageSize, filter, cancellationToken);
        return Ok(new PagedResponse<DermaImg>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DermaImg>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var image = await _manager.GetByIdAsync(id, cancellationToken);
        return image is null ? NotFound() : Ok(image);
    }

    [HttpGet("public/{publicId}")]
    public async Task<ActionResult<DermaImg>> GetByPublicId(string publicId, CancellationToken cancellationToken)
    {
        var image = await _manager.GetByPublicIdAsync(publicId, cancellationToken);
        return image is null ? NotFound() : Ok(image);
    }

    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> GetPreview(Guid id, CancellationToken cancellationToken)
    {
        var image = await _manager.GetByIdAsync(id, cancellationToken);
        if (image is null)
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

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DermaImg>> Create(
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

        var created = await _manager.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateDermaImgDto dto, CancellationToken cancellationToken)
    {
        await _manager.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _manager.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
