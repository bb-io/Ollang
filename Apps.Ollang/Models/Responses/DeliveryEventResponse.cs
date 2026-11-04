using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class DeliveryEventResponse
{
    [Display("Type")]
    public string? Type { get; set; }

    [Display("Occurred at")]
    public DateTime? OccurredAt { get; set; }

    [Display("Actor type")]
    public string? ActorType { get; set; }

    [Display("Actor ID")]
    public string? ActorId { get; set; }
}
