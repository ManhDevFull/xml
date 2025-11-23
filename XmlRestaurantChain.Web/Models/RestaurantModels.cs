namespace XmlRestaurantChain.Web.Models;

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = "#0f172a";
    public string ManagerNote { get; set; } = "Tận tâm phục vụ - Luxury Dining.";

    public ICollection<MenuCategory> Categories { get; set; } = new List<MenuCategory>();
    public ICollection<DiningTable> Tables { get; set; } = new List<DiningTable>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}

public class DiningTable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public TableStatus Status { get; set; }

    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = default!;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public enum TableStatus
{
    Available,
    Reserved,
    Occupied,
    Maintenance
}

public class Reservation
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public int PartySize { get; set; }
    public string? Notes { get; set; }
    public ReservationStatus Status { get; set; }

    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = default!;

    public int DiningTableId { get; set; }
    public DiningTable DiningTable { get; set; } = default!;
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    CheckedIn,
    Completed,
    Cancelled
}
