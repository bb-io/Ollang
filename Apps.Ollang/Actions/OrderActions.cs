using Apps.Ollang.Dtos;
using Apps.Ollang.Models.Identifiers;
using Apps.Ollang.Models.Requests;
using Apps.Ollang.Models.Responses;
using Apps.Ollang.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using RestSharp;

namespace Apps.Ollang.Actions;

[ActionList("Orders")]
public class OrderActions(
    InvocationContext invocationContext,
    IFileManagementClient fileManagementClient) : Invocable(invocationContext)
{
    [Action("Search orders", Description = "Search all Ollang orders matching the optional filters.")]
    public async Task<SearchOrdersResponse> SearchOrders([ActionParameter] SearchOrdersRequest input)
    {
        var orders = await Client.Paginate<OrderSummaryResponse>(page =>
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

            return request;
        });

        return new SearchOrdersResponse { Orders = orders };
    }

    [Action("Get order", Description = "Get complete order details, deliverables, QC results, and delivery history.")]
    public async Task<OrderResponse> GetOrder([ActionParameter] OrderIdentifier input)
    {
        ActionInputValidator.ValidateRequired(input.OrderId, "Order ID");
        var request = new RestRequest(
            $"/integration/orders/{Uri.EscapeDataString(input.OrderId)}",
            Method.Get);
        var order = await Client.ExecuteWithErrorHandling<OrderResponse>(request);
        NormalizeOrderCollections(order);
        return order;
    }

    [Action("Create order", Description = "Create one order per target language from an existing project or an uploaded source file.")]
    public async Task<CreateOrderResponse> CreateOrder([ActionParameter] CreateOrderRequest input)
    {
        var hasFile = input.File is not null;
        var hasProject = !string.IsNullOrWhiteSpace(input.ProjectId);
        if (hasFile == hasProject)
        {
            throw new PluginMisconfigurationException(
                "Provide either File or Project ID, but not both.");
        }

        ActionInputValidator.ValidateRequired(input.OrderType, "Order type");
        if (!int.TryParse(input.Level, out var level) || level is < 0 or > 1)
            throw new PluginMisconfigurationException("Level must be AI generated or Human review.");

        var targetLanguages = (input.TargetLanguages ?? [])
            .Where(language => !string.IsNullOrWhiteSpace(language))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (targetLanguages.Count == 0)
            throw new PluginMisconfigurationException("Select at least one Target language.");

        var rushLanguages = (input.RushTargetLanguages ?? [])
            .Where(language => !string.IsNullOrWhiteSpace(language))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var invalidRushLanguages = rushLanguages.Except(targetLanguages, StringComparer.OrdinalIgnoreCase).ToList();
        if (invalidRushLanguages.Count > 0)
        {
            throw new PluginMisconfigurationException(
                $"Every Rush target language must also be selected in Target languages: " +
                $"{string.Join(", ", invalidRushLanguages)}.");
        }

        var memoryIds = (input.MemoryIds ?? [])
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToList();
        if (memoryIds.Count > 20)
            throw new PluginMisconfigurationException("Select no more than 20 Memory IDs.");
        if (memoryIds.Distinct(StringComparer.OrdinalIgnoreCase).Count() != memoryIds.Count)
            throw new PluginMisconfigurationException("Memory IDs cannot contain duplicates.");

        var projectId = input.ProjectId;
        if (input.File is not null)
        {
            ActionInputValidator.ValidateRequired(input.FileName, "File name");
            ActionInputValidator.ValidateRequired(input.SourceLanguage, "Source language");

            var fileStream = await fileManagementClient.DownloadAsync(input.File);
            var fileBytes = await fileStream.GetByteData();
            var upload = await Client.UploadSourceFileAsync(
                fileBytes,
                input.File.Name,
                input.File.ContentType ?? "application/octet-stream",
                input.FileName!,
                input.SourceLanguage!,
                input.FolderId);
            projectId = upload.ProjectId;
        }

        var body = new Dictionary<string, object>
        {
            ["orderType"] = input.OrderType,
            ["level"] = level,
            ["projectId"] = projectId!,
            ["targetLanguageConfigs"] = targetLanguages.Select(language => new
            {
                language,
                isRush = rushLanguages.Contains(language)
            }).ToArray(),
            ["autoQc"] = input.RunQcAutomatically ?? false
        };

        if (!string.IsNullOrWhiteSpace(input.OrderSubtype))
            body["orderSubType"] = input.OrderSubtype;
        if (!string.IsNullOrWhiteSpace(input.DubbingStyle))
            body["dubbingStyle"] = input.DubbingStyle;
        if (!string.IsNullOrWhiteSpace(input.CallbackUrl))
            body["callbackUrl"] = input.CallbackUrl;
        if (memoryIds.Count > 0)
            body["selectedMemories"] = memoryIds;

        var request = new RestRequest("/integration/orders/create", Method.Post).AddJsonBody(body);
        var createdOrders = await Client.ExecuteWithErrorHandling<List<CreatedOrderDto>>(request);
        return new CreateOrderResponse
        {
            OrderIds = createdOrders.Select(order => order.OrderId).ToList()
        };
    }

    [Action("Request human review", Description = "Request professional human review for an eligible completed or delivered order.")]
    public async Task<SuccessResponse> RequestHumanReview([ActionParameter] OrderIdentifier input)
    {
        ActionInputValidator.ValidateRequired(input.OrderId, "Order ID");
        var request = new RestRequest(
            $"/integration/orders/{Uri.EscapeDataString(input.OrderId)}/human-review",
            Method.Post);
        await Client.ExecuteWithErrorHandling(request);
        return new SuccessResponse { Success = true };
    }

    [Action("Re-run order", Description = "Reprocess an eligible completed or stalled AI order.")]
    public async Task<RerunOrderResponse> RerunOrder([ActionParameter] RerunOrderRequest input)
    {
        ActionInputValidator.ValidateRequired(input.OrderId, "Order ID");
        var request = new RestRequest(
                $"/integration/orders/{Uri.EscapeDataString(input.OrderId)}/rerun",
                Method.Post)
            .AddJsonBody(new { triggerFullChain = input.RerunFullChain ?? false });
        return await Client.ExecuteWithErrorHandling<RerunOrderResponse>(request);
    }

    private static void AddOptionalQueryParameter(RestRequest request, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            request.AddQueryParameter(name, value);
    }

    private static void NormalizeOrderCollections(OrderResponse order)
    {
        order.OrderDocs ??= [];
        order.DeliveryEvents ??= [];
        if (order.LatestEvaluation is null)
            return;

        order.LatestEvaluation.Scores ??= [];
        order.LatestEvaluation.SegmentEvals ??= [];
        foreach (var segment in order.LatestEvaluation.SegmentEvals)
            segment.Scores ??= [];
    }
}
