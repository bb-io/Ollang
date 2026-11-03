using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class ReferenceDocumentResponse
{
    [Display("Document ID")]
    public string Id { get; set; } = string.Empty;

    [Display("Name")]
    public string? Name { get; set; }

    [Display("Document type")]
    public string? Type { get; set; }

    [Display("Size")]
    public long? Size { get; set; }

    [Display("Source language")]
    public string? SourceLanguage { get; set; }

    [Display("Project ID")]
    public string? ProjectId { get; set; }

    [Display("Folder ID")]
    public string? FolderId { get; set; }

    [Display("Created at")]
    public DateTime? CreatedAt { get; set; }
}
