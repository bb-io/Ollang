using Apps.Ollang.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Ollang.Connections;

public class ConnectionValidator(InvocationContext invocationContext) : BaseInvocable(invocationContext), IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new OllangClient(authenticationCredentialsProviders);
            var request = new RestRequest("/integration/folder", Method.Get)
                .AddQueryParameter("page", "1")
                .AddQueryParameter("take", "1");

            var response = await client.ExecuteAsync(request, cancellationToken);

            var isAuthenticationFailure = response.StatusCode is System.Net.HttpStatusCode.Unauthorized
                or System.Net.HttpStatusCode.Forbidden;
            var isValid = !isAuthenticationFailure;
            var responseDetail = response.Content ?? response.ErrorMessage ?? response.StatusCode.ToString();

            return new ConnectionValidationResponse
            {
                IsValid = isValid,
                Message = isAuthenticationFailure
                    ? responseDetail
                    : response.IsSuccessStatusCode
                        ? "Success"
                        : $"Ollang could not validate the connection because the service returned " +
                          $"{(int)response.StatusCode}: {responseDetail}",
            };

        } 
        catch(Exception ex)
        {
            InvocationContext.Logger?.LogError($"Connection validation failed: {ex.Message}", []);

            return new()
            {
                IsValid = true,
                Message = $"The Ollang service could not be reached during validation: {ex.Message}"
            };
        }

    }
}
