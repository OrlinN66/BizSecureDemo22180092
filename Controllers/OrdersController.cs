using BizSecureDemo22180092.Data;
using BizSecureDemo22180092.Models;
using BizSecureDemo22180092.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BizSecureDemo22180092.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderVm vm)
    {
        if (!ModelState.IsValid) return RedirectToAction("Index", "Home");

        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        _db.
        Orders.Add(new Order { UserId = uid, Title = vm.Title, Amount = vm.Amount });
        await _db.SaveChangesAsync();

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Details(int id)
    {
        // FIXED: Check ownership - prevents IDOR attacks
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == uid);

        if (order == null) return NotFound();

        return View(order);
    }

    // VULNERABLE: SQL Injection - String concatenation
    public async Task<IActionResult> SearchVulnerable(string? keyword)
    {
        if (string.IsNullOrEmpty(keyword))
            return View("SearchResults", new List<Order>());

        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // VULNERABLE: Direct string concatenation - SQL Injection risk!
        var query = $"SELECT * FROM Orders WHERE UserId = {uid} AND (Title LIKE '%{keyword}%' OR Amount LIKE '%{keyword}%')";
        
        var results = await _db.Orders.FromSqlRaw(query).ToListAsync();
        return View("SearchResults", results);
    }
}
