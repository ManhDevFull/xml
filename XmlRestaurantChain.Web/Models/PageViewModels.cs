namespace XmlRestaurantChain.Web.Models;

public class ReservationsPageViewModel
{
    public List<Reservation> Reservations { get; set; } = new();
    public Reservation NewReservation { get; set; } = new();
    public List<Restaurant> Restaurants { get; set; } = new();
    public List<DiningTable> Tables { get; set; } = new();
}

public class MenuPageViewModel
{
    public List<MenuCategory> Categories { get; set; } = new();
    public List<MenuItem> Items { get; set; } = new();
    public MenuCategory NewCategory { get; set; } = new();
    public MenuItem NewMenuItem { get; set; } = new();
    public List<Restaurant> Restaurants { get; set; } = new();
}

public class InventoryPageViewModel
{
    public List<InventoryItem> Items { get; set; } = new();
    public InventoryItem NewItem { get; set; } = new();
    public Supplier NewSupplier { get; set; } = new();
    public List<Restaurant> Restaurants { get; set; } = new();
    public List<Supplier> Suppliers { get; set; } = new();
}

public class OrdersPageViewModel
{
    public List<Order> Orders { get; set; } = new();
    public OrderCreateModel NewOrder { get; set; } = new();
    public List<Restaurant> Restaurants { get; set; } = new();
    public List<DiningTable> Tables { get; set; } = new();
    public List<MenuItem> MenuItems { get; set; } = new();
    public List<Reservation> Reservations { get; set; } = new();
}

public class RestaurantsPageViewModel
{
    public List<Restaurant> Restaurants { get; set; } = new();
    public Restaurant NewRestaurant { get; set; } = new();
}

public class LoyaltyPageViewModel
{
    public List<LoyaltyMember> Members { get; set; } = new();
    public LoyaltyMember NewMember { get; set; } = new();
    public List<Restaurant> Restaurants { get; set; } = new();
}

public class StaffPageViewModel
{
    public List<StaffProfile> Staff { get; set; } = new();
}
