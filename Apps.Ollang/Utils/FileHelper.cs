using RestSharp;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace Apps.Ollang.Utils;

public static class FileHelper
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

    public static string ResolveContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (ContentTypeOverrides.TryGetValue(extension, out var contentType))
            return contentType;

        return MimeTypes.TryGetMimeType(fileName, out contentType)
            ? contentType
            : MediaTypeNames.Application.Octet;
    }

    public static string ResolveFileName(
        string? documentName,
        string url,
        string fallbackName)
    {
        if (!string.IsNullOrWhiteSpace(documentName) && !string.IsNullOrWhiteSpace(Path.GetExtension(documentName)))
            return SanitizeFileName(documentName);

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var urlFileName = Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath));
            if (!string.IsNullOrWhiteSpace(urlFileName))
                return SanitizeFileName(urlFileName);
        }

        return SanitizeFileName(documentName ?? fallbackName);
    }

    public static string ResolveDownloadFileName(
        RestResponse response,
        string fallbackName)
    {
        fallbackName = SanitizeFileName(fallbackName);
        var contentDisposition = response.Headers?
            .FirstOrDefault(header => string.Equals(
                header.Name,
                "Content-Disposition",
                StringComparison.OrdinalIgnoreCase))?
            .Value?.ToString();

        if (string.IsNullOrWhiteSpace(contentDisposition) ||
            !ContentDispositionHeaderValue.TryParse(contentDisposition, out var parsedHeader))
        {
            return fallbackName;
        }

        var fileName = parsedHeader.FileNameStar ?? parsedHeader.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
            return fallbackName;

        fileName = fileName.Trim().Trim('"');
        if (fileName.StartsWith("UTF-8''", StringComparison.OrdinalIgnoreCase))
            fileName = fileName[7..];

        try
        {
            fileName = Uri.UnescapeDataString(fileName);
        }
        catch (UriFormatException)
        {
            return fallbackName;
        }

        fileName = SanitizeFileName(Path.GetFileName(fileName.Replace('\\', '/')));
        return string.IsNullOrWhiteSpace(fileName) ? fallbackName : fileName;
    }

    public static string SanitizeFileName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(character =>
            invalidCharacters.Contains(character) || character is '/' or '\\' ? '_' : character));
    }
}
