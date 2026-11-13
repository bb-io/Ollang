using RestSharp;

namespace Apps.Ollang.Extensions;

public static class RequestParameterExtensions
{
    public static RestRequest AddOptionalParameter(
        this RestRequest request,
        string name,
        string? value,
        ParameterType parameterType = ParameterType.GetOrPost)
    {
        if (!string.IsNullOrWhiteSpace(value))
            request.AddParameter(name, value, parameterType);

        return request;
    }
}
