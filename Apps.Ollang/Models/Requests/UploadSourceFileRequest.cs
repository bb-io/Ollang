using Apps.Ollang.Handlers;
using Apps.Ollang.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Ollang.Models.Requests;

public class UploadSourceFileRequest
{
    [Display("File")]
    public FileReference File { get; set; } = new();

    [Display("Name", Description = "Name of the project created from the source file.")]
    public string Name { get; set; } = string.Empty;

    [Display("Source language", Description = "BCP-47 language code, for example en or uk.")]
    [StaticDataSource(typeof(LanguageDataSourceHandler))]
    public string SourceLanguage { get; set; } = string.Empty;

    [Display("Folder ID")]
    [DataSource(typeof(FolderDataSourceHandler))]
    public string? FolderId { get; set; }

    [Display("Note", Description = "Optional instruction for the source file.")]
    public string? Note { get; set; }

    [Display("Note timestamp", Description = "Optional media timestamp for the note in HH:MM:SS format.")]
    public string? NoteTimestamp { get; set; }
}
