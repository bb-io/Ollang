using Apps.Ollang.Actions;
using Apps.Ollang.Handlers;
using Apps.Ollang.Models.Requests;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Ollang.Base;

namespace Tests.Ollang;

[TestClass]
public class ActionTests : TestBase
{
    [TestMethod]
    public async Task Search_orders_returns_non_null_collection()
    {
        var actions = new OrderActions(InvocationContext);

        var result = await actions.SearchOrders(new SearchOrdersRequest());

        Assert.IsNotNull(result.Orders);
        PrintJsonResult(result);
    }

    [TestMethod]
    public async Task Upload_workflow_is_successful()
    {
        var folderHandler = new FolderDataSourceHandler(InvocationContext);
        var folder = (await folderHandler.GetDataAsync(new DataSourceContext(), CancellationToken.None)).FirstOrDefault();
        Assert.IsNotNull(folder, "The sandbox needs at least one folder for the upload integration test.");

        var actions = new FileActions(InvocationContext, FileManager);
        var sourceResult = await actions.UploadSourceFile(new UploadSourceFileRequest
        {
            File = new FileReference { Name = "source.txt", ContentType = "text/plain" },
            Name = $"Blackbird integration test {DateTime.UtcNow:yyyyMMdd-HHmmss}",
            SourceLanguage = "en",
            FolderId = folder.Value,
            Note = "Preserve the original formatting.",
            NoteTimestamp = "00:00:01"
        });

        Assert.IsFalse(string.IsNullOrWhiteSpace(sourceResult.ProjectId));

        var vttResult = await actions.UploadVttFile(new UploadVttFileRequest
        {
            File = new FileReference { Name = "subtitles.vtt", ContentType = "text/vtt" },
            ProjectId = sourceResult.ProjectId,
            Name = "Blackbird integration subtitles",
            SourceLanguage = "en"
        });
        Assert.AreEqual(sourceResult.ProjectId, vttResult.ProjectId);

        var projectDocument = await actions.UploadReferenceDocument(new UploadReferenceDocumentRequest
        {
            Destination = "project",
            ProjectId = sourceResult.ProjectId,
            File = new FileReference { Name = "reference.txt", ContentType = "text/plain" },
            DocumentType = "guideline",
            Name = "Blackbird integration guideline",
            Language = "en"
        });
        Assert.IsFalse(string.IsNullOrWhiteSpace(projectDocument.Id));

        var folderDocument = await actions.UploadReferenceDocument(new UploadReferenceDocumentRequest
        {
            Destination = "folder",
            FolderId = folder.Value,
            File = new FileReference { Name = "reference.txt", ContentType = "text/plain" },
            DocumentType = "guideline_glossary",
            Name = "Blackbird integration glossary",
            Language = "en"
        });
        Assert.IsFalse(string.IsNullOrWhiteSpace(folderDocument.Id));
    }
}
