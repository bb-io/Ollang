using Apps.Ollang.Models.Requests;
using Apps.Ollang.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Ollang.Actions;

[ActionList("Files")]
public class FileActions(
    InvocationContext invocationContext,
    IFileManagementClient fileManagementClient) : Invocable(invocationContext)
{
    [Action("Upload source file", Description = "Upload a source file and create an Ollang project.")]
    public async Task<ProjectIdResponse> UploadSourceFile([ActionParameter] UploadSourceFileRequest input)
    {
        ValidateRequired(input.Name, "Name");
        ValidateRequired(input.SourceLanguage, "Source language");

        var fileStream = await fileManagementClient.DownloadAsync(input.File);
        var fileBytes = await fileStream.GetByteData();

        var request = new RestRequest("/integration/upload/direct", Method.Post)
        {
            AlwaysMultipartFormData = true
        };
        request.AddFile("file", fileBytes, input.File.Name, input.File.ContentType ?? "application/octet-stream");
        request.AddParameter("name", input.Name);
        request.AddParameter("sourceLanguage", input.SourceLanguage);

        if (!string.IsNullOrWhiteSpace(input.FolderId))
            request.AddParameter("folderId", input.FolderId);

        if (!string.IsNullOrWhiteSpace(input.NoteTimestamp) && string.IsNullOrWhiteSpace(input.Note))
            throw new PluginMisconfigurationException("Note is required when Note timestamp is provided.");

        if (!string.IsNullOrWhiteSpace(input.NoteTimestamp) &&
            !System.Text.RegularExpressions.Regex.IsMatch(input.NoteTimestamp, @"^\d{2}:[0-5]\d:[0-5]\d$"))
        {
            throw new PluginMisconfigurationException("Note timestamp must use the HH:MM:SS format.");
        }

        if (!string.IsNullOrWhiteSpace(input.Note))
        {
            request.AddParameter("notes", JsonConvert.SerializeObject(new[]
            {
                new
                {
                    details = input.Note,
                    timeStamp = input.NoteTimestamp
                }
            }));
        }

        return await Client.ExecuteWithErrorHandling<ProjectIdResponse>(request);
    }

    [Action("Upload VTT file", Description = "Upload a VTT subtitle file to an existing Ollang project.")]
    public async Task<ProjectIdResponse> UploadVttFile([ActionParameter] UploadVttFileRequest input)
    {
        ValidateRequired(input.ProjectId, "Project ID");
        ValidateRequired(input.Name, "Name");

        if (!string.Equals(Path.GetExtension(input.File.Name), ".vtt", StringComparison.OrdinalIgnoreCase))
            throw new PluginMisconfigurationException("The uploaded subtitle file must have a .vtt extension.");

        var fileStream = await fileManagementClient.DownloadAsync(input.File);
        var fileBytes = await fileStream.GetByteData();

        var request = new RestRequest("/integration/upload/vtt", Method.Post)
        {
            AlwaysMultipartFormData = true
        };
        request.AddFile("file", fileBytes, input.File.Name, input.File.ContentType ?? "text/vtt");
        request.AddParameter("projectId", input.ProjectId);
        request.AddParameter("name", input.Name);

        if (!string.IsNullOrWhiteSpace(input.SourceLanguage))
            request.AddParameter("sourceLanguage", input.SourceLanguage);

        return await Client.ExecuteWithErrorHandling<ProjectIdResponse>(request);
    }

    [Action(
        "Upload reference document",
        Description = "Upload a glossary, guideline, or character list to an Ollang project or folder.")]
    public async Task<ReferenceDocumentResponse> UploadReferenceDocument(
        [ActionParameter] UploadReferenceDocumentRequest input)
    {
        var destination = input.Destination.Trim().ToLowerInvariant();
        var destinationId = destination switch
        {
            "project" when !string.IsNullOrWhiteSpace(input.ProjectId) => input.ProjectId,
            "folder" when !string.IsNullOrWhiteSpace(input.FolderId) => input.FolderId,
            "project" => throw new PluginMisconfigurationException(
                "Project ID is required when Destination is Project."),
            "folder" => throw new PluginMisconfigurationException(
                "Folder ID is required when Destination is Folder."),
            _ => throw new PluginMisconfigurationException("Destination must be Project or Folder.")
        };

        ValidateRequired(input.DocumentType, "Document type");

        var fileStream = await fileManagementClient.DownloadAsync(input.File);
        var fileBytes = await fileStream.GetByteData();
        var request = new RestRequest(
            $"/integration/{destination}/{Uri.EscapeDataString(destinationId)}/docs/upload",
            Method.Post)
        {
            AlwaysMultipartFormData = true
        };

        request.AddFile("file", fileBytes, input.File.Name, input.File.ContentType ?? "application/octet-stream");
        request.AddParameter("type", input.DocumentType);

        if (!string.IsNullOrWhiteSpace(input.Name))
            request.AddParameter("name", input.Name);

        if (!string.IsNullOrWhiteSpace(input.Language))
            request.AddParameter("language", input.Language);

        return await Client.ExecuteWithErrorHandling<ReferenceDocumentResponse>(request);
    }

    private static void ValidateRequired(string? value, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PluginMisconfigurationException($"{displayName} is required.");
    }
}
