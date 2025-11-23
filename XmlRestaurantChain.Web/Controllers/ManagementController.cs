using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XmlRestaurantChain.Web.Data;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Controllers;

[Authorize]
public class ManagementController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ManagementController> _logger;

    public ManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ManagementController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Dashboard()
    {
        var vm = await BuildDashboardAsync();
        return View(vm);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRestaurant(Restaurant model)
    {
        if (string.IsNullOrWhiteSpace(model.ThemeColor))
        {
            model.ThemeColor = "#0ea5e9";
        }

        if (ModelState.IsValid)
        {
            _context.Restaurants.Add(model);
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Đã tạo nhà hàng {model.Name}";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(MenuCategory model)
    {
        if (ModelState.IsValid)
        {
            _context.MenuCategories.Add(model);
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Đã thêm danh mục {model.Name}";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMenuItem(MenuItem model)
    {
        if (ModelState.IsValid)
        {
            _context.MenuItems.Add(model);
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Đã thêm món {model.Name}";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTable(DiningTable model)
    {
        if (ModelState.IsValid)
        {
            _context.DiningTables.Add(model);
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Đã thêm bàn {model.Name}";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin,Manager,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReservation(Reservation model)
    {
        model.ReservedAt = DateTime.SpecifyKind(model.ReservedAt, DateTimeKind.Utc);

        if (ModelState.IsValid)
        {
            _context.Reservations.Add(model);
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Đã đặt bàn cho {model.CustomerName}";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin,Manager,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(OrderCreateModel model)
    {
        if (model.SelectedMenuItemIds.Count == 0)
        {
            TempData["Toast"] = "Chọn ít nhất 1 món cho order.";
            return RedirectToAction(nameof(Dashboard));
        }

        var menuItems = await _context.MenuItems
            .Where(m => model.SelectedMenuItemIds.Contains(m.Id))
            .ToListAsync();

        if (!menuItems.Any())
        {
            TempData["Toast"] = "Không tìm thấy món ăn cho order.";
            return RedirectToAction(nameof(Dashboard));
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

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            order.CreatedById = user.Id;
        }

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

        TempData["Toast"] = "Tạo order thành công.";
        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin,Manager,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Đã cập nhật trạng thái order #{id}";
        }
        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin,Manager,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTableStatus(int id, TableStatus status)
    {
        var table = await _context.DiningTables.FindAsync(id);
        if (table != null)
        {
            table.Status = status;
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Đã cập nhật trạng thái bàn {table.Name}";
        }
        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInventoryItem(InventoryItem model)
    {
        if (ModelState.IsValid)
        {
            _context.InventoryItems.Add(model);
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Đã thêm nguyên liệu {model.Name}";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSupplier(Supplier model)
    {
        if (ModelState.IsValid)
        {
            _context.Suppliers.Add(model);
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Đã thêm NCC {model.Name}";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "Admin,Manager,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLoyaltyMember(LoyaltyMember model)
    {
        if (ModelState.IsValid)
        {
            _context.LoyaltyMembers.Add(model);
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Đã lưu khách hàng thân thiết {model.CustomerName}";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    private async Task<DashboardViewModel> BuildDashboardAsync()
    {
        var restaurants = await _context.Restaurants
            .Include(r => r.Categories)
            .Include(r => r.Tables)
            .OrderBy(r => r.Name)
            .ToListAsync();

        var categories = await _context.MenuCategories.Include(c => c.Restaurant).ToListAsync();
        var menuItems = await _context.MenuItems
            .Include(m => m.MenuCategory)
            .ThenInclude(c => c.Restaurant)
            .OrderBy(m => m.Name)
            .ToListAsync();

        var tables = await _context.DiningTables
            .Include(t => t.Restaurant)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var reservations = await _context.Reservations
            .Include(r => r.DiningTable)
            .Include(r => r.Restaurant)
            .OrderBy(r => r.ReservedAt)
            .Take(8)
            .ToListAsync();

        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.MenuItem)
            .Include(o => o.DiningTable)
            .Include(o => o.Restaurant)
            .Where(o => o.Status == OrderStatus.New || o.Status == OrderStatus.InProgress)
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .ToListAsync();

        var inventoryAlerts = await _context.InventoryItems
            .Include(i => i.Supplier)
            .Include(i => i.Restaurant)
            .Where(i => i.Quantity <= i.ReorderLevel * 1.25m)
            .OrderBy(i => i.Quantity)
            .ToListAsync();

        var staffProfiles = await _context.StaffProfiles
            .Include(s => s.User)
            .Include(s => s.Restaurant)
            .ToListAsync();

        var loyaltyMembers = await _context.LoyaltyMembers
            .Include(l => l.Restaurant)
            .OrderByDescending(l => l.Points)
            .Take(6)
            .ToListAsync();

        var suppliers = await _context.Suppliers.ToListAsync();

        var today = DateTime.UtcNow.Date;
        var todayOrders = await _context.Orders
            .Where(o => o.CreatedAt.Date == today)
            .ToListAsync();

        var todayRevenue = todayOrders.Sum(o => o.Total);
        var todayReservations = reservations.Count(r => r.ReservedAt.Date == today);
        var guests = reservations.Where(r => r.ReservedAt.Date == today).Sum(r => r.PartySize);
        var openOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.New || o.Status == OrderStatus.InProgress);

        var defaultRestaurantId = restaurants.FirstOrDefault()?.Id ?? 0;

        return new DashboardViewModel
        {
            Restaurants = restaurants,
            Categories = categories,
            MenuItems = menuItems,
            Tables = tables,
            UpcomingReservations = reservations,
            ActiveOrders = orders,
            LowInventory = inventoryAlerts,
            Staff = staffProfiles,
            LoyaltyMembers = loyaltyMembers,
            Suppliers = suppliers,
            Metrics = new DashboardMetrics
            {
                TodayRevenue = todayRevenue,
                Reservations = todayReservations,
                Guests = guests,
                OpenOrders = openOrders,
                InventoryAlerts = inventoryAlerts.Count
            },
            Forms = new NewRecordForms
            {
                NewRestaurant = new Restaurant { ThemeColor = "#0ea5e9", City = "Da Nang", Country = "Vietnam" },
                NewCategory = new MenuCategory { RestaurantId = defaultRestaurantId },
                NewMenuItem = new MenuItem { IsAvailable = true, MenuCategoryId = categories.FirstOrDefault()?.Id ?? 0 },
                NewTable = new DiningTable { Status = TableStatus.Available, RestaurantId = defaultRestaurantId, Capacity = 4 },
                NewReservation = new Reservation
                {
                    PartySize = 2,
                    ReservedAt = DateTime.UtcNow.AddHours(2),
                    Status = ReservationStatus.Pending,
                    RestaurantId = defaultRestaurantId,
                    DiningTableId = tables.FirstOrDefault()?.Id ?? 0
                },
                NewOrder = new OrderCreateModel
                {
                    RestaurantId = defaultRestaurantId,
                    DiningTableId = tables.FirstOrDefault()?.Id,
                    Type = OrderType.DineIn
                },
                NewInventoryItem = new InventoryItem { RestaurantId = defaultRestaurantId, Quantity = 1, ReorderLevel = 5, Unit = "unit" },
                NewSupplier = new Supplier(),
                NewLoyaltyMember = new LoyaltyMember { RestaurantId = defaultRestaurantId }
            }
        };
    }
}
