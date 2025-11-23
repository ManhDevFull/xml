using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using XmlRestaurantChain.Web.Data;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class MenuController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MenuController> _logger;

    public MenuController(ApplicationDbContext context, ILogger<MenuController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var restaurants = await _context.Restaurants.OrderBy(r => r.Name).ToListAsync();
        var categories = await _context.MenuCategories.Include(c => c.Restaurant).OrderBy(c => c.Name).ToListAsync();
        var items = await _context.MenuItems.Include(m => m.MenuCategory).ThenInclude(c => c.Restaurant).OrderBy(m => m.Name).ToListAsync();

        var vm = new MenuPageViewModel
        {
            Restaurants = restaurants,
            Categories = categories,
            Items = items,
            NewCategory = new MenuCategory { RestaurantId = restaurants.FirstOrDefault()?.Id ?? 0 },
            NewMenuItem = new MenuItem { MenuCategoryId = categories.FirstOrDefault()?.Id ?? 0, IsAvailable = true }
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(MenuCategory category)
    {
        if (ModelState.IsValid)
        {
            _context.MenuCategories.Add(category);
            await _context.SaveChangesAsync();
            TempData["Toast"] = "Đã thêm danh mục.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateItem(MenuItem item)
    {
        if (ModelState.IsValid)
        {
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();
            TempData["Toast"] = "Đã thêm món.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(MenuImportXml import, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Toast"] = "Chọn file XML hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var parsed = await DeserializeAsync<MenuImportXml>(file);
        if (parsed == null)
        {
            TempData["Toast"] = "Không đọc được XML.";
            return RedirectToAction(nameof(Index));
        }

        var targetRestaurantId = import.RestaurantId != 0 ? import.RestaurantId : parsed.RestaurantId;
        var exists = await _context.Restaurants.AnyAsync(r => r.Id == targetRestaurantId);
        if (!exists)
        {
            TempData["Toast"] = "RestaurantId không tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var cat in parsed.Categories)
        {
            var category = new MenuCategory
            {
                Name = cat.Name,
                Description = cat.Name,
                RestaurantId = targetRestaurantId
            };
            _context.MenuCategories.Add(category);
            await _context.SaveChangesAsync();

            foreach (var mi in cat.Items)
            {
                _context.MenuItems.Add(new MenuItem
                {
                    Name = mi.Name,
                    Description = mi.Description,
                    Price = mi.Price,
                    IsAvailable = true,
                    MenuCategoryId = category.Id
                });
            }
        }

        await _context.SaveChangesAsync();
        TempData["Toast"] = "Import menu XML thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/menu/export/xml")]
    public async Task<IActionResult> ExportXml()
    {
        var data = await _context.Restaurants
            .Include(r => r.Categories)
            .ThenInclude(c => c.MenuItems)
            .ToListAsync();

        var dto = new MenuExportXml
        {
            Restaurants = data.Select(r => new RestaurantMenuXml
            {
                Id = r.Id,
                Name = r.Name,
                Categories = r.Categories.Select(c => new MenuCategoryXml
                {
                    Name = c.Name,
                    Items = c.MenuItems.Select(m => new MenuItemXml
                    {
                        Name = m.Name,
                        Price = m.Price,
                        Description = m.Description
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        return XmlFile(dto, "menu.xml");
    }

    private IActionResult XmlFile<T>(T data, string fileName)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, data);
        return File(stream.ToArray(), "application/xml", fileName);
    }

    private async Task<T?> DeserializeAsync<T>(IFormFile file) where T : class
    {
        try
        {
            await using var stream = file.OpenReadStream();
            var serializer = new XmlSerializer(typeof(T));
            return serializer.Deserialize(stream) as T;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi parse XML");
            return null;
        }
    }
}
