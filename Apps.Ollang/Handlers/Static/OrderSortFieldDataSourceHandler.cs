using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Handlers.Static;

public class OrderSortFieldDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("id", "Order ID"),
        new("createdAt", "Created at"),
        new("name", "Name"),
        new("status", "Status")
    ];
}
