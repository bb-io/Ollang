using Apps.Ollang.Handlers;
using Apps.Ollang.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Models.Requests;

public class CreateRevisionRequest
{
    [Display("Order ID")]
    [DataSource(typeof(OrderDataSourceHandler))]
    public string OrderId { get; set; } = string.Empty;

    [Display("Revision type")]
    [StaticDataSource(typeof(RevisionTypeDataSourceHandler))]
    public string RevisionType { get; set; } = string.Empty;

    [Display("Timestamp", Description = "Position of the issue in HH:MM:SS format.")]
    public string Timestamp { get; set; } = string.Empty;

    [Display("Description", Description = "Optional details about the correction needed.")]
    public string? Description { get; set; }
}
