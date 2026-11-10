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

        if (!string.IsNullOrWhiteSpace(input.SourceLanguage))
            request.AddParameter("sourceLanguage", input.SourceLanguage);

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

        if (!string.IsNullOrWhiteSpace(input.Name))
            request.AddParameter("name", input.Name);

        if (!string.IsNullOrWhiteSpace(input.Language))
            request.AddParameter("language", input.Language);

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
        order.OrderDocs ??= [];

        var outputDocuments = order.DeliveryFiles?.Outputs is { Count: > 0 }
            ? order.DeliveryFiles.Outputs
            : order.OrderDocs;

        var documentTypes = (input.DocumentTypes ?? [])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var documents = outputDocuments.Where(document =>
            !string.IsNullOrWhiteSpace(document.Url) &&
            (!IsSubtitleEmbeddedVideo(document) || input.IncludeSubtitleEmbeddedVideo == true) &&
            (documentTypes.Count == 0 || documentTypes.Contains(document.Type ?? string.Empty)) &&
            (string.IsNullOrWhiteSpace(input.NameContains) ||
             (document.Name?.Contains(input.NameContains, StringComparison.OrdinalIgnoreCase) ?? false)) &&
            (string.IsNullOrWhiteSpace(input.Language) ||
             string.Equals(
                 document.SourceLanguage ?? order.TargetLanguage,
                 input.Language,
                 StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var deliverables = new List<DeliverableFileResponse>();
        using var httpClient = new HttpClient();
        foreach (var document in documents)
        {
            var file = await DownloadSignedFile(
                httpClient,
                document.Url!,
                ResolveFileName(document.Name, document.Url!, document.Id));
            deliverables.Add(new DeliverableFileResponse
            {
                File = file,
                DocumentId = document.Id,
                Name = document.Name,
                DocumentType = document.Type,
                Language = document.SourceLanguage ?? order.TargetLanguage,
                Size = document.Size
            });
        }

        if (input.IncludeVtt == true && !string.IsNullOrWhiteSpace(order.VttUrl))
        {
            var baseName = SanitizeFileName(order.Name ?? order.OrderId);
            var vttName = string.Equals(Path.GetExtension(baseName), ".vtt", StringComparison.OrdinalIgnoreCase)
                ? baseName
                : $"{baseName}.vtt";
            var file = await DownloadSignedFile(httpClient, order.VttUrl, vttName);
            deliverables.Add(new DeliverableFileResponse
            {
                File = file,
                Name = vttName,
                DocumentType = "vtt",
                Language = order.TargetLanguage
            });
        }

        var embeddedVideoUrl = order.DeliveryFiles?.VideoWithBurnedInSubtitlesUrl;
        var embeddedVideoAlreadyIncluded = documents.Any(IsSubtitleEmbeddedVideo);
        if (input.IncludeSubtitleEmbeddedVideo == true &&
            !embeddedVideoAlreadyIncluded &&
            !string.IsNullOrWhiteSpace(embeddedVideoUrl))
        {
            var fallbackName = $"{SanitizeFileName(order.OrderId)}-subtitled.mp4";
            var embeddedVideoName = ResolveFileName(null, embeddedVideoUrl, fallbackName);
            var file = await DownloadSignedFile(httpClient, embeddedVideoUrl, embeddedVideoName);
            deliverables.Add(new DeliverableFileResponse
            {
                File = file,
                Name = embeddedVideoName,
                DocumentType = "created_subtitle_embedded_video",
                Language = order.TargetLanguage
            });
        }

        return new DownloadOrderDeliverablesResponse { Deliverables = deliverables };
    }

    private static bool IsSubtitleEmbeddedVideo(OrderDocumentResponse document) =>
        string.Equals(
            document.Type,
            "created_subtitle_embedded_video",
            StringComparison.OrdinalIgnoreCase);

    private async Task<Blackbird.Applications.Sdk.Common.Files.FileReference> DownloadSignedFile(
        HttpClient httpClient,
        string url,
        string fileName)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new PluginApplicationException("Ollang returned an invalid deliverable download URL.");

        using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            throw new PluginApplicationException(
                $"Ollang deliverable download failed with status {(int)response.StatusCode}.");
        }

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        var temporaryFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        await using var temporaryFile = new FileStream(
            temporaryFilePath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.DeleteOnClose);

        await response.Content.CopyToAsync(temporaryFile);
        temporaryFile.Position = 0;

        return await fileManagementClient.UploadAsync(temporaryFile, contentType, fileName);
    }

    private static string ResolveFileName(string? documentName, string url, string documentId)
    {
        if (!string.IsNullOrWhiteSpace(documentName) && !string.IsNullOrWhiteSpace(Path.GetExtension(documentName)))
            return SanitizeFileName(documentName);

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var urlFileName = Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath));
            if (!string.IsNullOrWhiteSpace(urlFileName))
                return SanitizeFileName(urlFileName);
        }

        return SanitizeFileName(documentName ?? documentId);
    }

    private static string SanitizeFileName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(character => invalidCharacters.Contains(character) ? '_' : character));
    }

}
