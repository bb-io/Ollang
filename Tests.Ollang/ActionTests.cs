using Apps.Ollang.Actions;
using Apps.Ollang.Handlers;
using Apps.Ollang.Models.Requests;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Ollang.Base;

namespace Tests.Ollang;

[TestClass]
public class ActionTests : TestBase
{
    [TestMethod]
    public async Task Search_orders_returns_non_null_collection()
    {
        var actions = new OrderActions(InvocationContext, FileManager);

        var result = await actions.SearchOrders(new SearchOrdersRequest());

        Assert.IsNotNull(result.Orders);
        PrintJsonResult(result);
    }

    [TestMethod]
    public async Task Search_orders_respects_created_at_range()
    {
        var actions = new OrderActions(InvocationContext, FileManager);

        var result = await actions.SearchOrders(new SearchOrdersRequest
        {
            CreatedFrom = new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedTo = new DateTime(2099, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.AreEqual(0, result.Orders.Count());
    }

    [TestMethod]
    public async Task Search_orders_rejects_inverted_created_at_range()
    {
        var actions = new OrderActions(InvocationContext, FileManager);

        await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() =>
            actions.SearchOrders(new SearchOrdersRequest
            {
                CreatedFrom = new DateTime(2026, 11, 18, 0, 0, 0, DateTimeKind.Utc),
                CreatedTo = new DateTime(2026, 11, 17, 0, 0, 0, DateTimeKind.Utc)
            }));
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

    [TestMethod]
    public async Task Download_order_deliverables_returns_all_selected_document_types()
    {
        string[] selectedDocumentTypes =
        [
            "created_ai_dub_vocals_only_audio",
            "created_embedded_video",
            "on_screen_text_xlsx"
        ];
        var input = new DownloadOrderDeliverablesRequest
        {
            OrderId = "6adb70a7677783f70447a217",
            DocumentTypes = selectedDocumentTypes,
            //NameContains = ".srt",
            IncludeSubtitleEmbeddedVideo = true
        };
        var actions = new FileActions(InvocationContext, FileManager);

        var result = await actions.DownloadOrderDeliverables(input);
        PrintJsonResult(result);

        var returnedDocumentTypes = result.Deliverables
            .Select(deliverable => deliverable.DocumentType)
            .Where(documentType => !string.IsNullOrWhiteSpace(documentType))
            .Cast<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var selectedDocumentType in selectedDocumentTypes)
        {
            Assert.IsTrue(
                returnedDocumentTypes.Contains(selectedDocumentType),
                $"The action did not return selected document type '{selectedDocumentType}'.");
        }
        Assert.AreEqual(selectedDocumentTypes.Length, result.Deliverables.Count);
    }
}
