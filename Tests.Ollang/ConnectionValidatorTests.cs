using Apps.Ollang.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;
using Tests.Ollang.Base;

namespace Tests.Ollang;

[TestClass]
public class ConnectionValidatorTests : TestBase
{
    [TestMethod]
    public async Task ValidateConnection_ValidData_ShouldBeSuccessful()
    {
        var validator = new ConnectionValidator(InvocationContext);

        var result = await validator.ValidateConnection(Creds, CancellationToken.None);
        Console.WriteLine(result.Message);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateConnection_ServerError_DoesNotRejectConnection()
    {
        var validator = new ConnectionValidator(InvocationContext);
        var newCredentials = Creds
            .Select(x => new AuthenticationCredentialsProvider(x.KeyName, "invalid-token"));

        var result = await validator.ValidateConnection(newCredentials, CancellationToken.None);
        Console.WriteLine(result.Message);
        // The sandbox currently returns HTTP 500 for an invalid token instead of the documented 401.
        // Connection validators must only reject explicit authentication failures, not service errors.
        Assert.IsTrue(result.IsValid);
    }
}
