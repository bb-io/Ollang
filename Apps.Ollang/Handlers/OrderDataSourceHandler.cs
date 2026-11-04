using Apps.Ollang.Dtos;
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
        var orders = new List<OrderSummaryResponse>();
        var page = 1;

        while (orders.Count < 30)
        {
            var request = new RestRequest("/integration/orders", Method.Get)
                .AddQueryParameter("pageOptions[page]", page)
                .AddQueryParameter("pageOptions[take]", 50)
                .AddQueryParameter("pageOptions[orderBy]", "createdAt")
                .AddQueryParameter("pageOptions[orderDirection]", "desc");

            if (!string.IsNullOrWhiteSpace(context.SearchString))
                request.AddQueryParameter("pageOptions[search]", context.SearchString);

            var response = await Client.ExecuteWithErrorHandling<PagedResponseDto<OrderSummaryResponse>>(
                request,
                cancellationToken);
            orders.AddRange(response.Data);

            if (!response.Meta.HasNextPage)
                break;

            page++;
        }

        return orders.Take(30).Select(order => new DataSourceItem(
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
