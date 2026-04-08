using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Services;

namespace Application.DermaImage.Managers;

public class DermaImgManager : IDermaImgManager
{
    private readonly IDermaImgService _service;

    public DermaImgManager(IDermaImgService service)
    {
        _service = service;
    }

    public async Task<(IEnumerable<DermaImg> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, DermaImgFilter? filter = null, CancellationToken cancellationToken = default)
    {
        return await _service.GetPagedAsync(page, pageSize, filter, cancellationToken);
    }

    public async Task<DermaImg?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _service.GetByIdAsync(id, cancellationToken);
    }

    public async Task<DermaImg?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        return await _service.GetByPublicIdAsync(publicId, cancellationToken);
    }

    public async Task<DermaImg> CreateAsync(CreateDermaImgDto dto, CancellationToken cancellationToken = default)
    {
        var image = MapToEntity(dto);
        return await _service.CreateAsync(image, cancellationToken);
    }

    public async Task<DermaImg> ReviewUploadAsync(Guid id, Guid reviewerUserId, ImageApprovalStatus status, string? reviewComment, CancellationToken cancellationToken = default)
    {
        var existing = await _service.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Image with id '{id}' was not found.");

        existing.ApprovalStatus = status;
        existing.ReviewedByUserId = reviewerUserId;
        existing.ReviewedAt = DateTime.UtcNow;
        existing.ReviewComment = string.IsNullOrWhiteSpace(reviewComment)
            ? null
            : reviewComment.Trim();

        await _service.UpdateAsync(existing, cancellationToken);
        return existing;
    }

    public async Task UpdateAsync(Guid id, CreateDermaImgDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _service.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Image with id '{id}' was not found.");

        MapToExistingEntity(dto, existing);
        await _service.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
    }

    private static DermaImg MapToEntity(CreateDermaImgDto dto)
    {
        return new DermaImg
        {
            FileName = dto.FileName,
            FilePath = dto.FilePath,
            ContentType = dto.ContentType,
            FileSize = dto.FileSize,
            IsPublic = dto.IsPublic,
            ApprovalStatus = ImageApprovalStatus.Pending,
            ReviewedAt = null,
            ReviewedByUserId = null,
            ReviewComment = null,
            ImageType = dto.ImageType,
            ImageManipulation = dto.ImageManipulation,
            DermoscopicType = dto.DermoscopicType,
            AgeApprox = dto.AgeApprox,
            Sex = dto.Sex,
            FotoType = dto.FotoType,
            PersonalHxMm = dto.PersonalHxMm,
            FamilyHxMm = dto.FamilyHxMm,
            AnatomSiteGeneral = dto.AnatomSiteGeneral,
            AnatomSiteSpecial = dto.AnatomSiteSpecial,
            ClinSizeLongDiamMm = dto.ClinSizeLongDiamMm,
            Diagnosis = dto.Diagnosis,
            DiagnosisCategory = dto.DiagnosisCategory,
            InjuryType = dto.InjuryType,
            DiagnosisConfirmType = dto.DiagnosisConfirmType,
            MelThickMm = dto.MelThickMm,
            MelMitoticIndex = dto.MelMitoticIndex,
            MelUlcer = dto.MelUlcer,
            ClinicalNotes = dto.ClinicalNotes,
            ContributorId = dto.ContributorId,
            InstitutionId = dto.InstitutionId
        };
    }

    private static void MapToExistingEntity(CreateDermaImgDto dto, DermaImg entity)
    {
        entity.IsPublic = dto.IsPublic;
        entity.ImageType = dto.ImageType;
        entity.ImageManipulation = dto.ImageManipulation;
        entity.DermoscopicType = dto.DermoscopicType;
        entity.AgeApprox = dto.AgeApprox;
        entity.Sex = dto.Sex;
        entity.FotoType = dto.FotoType;
        entity.PersonalHxMm = dto.PersonalHxMm;
        entity.FamilyHxMm = dto.FamilyHxMm;
        entity.AnatomSiteGeneral = dto.AnatomSiteGeneral;
        entity.AnatomSiteSpecial = dto.AnatomSiteSpecial;
        entity.ClinSizeLongDiamMm = dto.ClinSizeLongDiamMm;
        entity.Diagnosis = dto.Diagnosis;
        entity.DiagnosisCategory = dto.DiagnosisCategory;
        entity.InjuryType = dto.InjuryType;
        entity.DiagnosisConfirmType = dto.DiagnosisConfirmType;
        entity.MelThickMm = dto.MelThickMm;
        entity.MelMitoticIndex = dto.MelMitoticIndex;
        entity.MelUlcer = dto.MelUlcer;
        entity.ClinicalNotes = dto.ClinicalNotes;
    }
}
