using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using XmlRestaurantChain.Web.Data;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Controllers;

[Authorize(Roles = "Admin,Manager,Staff")]
public class ReservationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(ApplicationDbContext context, ILogger<ReservationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var restaurants = await _context.Restaurants.Include(r => r.Tables).OrderBy(r => r.Name).ToListAsync();
        var tables = await _context.DiningTables.Include(t => t.Restaurant).OrderBy(t => t.Name).ToListAsync();
        var reservations = await _context.Reservations
            .Include(r => r.Restaurant)
            .Include(r => r.DiningTable)
            .OrderByDescending(r => r.ReservedAt)
            .Take(50)
            .ToListAsync();

        var vm = new ReservationsPageViewModel
        {
            Restaurants = restaurants,
            Tables = tables,
            Reservations = reservations,
            NewReservation = new Reservation
            {
                RestaurantId = restaurants.FirstOrDefault()?.Id ?? 0,
                DiningTableId = tables.FirstOrDefault()?.Id ?? 0,
                ReservedAt = DateTime.UtcNow.AddHours(2),
                PartySize = 2,
                Status = ReservationStatus.Pending
            }
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reservation reservation)
    {
        reservation.ReservedAt = DateTime.SpecifyKind(reservation.ReservedAt, DateTimeKind.Utc);
        if (ModelState.IsValid)
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            TempData["Toast"] = "Đã tạo đặt bàn.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ReservationStatus status)
    {
        var res = await _context.Reservations.FindAsync(id);
        if (res != null)
        {
            res.Status = status;
            await _context.SaveChangesAsync();
            TempData["Toast"] = "Đã cập nhật trạng thái.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/reservations/export/xml")]
    public async Task<IActionResult> ExportXml()
    {
        var reservations = await _context.Reservations
            .Include(r => r.Restaurant)
            .Include(r => r.DiningTable)
            .OrderByDescending(r => r.ReservedAt)
            .Take(200)
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

    private IActionResult XmlFile<T>(T data, string fileName)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, data);
        return File(stream.ToArray(), "application/xml", fileName);
    }
}
