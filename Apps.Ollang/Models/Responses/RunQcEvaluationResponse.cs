using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Ollang.Models.Responses;

public class RunQcEvaluationResponse
{
    [Display("Success")]
    public bool? Success { get; set; }

    [Display("Message")]
    public string? Message { get; set; }

    [Display("Evaluation ID")]
    [JsonProperty("evalId")]
    public string EvaluationId { get; set; } = string.Empty;

    [Display("Credits used")]
    public decimal? CreditsUsed { get; set; }

    [Display("Is processing")]
    public bool? IsProcessing { get; set; }
}
