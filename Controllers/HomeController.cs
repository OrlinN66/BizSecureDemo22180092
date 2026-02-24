using BizSecureDemo22180092.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BizSecureDemo22180092.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // My orders
        var myOrders = await _db.Orders
            .Where(o => o.UserId == uid)
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        // Ex 2: All orders (public board) - needed for XSS demonstration
        var allOrders = await _db.Orders
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        ViewBag.AllOrders = allOrders;

        return View(myOrders);
    }
}
