using Apps.Ollang.Dtos;
using Apps.Ollang.Models.Requests;
using Apps.Ollang.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Ollang.Actions;

[ActionList("Orders")]
public class OrderActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Search orders", Description = "Search all Ollang orders matching the optional filters.")]
    public async Task<SearchOrdersResponse> SearchOrders([ActionParameter] SearchOrdersRequest input)
    {
        var orders = new List<OrderResponse>();
        var page = 1;

        while (true)
        {
            var request = new RestRequest("/integration/orders", Method.Get)
                .AddQueryParameter("pageOptions[page]", page)
                .AddQueryParameter("pageOptions[take]", 50)
                .AddQueryParameter("pageOptions[orderBy]", input.SortBy ?? "id")
                .AddQueryParameter("pageOptions[orderDirection]", input.SortDirection ?? "desc");

            AddOptionalQueryParameter(request, "pageOptions[search]", input.Search);
            AddOptionalQueryParameter(request, "filter[type]", input.OrderType);
            AddOptionalQueryParameter(request, "filter[name]", input.Name);

            if (input.CreatedFrom.HasValue)
                request.AddQueryParameter("filter[createdAtRange][from]", input.CreatedFrom.Value.ToString("O"));

            if (input.CreatedTo.HasValue)
                request.AddQueryParameter("filter[createdAtRange][to]", input.CreatedTo.Value.ToString("O"));

            var response = await Client.ExecuteWithErrorHandling<PagedResponseDto<OrderResponse>>(request);
            orders.AddRange(response.Data);

            if (!response.Meta.HasNextPage)
                break;

            page++;
        }

        return new SearchOrdersResponse { Orders = orders };
    }

    private static void AddOptionalQueryParameter(RestRequest request, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            request.AddQueryParameter(name, value);
    }
}
