using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class DownloadOrderDeliverablesResponse
{
    [Display("Deliverables")]
    public List<DeliverableFileResponse> Deliverables { get; set; } = [];
}
