using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class SearchOrdersResponse
{
    [Display("Orders")]
    public IEnumerable<OrderSummaryResponse> Orders { get; set; } = [];
}
