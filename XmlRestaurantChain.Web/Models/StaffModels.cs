namespace XmlRestaurantChain.Web.Models;

public class StaffProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = default!;

    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = default!;

    public string Position { get; set; } = "Staff";
    public bool Active { get; set; } = true;
    public DateTime HiredDate { get; set; } = DateTime.UtcNow;
}

public class LoyaltyMember
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int Points { get; set; }
    public string Tier { get; set; } = "Silver";
    public DateTime LastVisitedAt { get; set; } = DateTime.UtcNow;

    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = default!;
}
