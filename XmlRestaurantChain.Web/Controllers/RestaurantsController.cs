using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using XmlRestaurantChain.Web.Data;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class RestaurantsController : Controller
{
    private readonly ApplicationDbContext _context;

    public RestaurantsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var restaurants = await _context.Restaurants.Include(r => r.Tables).Include(r => r.Categories).ToListAsync();
        var vm = new RestaurantsPageViewModel
        {
            Restaurants = restaurants,
            NewRestaurant = new Restaurant { ThemeColor = "#0ea5e9", City = "Da Nang", Country = "Vietnam" }
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Restaurant restaurant)
    {
        if (ModelState.IsValid)
        {
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
            TempData["Toast"] = "Đã thêm nhà hàng.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/restaurants/export/xml")]
    public async Task<IActionResult> ExportXml()
    {
        var restaurants = await _context.Restaurants.Include(r => r.Tables).Include(r => r.Categories).ToListAsync();
        var serializer = new XmlSerializer(typeof(List<Restaurant>));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, restaurants);
        return File(stream.ToArray(), "application/xml", "restaurants.xml");
    }
}
