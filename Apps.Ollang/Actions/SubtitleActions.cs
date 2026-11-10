using Apps.Ollang.Models.Requests;
using Apps.Ollang.Models.Responses;
using Apps.Ollang.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Ollang.Actions;

[ActionList("Subtitles")]
public class SubtitleActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Request subtitle embedding", Description = "Start burning subtitles into the video for an eligible completed or delivered order.")]
    public async Task<SubtitleEmbeddingResponse> RequestSubtitleEmbedding(
        [ActionParameter] RequestSubtitleEmbeddingRequest input)
    {
        ActionInputValidator.ValidateRequired(input.OrderId, "Order ID");

        var request = new RestRequest(
            $"/integration/orders/{Uri.EscapeDataString(input.OrderId)}/subtitle-embedding",
            Method.Post);

        if (input.WithDubbedAudio.HasValue)
        {
            request.AddQueryParameter(
                "withDubbedAudio",
                input.WithDubbedAudio.Value ? "true" : "false");
        }

        return await Client.ExecuteWithErrorHandling<SubtitleEmbeddingResponse>(request);
    }
}
