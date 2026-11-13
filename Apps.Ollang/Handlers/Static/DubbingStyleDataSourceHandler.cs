using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Handlers.Static;

public class DubbingStyleDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("overdub", "Overdub"),
        new("lipsync", "Lip sync"),
        new("audioDescription", "Audio description")
    ];
}
