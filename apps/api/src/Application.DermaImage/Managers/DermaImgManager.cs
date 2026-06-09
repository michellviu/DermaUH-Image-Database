using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;
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

    public async Task<IReadOnlyList<DermaImg>> GetFilteredAsync(DermaImgFilter? filter = null, CancellationToken cancellationToken = default)
    {
        return await _service.GetFilteredAsync(filter, cancellationToken);
    }

    public async Task<IReadOnlyList<DermaImg>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _service.GetByIdsAsync(ids, cancellationToken);
    }

    public async Task<DermaImg> CreateAsync(CreateDermaImgDto dto, CancellationToken cancellationToken = default)
    {
        var image = MapToEntity(dto);
        return await _service.CreateAsync(image, cancellationToken);
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
            ImageType = dto.ImageType,
            ImageManipulation = dto.ImageManipulation,
            DermoscopicType = dto.DermoscopicType,
            AgeApprox = dto.AgeApprox,
            Sex = dto.Sex,
            SkinColor = dto.SkinColor,
            FotoType = dto.FotoType,
            PersonalHistory = dto.PersonalHistory,
            PersonalHxMm = dto.PersonalHxMm,
            FamilyHxMm = dto.FamilyHxMm,
            SunExposure = dto.SunExposure,
            Provincia = dto.Provincia,
            AnatomSiteGeneral = dto.AnatomSiteGeneral,
            AnatomSiteSpecial = dto.AnatomSiteSpecial,
            ClinSizeLongDiamMm = dto.ClinSizeLongDiamMm,
            Diagnosis = dto.Diagnosis,
            HistopathologicalDiagnosis = dto.HistopathologicalDiagnosis,
            DiagnosisCategory = dto.DiagnosisCategory,
            InjuryType = dto.InjuryType,
            DiagnosisConfirmType = dto.DiagnosisConfirmType,
            MelThickMm = dto.MelThickMm,
            MelMitoticIndex = dto.MelMitoticIndex,
            MelUlcer = dto.MelUlcer,
            ClinicalNotes = dto.ClinicalNotes,
            DermoscopicComments = dto.DermoscopicComments,
            InformedConsent = dto.InformedConsent,
            InformedConsentDate = dto.InformedConsentDate,
            InformedConsentText = dto.InformedConsentText,
            ContributorId = dto.ContributorId,
            InstitutionName = dto.InstitutionName,
            InstitutionDescription = dto.InstitutionDescription,
            InstitutionCountry = dto.InstitutionCountry
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
        entity.SkinColor = dto.SkinColor;
        entity.FotoType = dto.FotoType;
        entity.PersonalHistory = dto.PersonalHistory;
        entity.PersonalHxMm = dto.PersonalHxMm;
        entity.FamilyHxMm = dto.FamilyHxMm;
        entity.SunExposure = dto.SunExposure;
        entity.Provincia = dto.Provincia;
        entity.AnatomSiteGeneral = dto.AnatomSiteGeneral;
        entity.AnatomSiteSpecial = dto.AnatomSiteSpecial;
        entity.ClinSizeLongDiamMm = dto.ClinSizeLongDiamMm;
        entity.Diagnosis = dto.Diagnosis;
        entity.HistopathologicalDiagnosis = dto.HistopathologicalDiagnosis;
        entity.DiagnosisCategory = dto.DiagnosisCategory;
        entity.InjuryType = dto.InjuryType;
        entity.DiagnosisConfirmType = dto.DiagnosisConfirmType;
        entity.MelThickMm = dto.MelThickMm;
        entity.MelMitoticIndex = dto.MelMitoticIndex;
        entity.MelUlcer = dto.MelUlcer;
        entity.ClinicalNotes = dto.ClinicalNotes;
        entity.DermoscopicComments = dto.DermoscopicComments;
        entity.InformedConsent = dto.InformedConsent;
        entity.InformedConsentDate = dto.InformedConsentDate;
        entity.InformedConsentText = dto.InformedConsentText;
        entity.InstitutionName = dto.InstitutionName;
        entity.InstitutionDescription = dto.InstitutionDescription;
        entity.InstitutionCountry = dto.InstitutionCountry;
    }
}
