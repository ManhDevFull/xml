using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using XmlRestaurantChain.Web.Data;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Controllers;

[Authorize(Roles = "Admin,Manager,Staff")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var restaurants = await _context.Restaurants.OrderBy(r => r.Name).ToListAsync();
        var tables = await _context.DiningTables.Include(t => t.Restaurant).OrderBy(t => t.Name).ToListAsync();
        var reservations = await _context.Reservations.OrderByDescending(r => r.ReservedAt).Take(50).ToListAsync();
        var menuItems = await _context.MenuItems.Include(m => m.MenuCategory).ThenInclude(c => c.Restaurant).OrderBy(m => m.Name).ToListAsync();
        var orders = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.DiningTable)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .OrderByDescending(o => o.CreatedAt)
            .Take(50)
            .ToListAsync();

        var vm = new OrdersPageViewModel
        {
            Restaurants = restaurants,
            Tables = tables,
            MenuItems = menuItems,
            Reservations = reservations,
            Orders = orders,
            NewOrder = new OrderCreateModel
            {
                RestaurantId = restaurants.FirstOrDefault()?.Id ?? 0,
                DiningTableId = tables.FirstOrDefault()?.Id,
                Type = OrderType.DineIn
            }
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCreateModel model)
    {
        if (model.SelectedMenuItemIds.Count == 0)
        {
            TempData["Toast"] = "Chọn ít nhất một món.";
            return RedirectToAction(nameof(Index));
        }

        var menuItems = await _context.MenuItems.Where(m => model.SelectedMenuItemIds.Contains(m.Id)).ToListAsync();
        if (!menuItems.Any())
        {
            TempData["Toast"] = "Không tìm thấy món.";
            return RedirectToAction(nameof(Index));
        }

        var order = new Order
        {
            RestaurantId = model.RestaurantId,
            DiningTableId = model.DiningTableId,
            ReservationId = model.ReservationId,
            Type = model.Type,
            Status = OrderStatus.New,
            Notes = model.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var orderItems = menuItems.Select(m => new OrderItem
        {
            Order = order,
            MenuItemId = m.Id,
            Quantity = 1,
            UnitPrice = m.Price,
            LineTotal = m.Price
        }).ToList();

        order.Total = orderItems.Sum(i => i.LineTotal);
        _context.Orders.Add(order);
        _context.OrderItems.AddRange(orderItems);
        await _context.SaveChangesAsync();

        TempData["Toast"] = "Đã tạo order.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["Toast"] = "Đã cập nhật trạng thái order.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/orders/export/xml")]
    public async Task<IActionResult> ExportXml()
    {
        var orders = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.DiningTable)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .OrderByDescending(o => o.CreatedAt)
            .Take(100)
            .ToListAsync();

        var serializer = new XmlSerializer(typeof(List<OrderExportDto>));
        var dto = orders.Select(o => new OrderExportDto
        {
            Id = o.Id,
            Restaurant = o.Restaurant.Name,
            Table = o.DiningTable?.Name ?? "",
            Type = o.Type.ToString(),
            Status = o.Status.ToString(),
            Total = o.Total,
            CreatedAt = o.CreatedAt,
            Items = o.Items.Select(i => new OrderExportItemDto
            {
                Name = i.MenuItem.Name,
                Price = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        }).ToList();

        using var stream = new MemoryStream();
        serializer.Serialize(stream, dto);
        return File(stream.ToArray(), "application/xml", "orders.xml");
    }
}

public class OrderExportDto
{
    public int Id { get; set; }
    public string Restaurant { get; set; } = string.Empty;
    public string Table { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderExportItemDto> Items { get; set; } = new();
}

public class OrderExportItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
