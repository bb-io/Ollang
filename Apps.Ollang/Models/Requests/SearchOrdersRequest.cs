using Apps.Ollang.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Ollang.Models.Requests;

public class SearchOrdersRequest
{
    [Display("Search", Description = "Free-text search across orders.")]
    public string? Search { get; set; }

    [Display("Order type")]
    [StaticDataSource(typeof(OrderTypeDataSourceHandler))]
    public string? OrderType { get; set; }

    [Display("Name")]
    public string? Name { get; set; }

    [Display("Created from")]
    public DateTime? CreatedFrom { get; set; }

    [Display("Created to")]
    public DateTime? CreatedTo { get; set; }

    [Display("Sort by")]
    [StaticDataSource(typeof(OrderSortFieldDataSourceHandler))]
    public string? SortBy { get; set; }

    [Display("Sort direction")]
    [StaticDataSource(typeof(SortDirectionDataSourceHandler))]
    public string? SortDirection { get; set; }
}
