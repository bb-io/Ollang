using Apps.Ollang.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Ollang.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
    {
        new()
        {
            Name = "Developer API key",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>
            {
                new(CredsNames.Token)
                {
                    DisplayName = "API key",
                    Description = "API key generated in the Olabs Dashboard. Enter only the key value, without the X-Api-Key header name.",
                    Sensitive = true
                }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values) => values.Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value)
        ).ToList();
}
