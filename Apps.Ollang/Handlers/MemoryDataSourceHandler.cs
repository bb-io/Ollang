using Apps.Ollang.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Ollang.Handlers;

public class MemoryDataSourceHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest("/integration/memories", Method.Get);
        var memories = await Client.ExecuteWithErrorHandling<List<MemoryDto>>(request, cancellationToken);

        return memories
            .Where(memory => string.IsNullOrWhiteSpace(context.SearchString) ||
                             memory.Title.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(30)
            .Select(memory => new DataSourceItem(
                memory.Id,
                $"{memory.Title} ({memory.MemoryItemsCount} items)"));
    }
}
