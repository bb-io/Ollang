using Apps.Ollang.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Handlers.Static;

public class LanguageDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() => LanguageConstants.Languages
        .Select(language => new DataSourceItem(language.Code, $"{language.Name} ({language.Code})"));
}
