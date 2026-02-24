using System.ComponentModel.DataAnnotations;

namespace BizSecureDemo22180092.ViewModels;

public class RegisterVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";
}
