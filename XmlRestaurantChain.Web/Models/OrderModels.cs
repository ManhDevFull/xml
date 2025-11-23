namespace XmlRestaurantChain.Web.Models;

public class Order
{
    public int Id { get; set; }
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = default!;

    public int? ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public int? DiningTableId { get; set; }
    public DiningTable? DiningTable { get; set; }

    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = default!;
}

public enum OrderType
{
    DineIn,
    Delivery,
    Pickup
}

public enum OrderStatus
{
    New,
    InProgress,
    Completed,
    Paid,
    Cancelled
}
