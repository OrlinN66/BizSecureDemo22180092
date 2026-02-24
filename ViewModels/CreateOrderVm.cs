using System.ComponentModel.DataAnnotations;

namespace BizSecureDemo22180092.ViewModels;

public class CreateOrderVm
{
    // VULNERABLE: MaxLength 300 allows long XSS payloads
    [Required, MaxLength(300)]
    public string Title { get; set; } = "";

    [Required]
    public decimal Amount { get; set; }
}
