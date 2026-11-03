using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class SearchOrdersResponse
{
    [Display("Orders")]
    public List<OrderResponse> Orders { get; set; } = [];
}
