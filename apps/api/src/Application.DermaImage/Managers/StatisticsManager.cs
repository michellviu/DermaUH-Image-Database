using System.Globalization;
using Application.DermaImage.DTOs;
using Domain.DermaImage.Interfaces.Repository;

namespace Application.DermaImage.Managers;

public class StatisticsManager : IStatisticsManager
{
    private readonly IDermaImgRepository _images;
    private readonly IInstitutionRepository _institutions;

    public StatisticsManager(
        IDermaImgRepository images,
        IInstitutionRepository institutions)
    {
        _images = images;
        _institutions = institutions;
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
        var institutionsCount = await _institutions.CountAsync(cancellationToken: cancellationToken);
        var contributorsCount = await _images.CountDistinctContributorsAsync(includePrivate, cancellationToken);

        var diagnosis = await _images.GetDiagnosisCategoryCountsAsync(includePrivate, cancellationToken);
        var sex = await _images.GetSexCountsAsync(includePrivate, cancellationToken);
        var site = await _images.GetAnatomicalSiteCountsAsync(includePrivate, cancellationToken);
        var monthly = await _images.GetMonthlyUploadCountsAsync(recentMonths, includePrivate, cancellationToken);
        var topInstitutionsData = await _images.GetTopInstitutionsByImageCountAsync(topInstitutions, includePrivate, cancellationToken);

        return new StatisticsOverviewDto
        {
            TotalImages = totalImages,
            PublicImages = publicImages,
            PrivateImages = Math.Max(0, totalImages - publicImages),
            InstitutionsCount = institutionsCount,
            ContributorsCount = contributorsCount,
            DiagnosisDistribution = MapBuckets(diagnosis, MapDiagnosisLabel),
            SexDistribution = MapBuckets(sex, MapSexLabel),
            AnatomicalSiteDistribution = MapBuckets(site, MapAnatomicalSiteLabel),
            MonthlyUploads = BuildMonthlySeries(monthly, recentMonths),
            TopInstitutions = topInstitutionsData
                .Select(i => new TopInstitutionDto
                {
                    InstitutionId = i.InstitutionId,
                    InstitutionName = i.InstitutionName,
                    ImageCount = i.Count
                })
                .ToList()
        };
    }

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
                Label = current.ToString("MMM yy", CultureInfo.InvariantCulture),
                Count = count
            });
        }

        return result;
    }

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
}
