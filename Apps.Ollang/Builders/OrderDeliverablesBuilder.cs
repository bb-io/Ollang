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
        var documentTypes = (input.DocumentTypes ?? [])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return (order.DeliveryFiles?.Outputs ?? [])
            .Concat(order.OrderDocs)
            .Where(document => MatchesDocumentFilters(
                document,
                input,
                documentTypes,
                order.TargetLanguage))
            .DistinctBy(GetDocumentIdentity, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetDocumentIdentity(OrderDocumentResponse document) =>
        !string.IsNullOrWhiteSpace(document.Id)
            ? $"id:{document.Id}"
            : $"url:{document.Url}";

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
        MatchesDocumentSelection(document, input.NameContains, documentTypes) &&
        (string.IsNullOrWhiteSpace(input.Language) ||
         string.Equals(
             document.SourceLanguage ?? targetLanguage,
             input.Language,
             StringComparison.OrdinalIgnoreCase));

    private static bool MatchesDocumentSelection(
        OrderDocumentResponse document,
        string? nameContains,
        HashSet<string> documentTypes)
    {
        var hasTypeFilter = documentTypes.Count > 0;
        var hasNameFilter = !string.IsNullOrWhiteSpace(nameContains);
        if (!hasTypeFilter && !hasNameFilter)
            return true;

        var matchesType = hasTypeFilter && documentTypes.Contains(document.Type ?? string.Empty);
        var matchesName = hasNameFilter &&
            (document.Name?.Contains(nameContains!, StringComparison.OrdinalIgnoreCase) ?? false);
        return matchesType || matchesName;
    }

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
