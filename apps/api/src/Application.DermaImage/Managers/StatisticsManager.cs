using System.Globalization;
using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;

namespace Application.DermaImage.Managers;

public class StatisticsManager : IStatisticsManager
{
    private readonly IDermaImgRepository _images;
    public StatisticsManager(IDermaImgRepository images)
    {
        _images = images;
    }

    public async Task<StatisticsOverviewDto> GetOverviewAsync(
        bool includePrivate,
        int recentMonths = 6,
        int topInstitutions = 5,
        CancellationToken cancellationToken = default)
    {
        recentMonths = Math.Clamp(recentMonths, 3, 24);
        topInstitutions = Math.Clamp(topInstitutions, 3, 20);

        // EF Core DbContext is scoped and not thread-safe; run these queries sequentially.
        var totalImages = await _images.CountByVisibilityAsync(includePrivate, cancellationToken);
        var publicImages = includePrivate
            ? await _images.CountByVisibilityAsync(false, cancellationToken)
            : totalImages;
        var derivedInstitutions = await _images.GetDerivedInstitutionsAsync(includePrivate, cancellationToken);
        var institutionsCount = derivedInstitutions.Count;
        var contributorsCount = await _images.CountDistinctContributorsAsync(includePrivate, cancellationToken);

        // ── Existing distributions ─────────────────────────────────────
        var diagnosis = await _images.GetDiagnosisCategoryCountsAsync(includePrivate, cancellationToken);
        var injuryType = await _images.GetInjuryTypeCountsAsync(includePrivate, cancellationToken);
        var photoType = await _images.GetPhotoTypeCountsAsync(includePrivate, cancellationToken);
        var sex = await _images.GetSexCountsAsync(includePrivate, cancellationToken);
        var site = await _images.GetAnatomicalSiteCountsAsync(includePrivate, cancellationToken);
        var monthly = await _images.GetMonthlyUploadCountsAsync(recentMonths, includePrivate, cancellationToken);

        // ── New distributions ──────────────────────────────────────────
        var skinColor = await _images.GetSkinColorCountsAsync(includePrivate, cancellationToken);
        var diagConfirm = await _images.GetDiagnosisConfirmCountsAsync(includePrivate, cancellationToken);
        var imageType = await _images.GetImageTypeCountsAsync(includePrivate, cancellationToken);

        // ── Age statistics ─────────────────────────────────────────────
        var ages = await _images.GetAgeValuesAsync(includePrivate, cancellationToken);
        var ageGroups = BuildAgeGroupDistribution(ages);
        var (ageMean, ageMedian, ageStdDev) = ComputeAgeStats(ages);

        // ── Cross-tabulations ──────────────────────────────────────────
        var diagBySex = await _images.GetCrossTabAsync("DiagnosisCategory", "Sex", includePrivate, cancellationToken);
        var diagBySite = await _images.GetCrossTabAsync("DiagnosisCategory", "AnatomSiteGeneral", includePrivate, cancellationToken);
        var injuryByAge = await _images.GetCrossTabAsync("InjuryType", "AgeGroup", includePrivate, cancellationToken);
        var diagBySkinColor = await _images.GetCrossTabAsync("DiagnosisCategory", "SkinColor", includePrivate, cancellationToken);

        // ── Melanoma body map ──────────────────────────────────────────
        var melBodyMap = await _images.GetMelanomaByAnatomicalSiteAsync(includePrivate, cancellationToken);
        var melanomaBodyMap = BuildMelanomaBodyMap(melBodyMap);

        // ── Data completeness ──────────────────────────────────────────
        var completeness = await _images.GetDataCompletenessAsync(includePrivate, cancellationToken);

        return new StatisticsOverviewDto
        {
            TotalImages = totalImages,
            PublicImages = publicImages,
            PrivateImages = Math.Max(0, totalImages - publicImages),
            InstitutionsCount = institutionsCount,
            ContributorsCount = contributorsCount,
            DiagnosisDistribution = MapBuckets(diagnosis, MapDiagnosisLabel),
            InjuryTypeDistribution = MapBuckets(injuryType, MapInjuryTypeLabel),
            PhotoTypeDistribution = MapBuckets(photoType, MapPhotoTypeLabel),
            SexDistribution = MapBuckets(sex, MapSexLabel),
            AnatomicalSiteDistribution = MapBuckets(site, MapAnatomicalSiteLabel),
            MonthlyUploads = BuildMonthlySeries(monthly, recentMonths),
            SkinColorDistribution = MapBuckets(skinColor, MapSkinColorLabel),
            DiagnosisConfirmDistribution = MapBuckets(diagConfirm, MapDiagnosisConfirmLabel),
            ImageTypeDistribution = MapBuckets(imageType, MapImageTypeLabel),
            AgeGroupDistribution = ageGroups,
            AgeMedian = ageMedian,
            AgeMean = ageMean,
            AgeStdDev = ageStdDev,
            DiagnosisBySex = MapCrossTab(diagBySex, MapDiagnosisLabel, MapSexLabel),
            DiagnosisByAnatomicalSite = MapCrossTab(diagBySite, MapDiagnosisLabel, MapAnatomicalSiteLabel),
            InjuryTypeByAgeGroup = MapCrossTab(injuryByAge, MapInjuryTypeLabel, MapAgeGroupLabel),
            DiagnosisBySkinColor = MapCrossTab(diagBySkinColor, MapDiagnosisLabel, MapSkinColorLabel),
            MelanomaBodyMap = melanomaBodyMap,
            DataCompleteness = completeness.Select(x => new DataCompletenessDto
            {
                FieldName = x.FieldName,
                FieldLabel = MapCompletenessLabel(x.FieldName),
                FilledCount = x.FilledCount,
                TotalCount = x.TotalCount,
                Percentage = x.TotalCount > 0
                    ? Math.Round((x.FilledCount * 100d) / x.TotalCount, 1)
                    : 0
            }).ToList()
        };
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private static IReadOnlyList<StatisticsBucketDto> MapBuckets(
        IReadOnlyList<(string Key, int Count)> data,
        Func<string, string> labelMapper)
    {
        var total = Math.Max(1, data.Sum(x => x.Count));

        return data
            .Select(x => new StatisticsBucketDto
            {
                Key = x.Key,
                Label = labelMapper(x.Key),
                Count = x.Count,
                Percentage = Math.Round((x.Count * 100d) / total, 1)
            })
            .ToList();
    }

    private static IReadOnlyList<CrossTabBucketDto> MapCrossTab(
        IReadOnlyList<(string RowKey, string ColKey, int Count)> data,
        Func<string, string> rowLabelMapper,
        Func<string, string> colLabelMapper)
    {
        return data.Select(x => new CrossTabBucketDto
        {
            RowKey = x.RowKey,
            RowLabel = rowLabelMapper(x.RowKey),
            ColKey = x.ColKey,
            ColLabel = colLabelMapper(x.ColKey),
            Count = x.Count
        }).ToList();
    }

    private static IReadOnlyList<StatisticsBucketDto> BuildAgeGroupDistribution(IReadOnlyList<int> ages)
    {
        if (ages.Count == 0) return [];

        var groups = new[] { "0-17", "18-29", "30-39", "40-49", "50-59", "60-69", "70-79", "80+" };
        var counts = new Dictionary<string, int>();
        foreach (var g in groups) counts[g] = 0;

        foreach (var age in ages)
        {
            var group = age switch
            {
                < 18 => "0-17",
                < 30 => "18-29",
                < 40 => "30-39",
                < 50 => "40-49",
                < 60 => "50-59",
                < 70 => "60-69",
                < 80 => "70-79",
                _ => "80+"
            };
            counts[group]++;
        }

        var total = Math.Max(1, ages.Count);
        return groups
            .Select(g => new StatisticsBucketDto
            {
                Key = g,
                Label = g + " años",
                Count = counts[g],
                Percentage = Math.Round((counts[g] * 100d) / total, 1)
            })
            .ToList();
    }

    private static (double? Mean, double? Median, double? StdDev) ComputeAgeStats(IReadOnlyList<int> ages)
    {
        if (ages.Count == 0) return (null, null, null);

        var sorted = ages.OrderBy(a => a).ToList();
        var mean = sorted.Average();
        var median = sorted.Count % 2 == 0
            ? (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2.0
            : sorted[sorted.Count / 2];
        var variance = sorted.Average(a => (a - mean) * (a - mean));
        var stdDev = Math.Sqrt(variance);

        return (Math.Round(mean, 1), Math.Round(median, 1), Math.Round(stdDev, 1));
    }

    private static IReadOnlyList<MonthlyUploadDto> BuildMonthlySeries(
        IReadOnlyList<(int Year, int Month, int Count)> data,
        int recentMonths)
    {
        var byMonth = data.ToDictionary(
            x => $"{x.Year:D4}-{x.Month:D2}",
            x => x.Count);

        var firstMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
            .AddMonths(-(recentMonths - 1));

        var result = new List<MonthlyUploadDto>(recentMonths);
        for (var i = 0; i < recentMonths; i++)
        {
            var current = firstMonth.AddMonths(i);
            var monthKey = $"{current:yyyy-MM}";

            byMonth.TryGetValue(monthKey, out var count);
            result.Add(new MonthlyUploadDto
            {
                MonthKey = monthKey,
                Label = current.ToString("MMM yy", CultureInfo.GetCultureInfo("es-ES")),
                Count = count
            });
        }

        return result;
    }

    private static IReadOnlyList<BodyMapEntryDto> BuildMelanomaBodyMap(
        IReadOnlyList<(string SiteKey, int MelanomaCount, int TotalCount)> melBodyMap)
    {
        var knownKeys = Enum.GetNames<AnatomSiteGeneral>();
        var knownSet = new HashSet<string>(knownKeys, StringComparer.Ordinal);
        var allKeys = knownKeys
            .Concat(melBodyMap.Select(x => x.SiteKey).Where(key => !knownSet.Contains(key)))
            .ToList();

        var byKey = melBodyMap.ToDictionary(x => x.SiteKey, StringComparer.Ordinal);

        return allKeys.Select(key =>
        {
            var entry = byKey.TryGetValue(key, out var value)
                ? value
                : (SiteKey: key, MelanomaCount: 0, TotalCount: 0);

            var percentage = entry.TotalCount > 0
                ? Math.Round((entry.MelanomaCount * 100d) / entry.TotalCount, 1)
                : 0;

            return new BodyMapEntryDto
            {
                SiteKey = entry.SiteKey,
                SiteLabel = MapAnatomicalSiteLabel(entry.SiteKey),
                MelanomaCount = entry.MelanomaCount,
                TotalCount = entry.TotalCount,
                MelanomaPercentage = percentage
            };
        }).ToList();
    }

    // ── Label mappers ──────────────────────────────────────────────────

    private static string MapDiagnosisLabel(string key) => key switch
    {
        "Benign" => "Benigno",
        "Indeterminate" => "Indeterminado",
        "Malignant" => "Maligno",
        _ => key
    };

    private static string MapSexLabel(string key) => key switch
    {
        "Male" => "Masculino",
        "Female" => "Femenino",
        _ => key
    };

    private static string MapPhotoTypeLabel(string key) => key;

    private static string MapInjuryTypeLabel(string key) => key switch
    {
        "Melanoma" => "Melanoma",
        "BasalCellCarcinoma" => "Carcinoma basocelular",
        "SquamousCellCarcinoma" => "Carcinoma escamocelular",
        "Others" => "Otros",
        _ => key
    };

    private static string MapAnatomicalSiteLabel(string key) => key switch
    {
        "HeadNeck" => "Cabeza y cuello",
        "UpperExtremity" => "Extremidad superior",
        "LowerExtremity" => "Extremidad inferior",
        "AnteriorTorso" => "Torso anterior",
        "LateralTorso" => "Torso lateral",
        "PosteriorTorso" => "Torso posterior",
        "PalmsSoles" => "Palmas y plantas",
        "OralGenital" => "Oral y genital",
        _ => key
    };

    private static string MapSkinColorLabel(string key) => key switch
    {
        "White" => "Blanco",
        "Mixed_Race" => "Mestizo",
        "Black" => "Negro",
        _ => key
    };

    private static string MapDiagnosisConfirmLabel(string key) => key switch
    {
        "Histopathology" => "Histopatología",
        "SingleContributorClinicalAssessment" => "Evaluación clínica individual",
        "SerialImagingShowingNoChange" => "Imágenes seriadas sin cambio",
        "SingleImageExpertConsensus" => "Consenso de expertos",
        "ConfocalMicroscopyWithConsensusDermoscopy" => "Microscopía confocal",
        _ => key
    };

    private static string MapImageTypeLabel(string key) => key switch
    {
        "Dermoscopic" => "Dermoscópica",
        "ClinicalOverview" => "Clínica: Vista General",
        "ClinicalCloseUp" => "Clínica: Primer Plano",
        "TBPTileOverview" => "TBP: Vista General",
        "TBPTileCloseUp" => "TBP: Primer Plano",
        "RCMMacroscopic" => "RCM: Macroscópica",
        "RCMTile" => "RCM: Tile",
        "RCMMosaic" => "RCM: Mosaico",
        _ => key
    };

    private static string MapAgeGroupLabel(string key) => key + " años";

    private static string MapCompletenessLabel(string key) => key switch
    {
        "AgeApprox" => "Edad",
        "Sex" => "Sexo",
        "SkinColor" => "Color de piel",
        "FotoType" => "Fototipo",
        "AnatomSiteGeneral" => "Sitio anatómico",
        "DiagnosisCategory" => "Categoría diagnóstica",
        "InjuryType" => "Tipo de lesión",
        "DiagnosisConfirmType" => "Confirmación diagnóstica",
        "ImageType" => "Tipo de imagen",
        "Diagnosis" => "Diagnóstico",
        "ClinSizeLongDiamMm" => "Tamaño de lesión",
        "PersonalHxMm" => "Hist. personal melanoma",
        "FamilyHxMm" => "Hist. familiar melanoma",
        "SunExposure" => "Exposición solar",
        _ => key
    };
}
