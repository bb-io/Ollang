using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Tests.Ollang.Base;

public class FileManager : IFileManagementClient
{
    private readonly string inputFolder;
    private readonly string outputFolder;

    public FileManager()
    {
        var projectDirectory = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..",
            "..",
            ".."));
        var testFilesPath = Path.Combine(projectDirectory, "TestFiles");
        inputFolder = Path.Combine(testFilesPath, "Input");
        outputFolder = Path.Combine(testFilesPath, "Output");

        Directory.CreateDirectory(inputFolder);
        Directory.CreateDirectory(outputFolder);
    }


    public Task<Stream> DownloadAsync(FileReference reference)
    {
        var path = Path.Combine(inputFolder, reference.Name);
        Assert.IsTrue(File.Exists(path), $"File not found at: {path}");
        var bytes = File.ReadAllBytes(path);

        var stream = new MemoryStream(bytes);
        return Task.FromResult((Stream)stream);
    }

    public Task<FileReference> UploadAsync(Stream stream, string contentType, string fileName)
    {
        var path = Path.Combine(outputFolder, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using (var fileStream = File.Create(path))
        {
            stream.CopyTo(fileStream);
        }

        return Task.FromResult(new FileReference() { Name = fileName, ContentType = contentType });
    }
}

