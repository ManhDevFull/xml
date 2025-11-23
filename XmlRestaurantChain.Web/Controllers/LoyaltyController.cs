using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using XmlRestaurantChain.Web.Data;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Controllers;

[Authorize(Roles = "Admin,Manager,Staff")]
public class LoyaltyController : Controller
{
    private readonly ApplicationDbContext _context;

    public LoyaltyController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var restaurants = await _context.Restaurants.OrderBy(r => r.Name).ToListAsync();
        var members = await _context.LoyaltyMembers.Include(l => l.Restaurant).OrderByDescending(l => l.Points).ToListAsync();

        var vm = new LoyaltyPageViewModel
        {
            Restaurants = restaurants,
            Members = members,
            NewMember = new LoyaltyMember { RestaurantId = restaurants.FirstOrDefault()?.Id ?? 0, Points = 0, Tier = "Silver" }
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LoyaltyMember member)
    {
        if (ModelState.IsValid)
        {
            _context.LoyaltyMembers.Add(member);
            await _context.SaveChangesAsync();
            TempData["Toast"] = "Đã lưu khách hàng thân thiết.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/loyalty/export/xml")]
    public async Task<IActionResult> ExportXml()
    {
        var members = await _context.LoyaltyMembers.Include(l => l.Restaurant).ToListAsync();
        var serializer = new XmlSerializer(typeof(List<LoyaltyMember>));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, members);
        return File(stream.ToArray(), "application/xml", "loyalty.xml");
    }
}
