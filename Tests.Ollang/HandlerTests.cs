using Apps.Ollang.Handlers;
using Apps.Ollang.Handlers.Static;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Ollang.Base;

namespace Tests.Ollang;

[TestClass]
public class HandlerTests : TestBase
{
    [TestMethod]
    public async Task Folder_handler_returns_items()
    {
        var handler = new FolderDataSourceHandler(InvocationContext);

        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        PrintDataHandlerResult(result);
        Assert.IsTrue(result.Any());
    }

    [TestMethod]
    public async Task Project_handler_returns_items()
    {
        var handler = new ProjectDataSourceHandler(InvocationContext);

        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        PrintDataHandlerResult(result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Language_handler_returns_unique_values()
    {
        var result = new LanguageDataSourceHandler().GetData().ToList();

        Assert.IsTrue(result.Count > 200);
        Assert.AreEqual(result.Count, result.Select(item => item.Value).Distinct().Count());
        Assert.IsTrue(result.Any(item => item.Value == "uk"));
    }

    [TestMethod]
    public void Vtt_language_handler_includes_all_languages()
    {
        var result = new VttLanguageDataSourceHandler().GetData().ToList();

        Assert.AreEqual(1, result.Count(item => item.Value == "all"));
        Assert.AreEqual(result.Count, result.Select(item => item.Value).Distinct().Count());
    }
}
