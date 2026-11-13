using Apps.Ollang.Models.Responses;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Events.Polling.Models;

public class OrderStatusChangedResponse : OrderSummaryResponse
{
    [Display("Previous status")]
    public string PreviousStatus { get; set; } = string.Empty;
}
