using Apps.Ollang.Handlers;
using Apps.Ollang.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Ollang.Models.Requests;

public class CreateOrderRequest
{
    [Display("File", Description = "Optional source file. Provide either File or Project ID, but not both.")]
    public FileReference? File { get; set; }

    [Display("Project ID", Description = "Existing project. Required when File is not provided.")]
    [DataSource(typeof(ProjectDataSourceHandler))]
    public string? ProjectId { get; set; }

    [Display("File name", Description = "Project name used when File is provided.")]
    public string? FileName { get; set; }

    [Display("Source language", Description = "Required when File is provided.")]
    [StaticDataSource(typeof(LanguageDataSourceHandler))]
    public string? SourceLanguage { get; set; }

    [Display("Folder ID", Description = "Optional destination folder when File is provided.")]
    [DataSource(typeof(FolderDataSourceHandler))]
    public string? FolderId { get; set; }

    [Display("Order type")]
    [StaticDataSource(typeof(OrderTypeDataSourceHandler))]
    public string OrderType { get; set; } = string.Empty;

    [Display("Level")]
    [StaticDataSource(typeof(ServiceLevelDataSourceHandler))]
    public string Level { get; set; } = string.Empty;

    [Display("Standard target languages", Description = "Target languages that use standard delivery.")]
    [StaticDataSource(typeof(LanguageDataSourceHandler))]
    public IEnumerable<string> TargetLanguages { get; set; } = [];

    [Display("Rush target languages", Description = "Target languages that require rush delivery. Do not also select them as standard target languages.")]
    [StaticDataSource(typeof(LanguageDataSourceHandler))]
    public IEnumerable<string>? RushTargetLanguages { get; set; }

    [Display("Order subtype", Description = "Optional closed-caption or transcription subtype.")]
    [StaticDataSource(typeof(OrderSubtypeDataSourceHandler))]
    public string? OrderSubtype { get; set; }

    [Display("Dubbing style", Description = "Optional style for dubbing orders.")]
    [StaticDataSource(typeof(DubbingStyleDataSourceHandler))]
    public string? DubbingStyle { get; set; }

    [Display("Callback URL", Description = "Optional HTTP or HTTPS URL called when the order completes.")]
    public string? CallbackUrl { get; set; }

    [Display("Run QC automatically")]
    public bool? RunQcAutomatically { get; set; }

    [Display("Memory IDs", Description = "Optional translation memories. Maximum 20 unique values.")]
    [DataSource(typeof(MemoryDataSourceHandler))]
    public IEnumerable<string>? MemoryIds { get; set; }
}
