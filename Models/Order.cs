using System.ComponentModel.DataAnnotations.Schema;

namespace BizSecureDemo22180092.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }  // owner
    public string Title { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
