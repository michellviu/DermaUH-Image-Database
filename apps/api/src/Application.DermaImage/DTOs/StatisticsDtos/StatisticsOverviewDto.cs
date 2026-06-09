namespace Application.DermaImage.DTOs;

public class StatisticsOverviewDto
{
    public int TotalImages { get; set; }
    public int PublicImages { get; set; }
    public int PrivateImages { get; set; }
    public int InstitutionsCount { get; set; }
    public int ContributorsCount { get; set; }

    // ── Existing distributions ─────────────────────────────────────────
    public IReadOnlyList<StatisticsBucketDto> DiagnosisDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> InjuryTypeDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> PhotoTypeDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> SexDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> AnatomicalSiteDistribution { get; set; } = [];
    public IReadOnlyList<MonthlyUploadDto> MonthlyUploads { get; set; } = [];

    // ── New distributions ──────────────────────────────────────────────
    public IReadOnlyList<StatisticsBucketDto> SkinColorDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> DiagnosisConfirmDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> AgeGroupDistribution { get; set; } = [];
    public IReadOnlyList<StatisticsBucketDto> ImageTypeDistribution { get; set; } = [];

    // ── Cross-tabulations ──────────────────────────────────────────────
    public IReadOnlyList<CrossTabBucketDto> DiagnosisBySex { get; set; } = [];
    public IReadOnlyList<CrossTabBucketDto> DiagnosisByAnatomicalSite { get; set; } = [];
    public IReadOnlyList<CrossTabBucketDto> InjuryTypeByAgeGroup { get; set; } = [];
    public IReadOnlyList<CrossTabBucketDto> DiagnosisBySkinColor { get; set; } = [];

    // ── Melanoma body map ──────────────────────────────────────────────
    public IReadOnlyList<BodyMapEntryDto> MelanomaBodyMap { get; set; } = [];

    // ── Province distribution ──────────────────────────────────────────
    public IReadOnlyList<StatisticsBucketDto> ProvinceDistribution { get; set; } = [];

    // ── Age statistics ─────────────────────────────────────────────────
    public double? AgeMedian { get; set; }
    public double? AgeMean { get; set; }
    public double? AgeStdDev { get; set; }

    // ── Data completeness ──────────────────────────────────────────────
    public IReadOnlyList<DataCompletenessDto> DataCompleteness { get; set; } = [];
}
