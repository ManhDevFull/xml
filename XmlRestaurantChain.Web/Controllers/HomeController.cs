using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XmlRestaurantChain.Web.Data;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var restaurants = await _context.Restaurants.Include(r => r.Categories).ToListAsync();
        var menuItems = await _context.MenuItems.Include(m => m.MenuCategory).Take(6).ToListAsync();
        ViewBag.Restaurants = restaurants;
        ViewBag.FeaturedMenu = menuItems;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
