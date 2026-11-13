using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class SubtitleEmbeddingResponse
{
    [Display("Success")]
    public bool Success { get; set; }

    [Display("Message")]
    public string? Message { get; set; }

    [Display("Order ID")]
    public string? OrderId { get; set; }
}
