using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class RerunOrderResponse
{
    [Display("Success")]
    public bool Success { get; set; }

    [Display("Message")]
    public string? Message { get; set; }

    [Display("Order ID")]
    public string? OrderId { get; set; }

    [Display("Re-ran full chain")]
    public bool TriggerFullChain { get; set; }

    [Display("Triggered order IDs")]
    public List<string> TriggeredOrderIds { get; set; } = [];
}
