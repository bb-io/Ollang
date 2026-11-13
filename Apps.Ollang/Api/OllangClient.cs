using System.Globalization;
using System.Net;
using Apps.Ollang.Constants;
using Apps.Ollang.Dtos;
using Apps.Ollang.Extensions;
using Apps.Ollang.Models.Responses;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Apps.Ollang.Api;

public class OllangClient : BlackBirdRestClient
{
    public const string ApiBaseUrl = "https://api-integration.ollang.com";
    private const int RetryCount = 3;
    private const int MaxRetryAfterSeconds = 60;

    private readonly AsyncRetryPolicy<RestResponse> _rateLimitPolicy;

    public OllangClient(IEnumerable<AuthenticationCredentialsProvider> credentials) : base(new()
    {
        BaseUrl = new Uri(ApiBaseUrl),
        Timeout = TimeSpan.FromMinutes(3)
    })
    {
        this.AddDefaultHeader("X-Api-Key", credentials.Get(CredsNames.Token).Value);
        this.AddDefaultHeader("Accept", "application/json");

        _rateLimitPolicy = Policy
            .HandleResult<RestResponse>(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                RetryCount,
                (attempt, result, _) => GetRetryDelay(attempt, result.Result),
                (_, _, _, _) => Task.CompletedTask);
    }

    public override async Task<RestResponse> ExecuteWithErrorHandling(RestRequest request)
    {
        var response = await _rateLimitPolicy.ExecuteAsync(() => ExecuteAsync(request));
        if (!response.IsSuccessStatusCode)
            throw ConfigureErrorException(response);

        return response;
    }

    public async Task<RestResponse> ExecuteWithErrorHandling(
        RestRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _rateLimitPolicy.ExecuteAsync(
            token => ExecuteAsync(request, token),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw ConfigureErrorException(response);

        return response;
    }

    public async Task<T> ExecuteWithErrorHandling<T>(
        RestRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithErrorHandling(request, cancellationToken);
        try
        {
            return JsonConvert.DeserializeObject<T>(response.Content ?? string.Empty)
                   ?? throw new PluginApplicationException("Ollang returned an empty response.");
        }
        catch (JsonException exception)
        {
            throw new PluginApplicationException($"Ollang returned an invalid response: {exception.Message}");
        }
    }

    public async Task<List<T>> Paginate<T>(
        Func<int, RestRequest> requestFactory,
        int? maxItems = null,
        CancellationToken cancellationToken = default)
    {
        var items = new List<T>();
        var page = 1;

        while (!maxItems.HasValue || items.Count < maxItems.Value)
        {
            var request = requestFactory(page);
            var response = await ExecuteWithErrorHandling<PagedResponseDto<T>>(request, cancellationToken);
            items.AddRange(response.Data);

            if (!response.Meta.HasNextPage)
                break;

            page++;
        }

        return maxItems.HasValue
            ? items.Take(maxItems.Value).ToList()
            : items;
    }

    public Task<ProjectIdResponse> UploadSourceFileAsync(
        byte[] fileBytes,
        string fileName,
        string contentType,
        string name,
        string sourceLanguage,
        string? folderId = null,
        string? note = null,
        string? noteTimestamp = null)
    {
        var request = new RestRequest("/integration/upload/direct", Method.Post)
        {
            AlwaysMultipartFormData = true
        };
        request.AddFile("file", fileBytes, fileName, contentType);
        request.AddParameter("name", name);
        request.AddParameter("sourceLanguage", sourceLanguage);

        request.AddOptionalParameter("folderId", folderId);

        if (!string.IsNullOrWhiteSpace(note))
        {
            request.AddParameter("notes", JsonConvert.SerializeObject(new[]
            {
                new
                {
                    details = note,
                    timeStamp = noteTimestamp
                }
            }));
        }

        return ExecuteWithErrorHandling<ProjectIdResponse>(request);
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var message = ExtractErrorMessage(response);

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new PluginMisconfigurationException(
                $"Ollang rejected the request: {message} Check the action inputs and available account credits."),
            HttpStatusCode.Unauthorized => new PluginMisconfigurationException(
                "Ollang rejected the API token. Reconnect with a valid developer API key."),
            HttpStatusCode.Forbidden => new PluginMisconfigurationException(
                $"Ollang denied access: {message} Check the account permissions or plan."),
            HttpStatusCode.NotFound => new PluginMisconfigurationException(
                $"Ollang could not find the requested resource: {message} Check the selected ID."),
            HttpStatusCode.TooManyRequests => new PluginApplicationException(
                "Ollang is still rate limiting requests after retries. Try again later."),
            _ when (int)response.StatusCode >= 500 => new PluginApplicationException(
                $"Ollang is temporarily unavailable ({(int)response.StatusCode}). {message}"),
            _ => new PluginApplicationException(
                $"Ollang request failed ({(int)response.StatusCode}). {message}")
        };
    }

    private static TimeSpan GetRetryDelay(int attempt, RestResponse response)
    {
        var retryAfter = response.Headers?
            .FirstOrDefault(header => string.Equals(
                header.Name,
                "Retry-After",
                StringComparison.OrdinalIgnoreCase))?
            .Value?.ToString();

        if (int.TryParse(retryAfter, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) && seconds >= 0)
            return TimeSpan.FromSeconds(Math.Min(seconds, MaxRetryAfterSeconds));

        if (DateTimeOffset.TryParse(
                retryAfter,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var retryAt))
        {
            var delay = retryAt - DateTimeOffset.UtcNow;
            if (delay <= TimeSpan.Zero)
                return TimeSpan.Zero;

            return delay > TimeSpan.FromSeconds(MaxRetryAfterSeconds)
                ? TimeSpan.FromSeconds(MaxRetryAfterSeconds)
                : delay;
        }

        var backoffSeconds = Math.Min(Math.Pow(2, attempt - 1), 8);
        return TimeSpan.FromSeconds(backoffSeconds) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
    }

    private static string ExtractErrorMessage(RestResponse response)
    {
        var content = response.Content?.Trim();
        if (string.IsNullOrWhiteSpace(content))
            return response.ErrorMessage ?? response.StatusDescription ?? "No error details were returned.";

        if (response.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true ||
            content.StartsWith('{'))
        {
            try
            {
                var error = JsonConvert.DeserializeObject<ErrorDto>(content);
                if (!string.IsNullOrWhiteSpace(error?.Message))
                    return error.Message;

                if (!string.IsNullOrWhiteSpace(error?.Error))
                    return error.Error;
            }
            catch (JsonException)
            {
                // Fall through to a bounded plain-text message.
            }
        }

        var plainText = System.Text.RegularExpressions.Regex.Replace(content, "<[^>]+>", " ");
        plainText = System.Net.WebUtility.HtmlDecode(plainText);
        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\s+", " ").Trim();
        return plainText.Length > 300 ? $"{plainText[..300]}..." : plainText;
    }
}
