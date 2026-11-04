using Blackbird.Applications.Sdk.Common;

namespace Apps.Ollang.Models.Responses;

public class OrderFinanceResponse
{
    [Display("Payment amount")]
    public decimal? PaymentAmount { get; set; }
}
