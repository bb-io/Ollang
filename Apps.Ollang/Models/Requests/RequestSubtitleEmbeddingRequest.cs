using Apps.Ollang.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Requests;

public class RequestSubtitleEmbeddingRequest : OrderIdentifier
{
    [Display("With dubbed audio", Description = "Burn subtitles into the linked dubbed video instead of the original video. Applies to subtitle and closed caption orders.")]
    public bool? WithDubbedAudio { get; set; }
}
