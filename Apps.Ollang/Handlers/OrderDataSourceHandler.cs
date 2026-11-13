using Apps.Ollang.Extensions;
using Apps.Ollang.Models.Responses;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Ollang.Handlers;

public class OrderDataSourceHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var orders = await Client.Paginate<OrderSummaryResponse>(page =>
        {
            var request = new RestRequest("/integration/orders", Method.Get)
                .AddQueryParameter("pageOptions[page]", page)
                .AddQueryParameter("pageOptions[take]", 50)
                .AddQueryParameter("pageOptions[orderBy]", "createdAt")
                .AddQueryParameter("pageOptions[orderDirection]", "desc");

            request.AddOptionalParameter(
                "pageOptions[search]",
                context.SearchString,
                ParameterType.QueryString);

            return request;
        }, 30, cancellationToken);

        return orders.Select(order => new DataSourceItem(
            order.OrderId,
            BuildDisplayName(order)));
    }

    private static string BuildDisplayName(OrderSummaryResponse order)
    {
        var name = string.IsNullOrWhiteSpace(order.Name) ? order.OrderId : order.Name;
        var language = string.IsNullOrWhiteSpace(order.TargetLanguage) ? null : order.TargetLanguage;
        var status = string.IsNullOrWhiteSpace(order.Status) ? null : order.Status;
        var details = string.Join(", ", new[] { language, status }.Where(value => value is not null));
        return string.IsNullOrEmpty(details) ? name : $"{name} ({details})";
    }
}
