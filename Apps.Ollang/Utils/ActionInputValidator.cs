using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Ollang.Utils;

public static class ActionInputValidator
{
    public static void ValidateRequired(string? value, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PluginMisconfigurationException($"{displayName} is required.");
    }

    public static void ValidateTimestamp(string? value, string displayName)
    {
        if (!string.IsNullOrWhiteSpace(value) && !Regex.IsMatch(value, @"^\d{2}:[0-5]\d:[0-5]\d$"))
            throw new PluginMisconfigurationException($"{displayName} must use the HH:MM:SS format.");
    }

    public static void ValidateUrl(
        string? value,
        string displayName,
        bool allowHttp = false,
        bool requirePublicHost = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && (!allowHttp || uri.Scheme != Uri.UriSchemeHttp)))
        {
            var schemes = allowHttp ? "HTTP or HTTPS" : "HTTPS";
            throw new PluginMisconfigurationException($"{displayName} must be a valid {schemes} URL.");
        }

        if (requirePublicHost && IsLocalOrPrivateHost(uri))
            throw new PluginMisconfigurationException($"{displayName} must use a publicly accessible host.");
    }

    private static bool IsLocalOrPrivateHost(Uri uri)
    {
        if (uri.IsLoopback || string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            return true;

        return IPAddress.TryParse(uri.Host, out var address) && IsPrivateAddress(address);
    }

    private static bool IsPrivateAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return bytes[0] == 0 ||
                   bytes[0] == 10 ||
                   bytes[0] == 127 ||
                   bytes[0] == 169 && bytes[1] == 254 ||
                   bytes[0] == 172 && bytes[1] is >= 16 and <= 31 ||
                   bytes[0] == 192 && bytes[1] == 168;
        }

        return address.Equals(IPAddress.IPv6Any) ||
               address.IsIPv6LinkLocal ||
               address.IsIPv6SiteLocal ||
               bytes[0] is 0xfc or 0xfd ||
               IPAddress.IsLoopback(address);
    }
}
