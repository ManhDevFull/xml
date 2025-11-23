namespace XmlRestaurantChain.Web.Models;

public class DashboardViewModel
{
    public IList<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
    public IList<MenuCategory> Categories { get; set; } = new List<MenuCategory>();
    public IList<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    public IList<DiningTable> Tables { get; set; } = new List<DiningTable>();
    public IList<Reservation> UpcomingReservations { get; set; } = new List<Reservation>();
    public IList<Order> ActiveOrders { get; set; } = new List<Order>();
    public IList<InventoryItem> LowInventory { get; set; } = new List<InventoryItem>();
    public IList<StaffProfile> Staff { get; set; } = new List<StaffProfile>();
    public IList<LoyaltyMember> LoyaltyMembers { get; set; } = new List<LoyaltyMember>();
    public IList<Supplier> Suppliers { get; set; } = new List<Supplier>();
    public DashboardMetrics Metrics { get; set; } = new();
    public NewRecordForms Forms { get; set; } = new();
}

public class DashboardMetrics
{
    public decimal TodayRevenue { get; set; }
    public int Reservations { get; set; }
    public int Guests { get; set; }
    public int OpenOrders { get; set; }
    public int InventoryAlerts { get; set; }
}

public class NewRecordForms
{
    public Restaurant NewRestaurant { get; set; } = new();
    public MenuCategory NewCategory { get; set; } = new();
    public MenuItem NewMenuItem { get; set; } = new();
    public DiningTable NewTable { get; set; } = new();
    public Reservation NewReservation { get; set; } = new() { ReservedAt = DateTime.UtcNow.AddHours(2), PartySize = 2, Status = ReservationStatus.Pending };
    public OrderCreateModel NewOrder { get; set; } = new();
    public InventoryItem NewInventoryItem { get; set; } = new() { Quantity = 1, ReorderLevel = 5 };
    public Supplier NewSupplier { get; set; } = new();
    public LoyaltyMember NewLoyaltyMember { get; set; } = new();
}

public class OrderCreateModel
{
    public int RestaurantId { get; set; }
    public int? DiningTableId { get; set; }
    public int? ReservationId { get; set; }
    public OrderType Type { get; set; } = OrderType.DineIn;
    public string? Notes { get; set; }
    public List<int> SelectedMenuItemIds { get; set; } = new();
}
