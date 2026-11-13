using Apps.Ollang.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Models.Requests;

public class RerunOrderRequest
{
    [Display("Order ID")]
    [DataSource(typeof(OrderDataSourceHandler))]
    public string OrderId { get; set; } = string.Empty;

    [Display("Re-run full chain", Description = "Also reset and re-run the connected dependency chain.")]
    public bool? RerunFullChain { get; set; }
}
