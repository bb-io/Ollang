using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class ProjectIdResponse
{
    [Display("Project ID")]
    public string ProjectId { get; set; } = string.Empty;
}
