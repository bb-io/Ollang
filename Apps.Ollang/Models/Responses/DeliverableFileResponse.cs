using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Ollang.Models.Responses;

public class DeliverableFileResponse
{
    [Display("File")]
    public FileReference File { get; set; } = default!;

    [Display("Document ID")]
    public string? DocumentId { get; set; }

    [Display("Name")]
    public string? Name { get; set; }

    [Display("Document type")]
    public string? DocumentType { get; set; }

    [Display("Language")]
    public string? Language { get; set; }

    [Display("Size")]
    public long? Size { get; set; }
}
