using System.ComponentModel.DataAnnotations;

namespace BizSecureDemo22180092.ViewModels;

public class CreateOrderVm
{
    // FIXED: Reduced MaxLength to 80 - limits XSS payload length
    [Required, MaxLength(80)]
    public string Title { get; set; } = "";

    [Required]
    public decimal Amount { get; set; }
}
