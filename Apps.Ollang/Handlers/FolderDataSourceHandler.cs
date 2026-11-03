using Apps.Ollang.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Ollang.Handlers;

public class FolderDataSourceHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var folders = new List<FolderDto>();
        var page = 1;

        while (true)
        {
            var request = new RestRequest("/integration/folder", Method.Get)
                .AddQueryParameter("page", page)
                .AddQueryParameter("take", 50)
                .AddQueryParameter("orderBy", "name")
                .AddQueryParameter("orderDirection", "asc");

            if (!string.IsNullOrWhiteSpace(context.SearchString))
                request.AddQueryParameter("search", context.SearchString);

            var response = await Client.ExecuteWithErrorHandling<PagedResponseDto<FolderDto>>(
                request,
                cancellationToken);
            folders.AddRange(response.Data);

            if (!response.Meta.HasNextPage)
                break;

            page++;
        }

        return folders
            .Take(30)
            .Select(folder => new DataSourceItem(folder.Id, folder.Name));
    }
}
