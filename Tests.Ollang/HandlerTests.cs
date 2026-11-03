using Apps.Ollang.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Ollang.Base;

namespace Tests.Ollang;

[TestClass]
public class HandlerTests : TestBase
{
    [TestMethod]
    public async Task Dynamic_handler_works()
    {
        // Arrange
        var handler = new DynamicHandler(InvocationContext);

        // Act
        var result = await handler.GetDataAsync(new DataSourceContext { }, CancellationToken.None);

        // Assert
        PrintDataHandlerResult(result);
        Assert.IsTrue(result.Count() > 0);
    }
}
