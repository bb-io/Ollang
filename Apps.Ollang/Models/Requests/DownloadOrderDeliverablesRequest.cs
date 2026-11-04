using Apps.Ollang.Handlers;
using Apps.Ollang.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Models.Requests;

public class DownloadOrderDeliverablesRequest
{
    [Display("Order ID")]
    [DataSource(typeof(OrderDataSourceHandler))]
    public string OrderId { get; set; } = string.Empty;

    [Display("Document types", Description = "Optional deliverable types to include.")]
    [StaticDataSource(typeof(OrderDocumentTypeDataSourceHandler))]
    public IEnumerable<string>? DocumentTypes { get; set; }

    [Display("Name contains", Description = "Only include documents whose name contains this text.")]
    public string? NameContains { get; set; }

    [Display("Language")]
    [StaticDataSource(typeof(LanguageDataSourceHandler))]
    public string? Language { get; set; }

    [Display("Include VTT", Description = "Also download the order VTT file when available.")]
    public bool? IncludeVtt { get; set; }
}
