using Apps.Ollang.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Handlers.Static;

public class VttLanguageDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
        new[] { new DataSourceItem("all", "All languages") }
            .Concat(LanguageConstants.Languages.Select(language =>
                new DataSourceItem(language.Code, $"{language.Name} ({language.Code})")));
}
