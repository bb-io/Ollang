using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Ollang.Models.Responses;

public class OrderResponse
{
    [Display("Order ID")]
    [JsonProperty("id")]
    public string OrderId { get; set; } = string.Empty;

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

    [Display("Rate")]
    public decimal? Rate { get; set; }

    [Display("Level")]
    public int? Level { get; set; }

    [Display("Folder ID")]
    public string? FolderId { get; set; }

    [Display("VTT URL", Description = "Signed VTT download URL when available.")]
    public string? VttUrl { get; set; }

    [Display("Documents")]
    public List<OrderDocumentResponse> OrderDocs { get; set; } = [];

    [Display("Delivery files")]
    public DeliveryFilesResponse? DeliveryFiles { get; set; }

    [Display("Finance")]
    public OrderFinanceResponse? Finance { get; set; }

    [Display("Latest evaluation")]
    public QcEvaluationResponse? LatestEvaluation { get; set; }

    [Display("Delivery events")]
    public List<DeliveryEventResponse> DeliveryEvents { get; set; } = [];
}
