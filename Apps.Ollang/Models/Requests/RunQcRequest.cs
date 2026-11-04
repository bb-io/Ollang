using Apps.Ollang.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Ollang.Models.Requests;

public class RunQcRequest
{
    [Display("Order ID")]
    [DataSource(typeof(OrderDataSourceHandler))]
    public string OrderId { get; set; } = string.Empty;

    [Display("Accuracy", Description = "Evaluate translation accuracy. Defaults to true.")]
    public bool? Accuracy { get; set; }

    [Display("Fluency", Description = "Evaluate fluency. Defaults to true.")]
    public bool? Fluency { get; set; }

    [Display("Tone", Description = "Evaluate tone and style. Defaults to true.")]
    public bool? Tone { get; set; }

    [Display("Cultural fit", Description = "Evaluate cultural appropriateness. Defaults to true.")]
    public bool? CulturalFit { get; set; }

    [Display("Custom prompt", Description = "Optional instructions for the evaluator.")]
    public string? CustomPrompt { get; set; }

    [Display("Callback URL", Description = "Optional public HTTPS URL called when evaluation completes.")]
    public string? CallbackUrl { get; set; }
}
