using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Handlers.Static;

public class RevisionTypeDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("missingSubtitle", "Missing subtitle"),
        new("wrongSubtitle", "Wrong subtitle"),
        new("syncError", "Synchronization error"),
        new("formatError", "Formatting error"),
        new("other", "Other")
    ];
}
