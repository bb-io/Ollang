using Apps.Ollang.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Models.Identifiers;

public class OrderIdentifier
{
    [Display("Order ID", Description = "Order to use.")]
    [DataSource(typeof(OrderDataSourceHandler))]
    public string OrderId { get; set; } = string.Empty;
}
