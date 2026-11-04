using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class OrderDocumentResponse
{
    [Display("Document ID")]
    public string Id { get; set; } = string.Empty;

    [Display("Name")]
    public string? Name { get; set; }

    [Display("URL", Description = "Signed download URL.")]
    public string? Url { get; set; }

    [Display("Type")]
    public string? Type { get; set; }

    [Display("Size")]
    public long? Size { get; set; }

    [Display("Duration")]
    public double? Duration { get; set; }

    [Display("Word count")]
    public int? WordCount { get; set; }

    [Display("Source language")]
    public string? SourceLanguage { get; set; }

    [Display("Created at")]
    public DateTime? CreatedAt { get; set; }

    [Display("Updated at")]
    public DateTime? UpdatedAt { get; set; }
}
