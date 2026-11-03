using Newtonsoft.Json.Linq;

namespace Apps.Ollang.Dtos;

public class ErrorDto
{
    public string? Error { get; set; }

    public string? Message { get; set; }

    public JToken? Code { get; set; }

    public JToken? Data { get; set; }

    public JToken? Detail { get; set; }
}
