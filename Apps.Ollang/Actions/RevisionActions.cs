using Apps.Ollang.Models.Identifiers;
using Apps.Ollang.Models.Requests;
using Apps.Ollang.Models.Responses;
using Apps.Ollang.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Ollang.Actions;

[ActionList("Revisions")]
public class RevisionActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Search order revisions", Description = "Get all revision requests for an order.")]
    public async Task<SearchRevisionsResponse> SearchOrderRevisions([ActionParameter] OrderIdentifier input)
    {
        ActionInputValidator.ValidateRequired(input.OrderId, "Order ID");
        var request = new RestRequest(
            $"/integration/revision/{Uri.EscapeDataString(input.OrderId)}",
            Method.Get);
        var revisions = await Client.ExecuteWithErrorHandling<List<RevisionResponse>>(request);
        return new SearchRevisionsResponse { Revisions = revisions ?? [] };
    }

    [Action("Create revision", Description = "Request a correction for an eligible completed or delivered order.")]
    public async Task<RevisionResponse> CreateRevision([ActionParameter] CreateRevisionRequest input)
    {
        ActionInputValidator.ValidateRequired(input.OrderId, "Order ID");
        ActionInputValidator.ValidateRequired(input.RevisionType, "Revision type");
        ActionInputValidator.ValidateRequired(input.Timestamp, "Timestamp");
        ActionInputValidator.ValidateTimestamp(input.Timestamp, "Timestamp");

        var body = new Dictionary<string, object>
        {
            ["type"] = input.RevisionType,
            ["time"] = input.Timestamp
        };
        if (!string.IsNullOrWhiteSpace(input.Description))
            body["description"] = input.Description;

        var request = new RestRequest(
                $"/integration/revision/{Uri.EscapeDataString(input.OrderId)}",
                Method.Post)
            .AddJsonBody(body);

        return await Client.ExecuteWithErrorHandling<RevisionResponse>(request);
    }
}
