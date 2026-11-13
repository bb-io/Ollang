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

}
