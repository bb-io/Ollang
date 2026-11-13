using Apps.Ollang.Builders;
using Apps.Ollang.Extensions;
using Apps.Ollang.Models.Requests;
using Apps.Ollang.Models.Responses;
using Apps.Ollang.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
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
        ActionInputValidator.ValidateRequired(input.Name, "Name");
        ActionInputValidator.ValidateRequired(input.SourceLanguage, "Source language");

        var fileStream = await fileManagementClient.DownloadAsync(input.File);
        var fileBytes = await fileStream.GetByteData();

        if (!string.IsNullOrWhiteSpace(input.NoteTimestamp) && string.IsNullOrWhiteSpace(input.Note))
            throw new PluginMisconfigurationException("Note is required when Note timestamp is provided.");

        ActionInputValidator.ValidateTimestamp(input.NoteTimestamp, "Note timestamp");

        return await Client.UploadSourceFileAsync(
            fileBytes,
            input.File.Name,
            input.File.ContentType ?? "application/octet-stream",
            input.Name,
            input.SourceLanguage,
            input.FolderId,
            input.Note,
            input.NoteTimestamp);
    }

    [Action("Upload VTT file", Description = "Upload a VTT subtitle file to an existing Ollang project.")]
    public async Task<ProjectIdResponse> UploadVttFile([ActionParameter] UploadVttFileRequest input)
    {
        ActionInputValidator.ValidateRequired(input.ProjectId, "Project ID");
        ActionInputValidator.ValidateRequired(input.Name, "Name");

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

        request.AddOptionalParameter("sourceLanguage", input.SourceLanguage);

        return await Client.ExecuteWithErrorHandling<ProjectIdResponse>(request);
    }

    [Action("Upload reference document", Description = "Upload a glossary, guideline, or character list to an Ollang project or folder.")]
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

        ActionInputValidator.ValidateRequired(input.DocumentType, "Document type");

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

        request.AddOptionalParameter("name", input.Name);
        request.AddOptionalParameter("language", input.Language);

        return await Client.ExecuteWithErrorHandling<ReferenceDocumentResponse>(request);
    }

    [Action("Download order deliverables", Description = "Download order files using optional type, name, and language filters.")]
    public async Task<DownloadOrderDeliverablesResponse> DownloadOrderDeliverables(
        [ActionParameter] DownloadOrderDeliverablesRequest input)
    {
        ActionInputValidator.ValidateRequired(input.OrderId, "Order ID");

        var orderRequest = new RestRequest(
            $"/integration/orders/{Uri.EscapeDataString(input.OrderId)}",
            Method.Get);
        var order = await Client.ExecuteWithErrorHandling<OrderResponse>(orderRequest);
        var deliverables = OrderDeliverablesBuilder.Build(order, input);
        return new DownloadOrderDeliverablesResponse { Deliverables = deliverables };
    }
}
