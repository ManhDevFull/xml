using Microsoft.AspNetCore.Identity;

namespace XmlRestaurantChain.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
