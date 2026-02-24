using System.ComponentModel.DataAnnotations;

namespace BizSecureDemo22180092.ViewModels;

public class CreateOrderVm
{
    // Ex2: MaxLength reverted back to 80 after XSS demo (was 300 during vulnerability demo)
    [Required, MaxLength(80)]
    public string Title { get; set; } = "";

    [Required]
    public decimal Amount { get; set; }
}
