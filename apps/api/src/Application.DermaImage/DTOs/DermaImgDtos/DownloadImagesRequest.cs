namespace Application.DermaImage.DTOs;

public class DownloadImagesRequest
{
    public List<Guid> ImageIds { get; set; } = [];
}
