using Apps.Ollang.Events.Polling;
using Apps.Ollang.Events.Polling.Models;
using Apps.Ollang.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.Ollang.Base;

namespace Tests.Ollang;

[TestClass]
public class OrderPollingTests : TestBase
{
    private OrderPollingList pollingList = null!;

    [TestInitialize]
    public void Initialize() => pollingList = new OrderPollingList(InvocationContext);

    [TestMethod]
    public async Task On_order_status_changed_with_null_memory_fires_for_completed_order()
    {
        var orderId = await GetCompletedOrderId();
        var response = await pollingList.OnOrderStatusChanged(
            new PollingEventRequest<OrderStatusPollingMemory> { Memory = null },
            new OrderStatusChangedFilter { OrderId = orderId });

        Assert.IsTrue(response.FlyBird);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual("completed", response.Result!.Status, true);
        Assert.IsNotNull(response.Memory);
        var memory = response.Memory!;
        Assert.AreEqual("completed", memory.LastStatus, true);
    }

    [TestMethod]
    public async Task On_order_status_changed_with_unchanged_terminal_status_does_not_fire_again()
    {
        var orderId = await GetCompletedOrderId();
        var baseline = await pollingList.OnOrderStatusChanged(
            new PollingEventRequest<OrderStatusPollingMemory> { Memory = null },
            new OrderStatusChangedFilter { OrderId = orderId });
        Assert.IsNotNull(baseline.Memory);
        var response = await pollingList.OnOrderStatusChanged(
            new PollingEventRequest<OrderStatusPollingMemory> { Memory = baseline.Memory },
            new OrderStatusChangedFilter { OrderId = orderId });

        Assert.IsFalse(response.FlyBird);
        Assert.IsNull(response.Result);
        Assert.IsNotNull(response.Memory);
        Assert.AreEqual("completed", response.Memory!.LastStatus, true);
    }

    private async Task<string> GetCompletedOrderId()
    {
        var handler = new OrderDataSourceHandler(InvocationContext);
        var order = (await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None))
            .FirstOrDefault(item => item.DisplayName.Contains("completed", StringComparison.OrdinalIgnoreCase));
        Assert.IsNotNull(order, "The integration account needs a completed order for polling tests.");
        return order!.Value;
    }
}
