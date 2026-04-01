namespace Application.DermaImage.DTOs;

public class StatisticsOverviewDto
{
    public int TotalImages { get; set; }
    public int PublicImages { get; set; }
    public int PrivateImages { get; set; }
    public int InstitutionsCount { get; set; }
    public int ContributorsCount { get; set; }

    public IReadOnlyList<StatisticsBucketDto> DiagnosisDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> SexDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> AnatomicalSiteDistribution { get; set; } = [];
    public IReadOnlyList<MonthlyUploadDto> MonthlyUploads { get; set; } = [];
    public IReadOnlyList<TopInstitutionDto> TopInstitutions { get; set; } = [];
}

public class StatisticsBucketDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class MonthlyUploadDto
{
    public string MonthKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TopInstitutionDto
{
    public Guid? InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public int ImageCount { get; set; }
}
