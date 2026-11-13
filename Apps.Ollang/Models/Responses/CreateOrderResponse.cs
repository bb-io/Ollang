using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class CreateOrderResponse
{
    [Display("Order IDs")]
    public List<string> OrderIds { get; set; } = [];
}
