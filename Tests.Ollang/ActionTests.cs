using Apps.Ollang.Actions;
using Tests.Ollang.Base;

namespace Tests.Ollang;

[TestClass]
public class ActionTests : TestBase
{
    [TestMethod]
    public async Task Action_IsSuccess()
    {
        var actions = new Actions(InvocationContext);

        await actions.Action();
    }
}
