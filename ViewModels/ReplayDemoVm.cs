namespace BizSecureDemo22180092.ViewModels;

public class ReplayDemoVm
{
    public decimal Balance { get; set; }

    public string? Message { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; } = 100;

    public string Token { get; set; } = "SECRET123";
}
