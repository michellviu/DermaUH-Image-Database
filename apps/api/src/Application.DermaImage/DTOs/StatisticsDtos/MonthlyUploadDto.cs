namespace Application.DermaImage.DTOs;

public class MonthlyUploadDto
{
    public string MonthKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}
