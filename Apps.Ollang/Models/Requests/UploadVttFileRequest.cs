using Apps.Ollang.Handlers;
using Apps.Ollang.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Ollang.Models.Requests;

public class UploadVttFileRequest
{
    [Display("File", Description = "VTT subtitle file. The .vtt extension is required.")]
    public FileReference File { get; set; } = new();

    [Display("Project ID")]
    [DataSource(typeof(ProjectDataSourceHandler))]
    public string ProjectId { get; set; } = string.Empty;

    [Display("Name")]
    public string Name { get; set; } = string.Empty;

    [Display("Source language", Description = "Optional BCP-47 language code. The value 'all' is also accepted.")]
    [StaticDataSource(typeof(VttLanguageDataSourceHandler))]
    public string? SourceLanguage { get; set; }
}
