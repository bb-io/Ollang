using Apps.Ollang.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Events.Polling.Models;

public class OrderStatusChangedFilter
{
    [Display("Order ID", Description = "Order to monitor for status changes.")]
    [DataSource(typeof(OrderDataSourceHandler))]
    public string OrderId { get; set; } = string.Empty;
}
