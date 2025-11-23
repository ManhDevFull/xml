using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using XmlRestaurantChain.Web.Data;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class StaffController : Controller
{
    private readonly ApplicationDbContext _context;

    public StaffController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var staff = await _context.StaffProfiles.Include(s => s.User).Include(s => s.Restaurant).ToListAsync();
        var vm = new StaffPageViewModel { Staff = staff };
        return View(vm);
    }

    [HttpGet("/staff/export/xml")]
    public async Task<IActionResult> ExportXml()
    {
        var staff = await _context.StaffProfiles.Include(s => s.User).Include(s => s.Restaurant).ToListAsync();
        var serializer = new XmlSerializer(typeof(List<StaffProfile>));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, staff);
        return File(stream.ToArray(), "application/xml", "staff.xml");
    }
}
