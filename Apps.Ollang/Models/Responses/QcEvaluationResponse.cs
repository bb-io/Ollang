using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Ollang.Models.Responses;

public class QcEvaluationResponse
{
    [Display("Success")]
    public bool? Success { get; set; }

    [Display("Message")]
    public string? Message { get; set; }

    [Display("Evaluation ID")]
    [JsonProperty("id")]
    public string EvaluationId { get; set; } = string.Empty;

    [Display("Order ID")]
    public string? OrderId { get; set; }

    [Display("Created at")]
    public DateTime? CreatedAt { get; set; }

    [Display("Credits used")]
    public decimal? CreditsUsed { get; set; }

    [Display("Is processing")]
    public bool? IsProcessing { get; set; }

    [Display("Summary")]
    public string? TextSummary { get; set; }

    [Display("Scores")]
    public List<QcScoreResponse> Scores { get; set; } = [];

    [Display("Segment evaluations")]
    public List<SegmentEvaluationResponse> SegmentEvals { get; set; } = [];

    [Display("Is loading")]
    public bool? IsLoading { get; set; }
}
