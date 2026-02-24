namespace BizSecureDemo22180092.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";

    // Ex 4: Brute-force lockout protection
    public int? FailedLogins { get; set; }
    public DateTime? LockoutUntilUtc { get; set; }
}
