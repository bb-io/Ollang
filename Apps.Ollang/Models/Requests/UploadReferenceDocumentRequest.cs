using Apps.Ollang.Handlers;
using Apps.Ollang.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Ollang.Models.Requests;

public class UploadReferenceDocumentRequest
{
    [Display("Destination")]
    [StaticDataSource(typeof(ReferenceDestinationDataSourceHandler))]
    public string Destination { get; set; } = string.Empty;

    [Display("Project ID", Description = "Required when Destination is Project.")]
    [DataSource(typeof(ProjectDataSourceHandler))]
    public string? ProjectId { get; set; }

    [Display("Folder ID", Description = "Required when Destination is Folder.")]
    [DataSource(typeof(FolderDataSourceHandler))]
    public string? FolderId { get; set; }

    [Display("File", Description = "A glossary, guideline, or character list smaller than 50 MiB.")]
    public FileReference File { get; set; } = new();

    [Display("Document type")]
    [StaticDataSource(typeof(ReferenceDocumentTypeDataSourceHandler))]
    public string DocumentType { get; set; } = string.Empty;

    [Display("Name", Description = "Optional display name. Ollang uses the filename when omitted.")]
    public string? Name { get; set; }

    [Display("Language", Description = "Optional BCP-47 language code, for example en or uk.")]
    [StaticDataSource(typeof(LanguageDataSourceHandler))]
    public string? Language { get; set; }
}
