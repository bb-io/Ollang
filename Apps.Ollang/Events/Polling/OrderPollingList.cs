using Apps.Ollang.Events.Polling.Models;
using Apps.Ollang.Models.Responses;
using Apps.Ollang.Utils;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using RestSharp;

namespace Apps.Ollang.Events.Polling;

[PollingEventList]
public class OrderPollingList(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [PollingEvent("On order status changed",
        Description = "Triggered when the selected order reaches completed, failed, or stalled status.")]
    public async Task<PollingEventResponse<OrderStatusPollingMemory, OrderStatusChangedResponse>>
        OnOrderStatusChanged(
            PollingEventRequest<OrderStatusPollingMemory> request,
            [PollingEventParameter] OrderStatusChangedFilter filter)
    {
        ActionInputValidator.ValidateRequired(filter.OrderId, "Order ID");
        var order = await GetOrder(filter.OrderId);
        var currentStatus = order.Status ?? string.Empty;
        var previousStatus = request.Memory?.LastStatus ?? string.Empty;
        var statusChanged = request.Memory is null || !string.Equals(
            previousStatus,
            currentStatus,
            StringComparison.OrdinalIgnoreCase);
        var shouldFly = statusChanged && IsTerminalStatus(currentStatus);

        return new PollingEventResponse<OrderStatusPollingMemory, OrderStatusChangedResponse>
        {
            FlyBird = shouldFly,
            Memory = new OrderStatusPollingMemory { LastStatus = currentStatus },
            Result = shouldFly ? MapChangedOrder(order, previousStatus) : null
        };
    }

    private Task<OrderResponse> GetOrder(string orderId)
    {
        var request = new RestRequest(
            $"/integration/orders/{Uri.EscapeDataString(orderId)}",
            Method.Get);
        return Client.ExecuteWithErrorHandling<OrderResponse>(request);
    }

    private static OrderStatusChangedResponse MapChangedOrder(
        OrderResponse order,
        string previousStatus) => new()
    {
        OrderId = order.OrderId,
        Name = order.Name,
        SourceLanguage = order.SourceLanguage,
        TargetLanguage = order.TargetLanguage,
        Status = order.Status,
        Type = order.Type,
        CreatedAt = order.CreatedAt,
        ProjectId = order.ProjectId,
        PreviousStatus = previousStatus
    };

    private static bool IsTerminalStatus(string status) =>
        status.Equals("completed", StringComparison.OrdinalIgnoreCase) ||
        status.Equals("failed", StringComparison.OrdinalIgnoreCase) ||
        status.Equals("stalled", StringComparison.OrdinalIgnoreCase);
}
