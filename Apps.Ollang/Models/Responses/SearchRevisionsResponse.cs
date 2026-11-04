using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class SearchRevisionsResponse
{
    [Display("Revisions")]
    public List<RevisionResponse> Revisions { get; set; } = [];
}
