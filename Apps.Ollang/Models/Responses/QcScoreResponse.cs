using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Ollang.Models.Responses;

public class QcScoreResponse
{
    [Display("Name")]
    public string? Name { get; set; }

    [Display("Description")]
    public string? Description { get; set; }

    [Display("Score")]
    public double? Score { get; set; }

    [JsonProperty("value")]
    private double? Value
    {
        set => Score = value;
    }

    [JsonProperty("details")]
    private string? Details
    {
        set => Description = value;
    }
}
