using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Handlers.Static;

public class ReferenceDocumentTypeDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("guideline", "Guideline"),
        new("guideline_glossary", "Glossary"),
        new("character_list", "Character list")
    ];
}
