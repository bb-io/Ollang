using Apps.Ollang.Models.Requests;
using Apps.Ollang.Models.Responses;
using Apps.Ollang.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Ollang.Actions;

[ActionList("Quality control")]
public class QualityControlActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Run QC evaluation", Description = "Start an asynchronous AI quality evaluation for an eligible completed or delivered order.")]
    public async Task<QcEvaluationResponse> RunQcEvaluation([ActionParameter] RunQcRequest input)
    {
        ActionInputValidator.ValidateRequired(input.OrderId, "Order ID");

        var body = new Dictionary<string, object>
        {
            ["accuracy"] = input.Accuracy ?? true,
            ["fluency"] = input.Fluency ?? true,
            ["tone"] = input.Tone ?? true,
            ["culturalFit"] = input.CulturalFit ?? true
        };
        if (!string.IsNullOrWhiteSpace(input.CustomPrompt))
            body["customPrompt"] = input.CustomPrompt;
        if (!string.IsNullOrWhiteSpace(input.CallbackUrl))
            body["callbackUrl"] = input.CallbackUrl;

        var request = new RestRequest(
                $"/integration/orders/{Uri.EscapeDataString(input.OrderId)}/qc",
                Method.Post)
            .AddJsonBody(body);

        return await Client.ExecuteWithErrorHandling<QcEvaluationResponse>(request);
    }
}
