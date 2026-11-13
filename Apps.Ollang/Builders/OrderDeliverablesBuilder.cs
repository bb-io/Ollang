using Apps.Ollang.Models.Requests;
using Apps.Ollang.Models.Responses;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using System.Net.Mime;

namespace Apps.Ollang.Builders;

public static class OrderDeliverablesBuilder
{
    private static readonly IReadOnlyDictionary<string, string> ContentTypeOverrides =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".itt"] = "application/ttml+xml",
            [".dfxp"] = "application/ttml+xml",
            [".ass"] = "text/x-ssa",
            [".scc"] = "text/x-scc",
            [".xliff"] = "application/xliff+xml",
            [".sdlxliff"] = "application/xliff+xml",
            [".mp3"] = "audio/mpeg"
        };

    public static List<DeliverableFileResponse> Build(
        OrderResponse order,
        DownloadOrderDeliverablesRequest input)
    {
        order.OrderDocs ??= [];

        var documents = GetFilteredDocuments(order, input);
        var deliverables = documents
            .Select(document => CreateDocumentDeliverable(document, order.TargetLanguage))
            .ToList();

        AddOptionalDeliverables(deliverables, documents, order, input);
        return deliverables;
    }

    private static List<OrderDocumentResponse> GetFilteredDocuments(
        OrderResponse order,
        DownloadOrderDeliverablesRequest input)
    {
        var outputDocuments = order.DeliveryFiles?.Outputs is { Count: > 0 }
            ? order.DeliveryFiles.Outputs
            : order.OrderDocs;
        var documentTypes = (input.DocumentTypes ?? [])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return outputDocuments
            .Where(document => MatchesDocumentFilters(
                document,
                input,
                documentTypes,
                order.TargetLanguage))
            .ToList();
    }

    private static DeliverableFileResponse CreateDocumentDeliverable(
        OrderDocumentResponse document,
        string? targetLanguage)
    {
        var file = CreateSignedFileReference(
            document.Url!,
            ResolveFileName(document.Name, document.Url!, document.Id));

        return new DeliverableFileResponse
        {
            File = file,
            DocumentId = document.Id,
            Name = document.Name,
            DocumentType = document.Type,
            Language = document.SourceLanguage ?? targetLanguage,
            Size = document.Size
        };
    }

    private static void AddOptionalDeliverables(
        List<DeliverableFileResponse> deliverables,
        IReadOnlyCollection<OrderDocumentResponse> documents,
        OrderResponse order,
        DownloadOrderDeliverablesRequest input)
    {
        if (input.IncludeVtt == true && !string.IsNullOrWhiteSpace(order.VttUrl))
        {
            var baseName = SanitizeFileName(order.Name ?? order.OrderId);
            var vttName = string.Equals(Path.GetExtension(baseName), ".vtt", StringComparison.OrdinalIgnoreCase)
                ? baseName
                : $"{baseName}.vtt";
            var file = CreateSignedFileReference(order.VttUrl, vttName);
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
            var file = CreateSignedFileReference(embeddedVideoUrl, embeddedVideoName);
            deliverables.Add(new DeliverableFileResponse
            {
                File = file,
                Name = embeddedVideoName,
                DocumentType = "created_subtitle_embedded_video",
                Language = order.TargetLanguage
            });
        }
    }

    private static bool IsSubtitleEmbeddedVideo(OrderDocumentResponse document) =>
        string.Equals(
            document.Type,
            "created_subtitle_embedded_video",
            StringComparison.OrdinalIgnoreCase);

    private static bool MatchesDocumentFilters(
        OrderDocumentResponse document,
        DownloadOrderDeliverablesRequest input,
        HashSet<string> documentTypes,
        string? targetLanguage) =>
        !string.IsNullOrWhiteSpace(document.Url) &&
        (!IsSubtitleEmbeddedVideo(document) || input.IncludeSubtitleEmbeddedVideo == true) &&
        (documentTypes.Count == 0 || documentTypes.Contains(document.Type ?? string.Empty)) &&
        (string.IsNullOrWhiteSpace(input.NameContains) ||
         (document.Name?.Contains(input.NameContains, StringComparison.OrdinalIgnoreCase) ?? false)) &&
        (string.IsNullOrWhiteSpace(input.Language) ||
         string.Equals(
             document.SourceLanguage ?? targetLanguage,
             input.Language,
             StringComparison.OrdinalIgnoreCase));

    private static FileReference CreateSignedFileReference(
        string url,
        string fileName)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new PluginApplicationException("Ollang returned an invalid deliverable download URL.");

        return new FileReference(
            new HttpRequestMessage(HttpMethod.Get, uri),
            fileName,
            ResolveContentType(fileName));
    }

    private static string ResolveContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (ContentTypeOverrides.TryGetValue(extension, out var contentType))
            return contentType;

        return MimeTypes.TryGetMimeType(fileName, out contentType)
            ? contentType
            : MediaTypeNames.Application.Octet;
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
