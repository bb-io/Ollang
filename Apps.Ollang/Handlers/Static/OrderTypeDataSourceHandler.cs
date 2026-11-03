using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Handlers.Static;

public class OrderTypeDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("cc", "Closed captions"),
        new("subtitle", "Subtitle translation"),
        new("document", "Document translation"),
        new("aiDubbing", "AI dubbing"),
        new("studioDubbing", "Studio dubbing")
    ];
}
