using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Ollang.Models.Responses;

public class RevisionResponse
{
    [Display("Revision ID")]
    [JsonProperty("id")]
    public string RevisionId { get; set; } = string.Empty;

    [Display("Created at")]
    public DateTime? CreatedAt { get; set; }

    [Display("Revision type")]
    [JsonProperty("type")]
    public string? RevisionType { get; set; }

    [Display("Timestamp")]
    [JsonProperty("time")]
    public string? Timestamp { get; set; }

    [Display("Description")]
    public string? Description { get; set; }
}
