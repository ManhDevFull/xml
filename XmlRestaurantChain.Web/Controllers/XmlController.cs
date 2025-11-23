using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XmlRestaurantChain.Web.Data;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Controllers;

[Authorize(Roles = "Admin,Manager,Staff")]
public class XmlController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<XmlController> _logger;

    public XmlController(ApplicationDbContext context, ILogger<XmlController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("/xml/tools")]
    public async Task<IActionResult> Index()
    {
        var restaurants = await _context.Restaurants.OrderBy(r => r.Name).ToListAsync();
        return View(restaurants);
    }

    [HttpGet("/xml/export/menu")]
    public async Task<IActionResult> ExportMenu()
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

    [HttpGet("/xml/export/reservations")]
    public async Task<IActionResult> ExportReservations()
    {
        var reservations = await _context.Reservations
            .Include(r => r.Restaurant)
            .Include(r => r.DiningTable)
            .OrderByDescending(r => r.ReservedAt)
            .Take(100)
            .ToListAsync();

        var dto = new ReservationExportXml
        {
            Reservations = reservations.Select(r => new ReservationXml
            {
                Id = r.Id,
                CustomerName = r.CustomerName,
                Phone = r.CustomerPhone,
                ReservedAt = r.ReservedAt,
                PartySize = r.PartySize,
                Status = r.Status.ToString(),
                Restaurant = r.Restaurant.Name,
                Table = r.DiningTable.Name,
                Notes = r.Notes
            }).ToList()
        };

        return XmlFile(dto, "reservations.xml");
    }

    [HttpGet("/xml/export/inventory")]
    public async Task<IActionResult> ExportInventory()
    {
        var items = await _context.InventoryItems
            .Include(i => i.Restaurant)
            .Include(i => i.Supplier)
            .OrderBy(i => i.Name)
            .ToListAsync();

        var dto = new InventoryExportXml
        {
            Items = items.Select(i => new InventoryItemXml
            {
                Name = i.Name,
                Unit = i.Unit,
                Quantity = i.Quantity,
                ReorderLevel = i.ReorderLevel,
                Restaurant = i.Restaurant.Name,
                Supplier = i.Supplier?.Name ?? string.Empty
            }).ToList()
        };

        return XmlFile(dto, "inventory.xml");
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("/xml/import/menu")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportMenu([FromForm] int restaurantId, MenuImportXml import, IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Toast"] = "Chọn file XML hợp lệ.";
            return RedirectToAction("Index");
        }

        var parsed = await DeserializeAsync<MenuImportXml>(file);
        if (parsed == null)
        {
            TempData["Toast"] = "Không đọc được file XML.";
            return RedirectToAction("Index");
        }

        var targetRestaurantId = restaurantId != 0 ? restaurantId : parsed.RestaurantId;
        var exists = await _context.Restaurants.AnyAsync(r => r.Id == targetRestaurantId);
        if (!exists)
        {
            TempData["Toast"] = $"RestaurantId={targetRestaurantId} không tồn tại. Chọn lại trong danh sách.";
            return RedirectToAction("Index");
        }

        var categories = parsed.Categories;
        foreach (var cat in categories)
        {
            var category = new MenuCategory
            {
                Name = cat.Name,
                Description = cat.Name,
                RestaurantId = targetRestaurantId
            };
            _context.MenuCategories.Add(category);
            await _context.SaveChangesAsync();

            foreach (var item in cat.Items)
            {
                _context.MenuItems.Add(new MenuItem
                {
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    IsAvailable = true,
                    MenuCategoryId = category.Id
                });
            }
        }

        await _context.SaveChangesAsync();
        TempData["Toast"] = "Import menu từ XML thành công.";
        return RedirectToAction("Index");
    }

    private IActionResult XmlFile<T>(T data, string fileName)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, data);
        var bytes = stream.ToArray();
        return File(bytes, "application/xml", fileName);
    }

    private async Task<T?> DeserializeAsync<T>(IFormFile file) where T : class
    {
        try
        {
            await using var stream = file.OpenReadStream();
            var serializer = new XmlSerializer(typeof(T));
            var result = serializer.Deserialize(stream);
            return result as T;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi parse XML import");
            return null;
        }
    }
}
