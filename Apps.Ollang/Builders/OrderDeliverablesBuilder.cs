using Apps.Ollang.Models.Requests;
using Apps.Ollang.Models.Responses;
using Apps.Ollang.Utils;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Ollang.Builders;

public static class OrderDeliverablesBuilder
{
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
            FileHelper.ResolveFileName(document.Name, document.Url!, document.Id));

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
            var baseName = FileHelper.SanitizeFileName(order.Name ?? order.OrderId);
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
            var fallbackName = $"{FileHelper.SanitizeFileName(order.OrderId)}-subtitled.mp4";
            var embeddedVideoName = FileHelper.ResolveFileName(null, embeddedVideoUrl, fallbackName);
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
            FileHelper.ResolveContentType(fileName));
    }
}
