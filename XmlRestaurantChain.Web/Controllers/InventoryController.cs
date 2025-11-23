using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using XmlRestaurantChain.Web.Data;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var restaurants = await _context.Restaurants.OrderBy(r => r.Name).ToListAsync();
        var suppliers = await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();
        var items = await _context.InventoryItems.Include(i => i.Restaurant).Include(i => i.Supplier).OrderBy(i => i.Name).ToListAsync();

        var vm = new InventoryPageViewModel
        {
            Restaurants = restaurants,
            Suppliers = suppliers,
            Items = items,
            NewItem = new InventoryItem { RestaurantId = restaurants.FirstOrDefault()?.Id ?? 0, Quantity = 1, ReorderLevel = 5, Unit = "unit" },
            NewSupplier = new Supplier()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateItem(InventoryItem item)
    {
        if (ModelState.IsValid)
        {
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();
            TempData["Toast"] = "Đã thêm nguyên liệu.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSupplier(Supplier supplier)
    {
        if (ModelState.IsValid)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            TempData["Toast"] = "Đã thêm NCC.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/inventory/export/xml")]
    public async Task<IActionResult> ExportXml()
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

    private IActionResult XmlFile<T>(T data, string fileName)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, data);
        return File(stream.ToArray(), "application/xml", fileName);
    }
}
