using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Ollang.Models.Responses;

public class SegmentEvaluationResponse
{
    [Display("Segment ID")]
    public string? SegmentId { get; set; }

    [Display("Explanation")]
    public string? Explanation { get; set; }

    [Display("Suggested new value")]
    public string? SuggestedNewValue { get; set; }

    [Display("Scores")]
    public List<QcScoreResponse> Scores { get; set; } = [];

    [Display("Comments")]
    public string? Comments { get; set; }

    [JsonProperty("id")]
    private string? Id
    {
        set => SegmentId = value;
    }

    [JsonProperty("explain")]
    private string? Explain
    {
        set => Explanation = value;
    }
}
