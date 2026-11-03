using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class OrderResponse
{
    [Display("Order ID")]
    public string Id { get; set; } = string.Empty;

    [Display("Name")]
    public string? Name { get; set; }

    [Display("Source language")]
    public string? SourceLanguage { get; set; }

    [Display("Target language")]
    public string? TargetLanguage { get; set; }

    [Display("Status")]
    public string? Status { get; set; }

    [Display("Type")]
    public string? Type { get; set; }

    [Display("Created at")]
    public DateTime? CreatedAt { get; set; }

    [Display("Project ID")]
    public string? ProjectId { get; set; }
}
