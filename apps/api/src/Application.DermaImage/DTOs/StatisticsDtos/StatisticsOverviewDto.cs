namespace Application.DermaImage.DTOs;

public class StatisticsOverviewDto
{
    public int TotalImages { get; set; }
    public int PublicImages { get; set; }
    public int PrivateImages { get; set; }
    public int InstitutionsCount { get; set; }
    public int ContributorsCount { get; set; }

    public IReadOnlyList<StatisticsBucketDto> DiagnosisDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> InjuryTypeDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> PhotoTypeDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> SexDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> AnatomicalSiteDistribution { get; set; } = [];
    public IReadOnlyList<MonthlyUploadDto> MonthlyUploads { get; set; } = [];
}
