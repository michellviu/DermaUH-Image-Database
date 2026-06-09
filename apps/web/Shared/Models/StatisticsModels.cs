namespace Web.DermaImage.Shared.Models;

public class StatisticsOverviewDto
{
    public int TotalImages { get; set; }
    public int PublicImages { get; set; }
    public int PrivateImages { get; set; }
    public int InstitutionsCount { get; set; }
    public int ContributorsCount { get; set; }

    // ── Existing distributions ─────────────────────────────────────────
    public List<StatisticsBucketDto> DiagnosisDistribution { get; set; } = [];
    public List<StatisticsBucketDto> InjuryTypeDistribution { get; set; } = [];
    public List<StatisticsBucketDto> PhotoTypeDistribution { get; set; } = [];
    public List<StatisticsBucketDto> SexDistribution { get; set; } = [];
    public List<StatisticsBucketDto> AnatomicalSiteDistribution { get; set; } = [];
    public List<MonthlyUploadDto> MonthlyUploads { get; set; } = [];

    // ── New distributions ──────────────────────────────────────────────
    public List<StatisticsBucketDto> SkinColorDistribution { get; set; } = [];
    public List<StatisticsBucketDto> DiagnosisConfirmDistribution { get; set; } = [];
    public List<StatisticsBucketDto> AgeGroupDistribution { get; set; } = [];
    public List<StatisticsBucketDto> ImageTypeDistribution { get; set; } = [];

    // ── Cross-tabulations ──────────────────────────────────────────────
    public List<CrossTabBucketDto> DiagnosisBySex { get; set; } = [];
    public List<CrossTabBucketDto> DiagnosisByAnatomicalSite { get; set; } = [];
    public List<CrossTabBucketDto> InjuryTypeByAgeGroup { get; set; } = [];
    public List<CrossTabBucketDto> DiagnosisBySkinColor { get; set; } = [];

    // ── Melanoma body map ──────────────────────────────────────────────
    public List<BodyMapEntryDto> MelanomaBodyMap { get; set; } = [];

    // ── Province distribution ──────────────────────────────────────────
    public List<StatisticsBucketDto> ProvinceDistribution { get; set; } = [];

    // ── Age statistics ─────────────────────────────────────────────────
    public double? AgeMedian { get; set; }
    public double? AgeMean { get; set; }
    public double? AgeStdDev { get; set; }

    // ── Data completeness ──────────────────────────────────────────────
    public List<DataCompletenessDto> DataCompleteness { get; set; } = [];
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

public class CrossTabBucketDto
{
    public string RowKey { get; set; } = string.Empty;
    public string RowLabel { get; set; } = string.Empty;
    public string ColKey { get; set; } = string.Empty;
    public string ColLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class BodyMapEntryDto
{
    public string SiteKey { get; set; } = string.Empty;
    public string SiteLabel { get; set; } = string.Empty;
    public int MelanomaCount { get; set; }
    public int TotalCount { get; set; }
    public double MelanomaPercentage { get; set; }
}

public class DataCompletenessDto
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public int FilledCount { get; set; }
    public int TotalCount { get; set; }
    public double Percentage { get; set; }
}
