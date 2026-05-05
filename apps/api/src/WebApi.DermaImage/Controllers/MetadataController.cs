using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DermaImage.Metadata;

namespace WebApi.DermaImage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("images")]
    public ActionResult<IEnumerable<ImageMetadataDefinition>> GetImageMetadata()
    {
        return Ok(ImageMetadataCatalog.Definitions);
    }

    [AllowAnonymous]
    [HttpGet("images/download")]
    public IActionResult DownloadImageMetadataCsv()
    {
        var csv = ImageMetadataCatalog.BuildDefinitionsCsv();
        var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv);
        var fileName = $"dermauh-metadata-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
