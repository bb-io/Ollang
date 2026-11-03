using Apps.Ollang.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Ollang.Handlers;

public class ProjectDataSourceHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var projects = new List<ProjectDto>();
        var page = 1;

        while (true)
        {
            var request = new RestRequest("/integration/project", Method.Get)
                .AddQueryParameter("page", page)
                .AddQueryParameter("take", 50)
                .AddQueryParameter("orderBy", "name")
                .AddQueryParameter("orderDirection", "asc");

            if (!string.IsNullOrWhiteSpace(context.SearchString))
                request.AddQueryParameter("search", context.SearchString);

            var response = await Client.ExecuteWithErrorHandling<PagedResponseDto<ProjectDto>>(
                request,
                cancellationToken);
            projects.AddRange(response.Data);

            if (!response.Meta.HasNextPage)
                break;

            page++;
        }

        return projects
            .Take(30)
            .Select(project => new DataSourceItem(
                project.Id,
                string.IsNullOrWhiteSpace(project.SourceLanguage)
                    ? project.Name
                    : $"{project.Name} ({project.SourceLanguage})"));
    }
}
