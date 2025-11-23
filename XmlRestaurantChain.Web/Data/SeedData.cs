using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XmlRestaurantChain.Web.Models;

namespace XmlRestaurantChain.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        var roles = new[] { "Admin", "Manager", "Staff" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        async Task<ApplicationUser> EnsureUserAsync(string email, string displayName, string role, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = displayName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var message = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Không tạo được user seed: {message}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }

            return user;
        }

        var adminUser = await EnsureUserAsync("admin@xmlchain.local", "Administrator", "Admin", "Admin!2345");
        var managerUser = await EnsureUserAsync("manager@xmlchain.local", "General Manager", "Manager", "Manager!2345");
        var staffUser = await EnsureUserAsync("staff@xmlchain.local", "Dining Staff", "Staff", "Staff!2345");

        if (await context.Restaurants.AnyAsync())
        {
            return;
        }

        var mainRestaurant = new Restaurant
        {
            Name = "CNX Luxury Dining",
            Address = "123 Heritage Blvd",
            City = "Da Nang",
            Country = "Vietnam",
            Phone = "0901 234 567",
            Email = "contact@cnx.vn",
            Description = "Chuỗi nhà hàng sang trọng phong cách Đông Dương.",
            ThemeColor = "#f59e0b"
        };

        var skyRestaurant = new Restaurant
        {
            Name = "CNX Sky Lounge",
            Address = "88 Riverside Avenue",
            City = "Da Nang",
            Country = "Vietnam",
            Phone = "0901 999 888",
            Email = "sky@cnx.vn",
            Description = "Nhà hàng rooftop, cocktail & fine dining.",
            ThemeColor = "#06b6d4"
        };

        context.Restaurants.AddRange(mainRestaurant, skyRestaurant);
        await context.SaveChangesAsync();

        var suppliers = new[]
        {
            new Supplier { Name = "FreshFarm Co", ContactEmail = "hello@freshfarm.vn", ContactPhone = "028 1234 7777", Address = "KCN Hoa Khanh" },
            new Supplier { Name = "SeaToTable", ContactEmail = "sales@seatotable.vn", ContactPhone = "0236 555 999", Address = "Cảng Tiên Sa" },
            new Supplier { Name = "GourmetProvisions", ContactEmail = "cs@gourmetpro.vn", ContactPhone = "028 5555 1212", Address = "Thảo Điền, TP.HCM" }
        };

        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();

        var categories = new[]
        {
            new MenuCategory { Name = "Signature", Description = "Món đặc trưng", Restaurant = mainRestaurant },
            new MenuCategory { Name = "Tasting Menu", Description = "Set menu", Restaurant = mainRestaurant },
            new MenuCategory { Name = "Cocktails", Description = "Signature cocktails", Restaurant = skyRestaurant },
            new MenuCategory { Name = "Dessert", Description = "Tráng miệng", Restaurant = skyRestaurant }
        };

        context.MenuCategories.AddRange(categories);
        await context.SaveChangesAsync();

        var menuItems = new[]
        {
            new MenuItem { Name = "Wagyu Phở", Description = "Phở bò Wagyu, truffle oil", Price = 520_000, IsAvailable = true, MenuCategory = categories[0] },
            new MenuItem { Name = "Lobster Bánh Mì", Description = "Bánh mì tôm hùm bơ tỏi", Price = 450_000, IsAvailable = true, MenuCategory = categories[0] },
            new MenuItem { Name = "Indochine Tasting", Description = "7-course tasting", Price = 1_490_000, IsAvailable = true, MenuCategory = categories[1] },
            new MenuItem { Name = "Sky Negroni", Description = "Barrel-aged negroni", Price = 210_000, IsAvailable = true, MenuCategory = categories[2] },
            new MenuItem { Name = "Yuzu Martini", Description = "Gin, yuzu cordial, jasmine", Price = 195_000, IsAvailable = true, MenuCategory = categories[2] },
            new MenuItem { Name = "Coconut Panna Cotta", Description = "Panna cotta dừa, xoài", Price = 165_000, IsAvailable = true, MenuCategory = categories[3] }
        };

        context.MenuItems.AddRange(menuItems);
        await context.SaveChangesAsync();

        var tables = new[]
        {
            new DiningTable { Name = "Sảnh A1", Capacity = 4, Status = TableStatus.Available, Restaurant = mainRestaurant },
            new DiningTable { Name = "Sảnh A2", Capacity = 6, Status = TableStatus.Reserved, Restaurant = mainRestaurant },
            new DiningTable { Name = "Private 01", Capacity = 10, Status = TableStatus.Available, Restaurant = mainRestaurant },
            new DiningTable { Name = "Sky Window", Capacity = 2, Status = TableStatus.Occupied, Restaurant = skyRestaurant },
            new DiningTable { Name = "Sky Lounge", Capacity = 8, Status = TableStatus.Available, Restaurant = skyRestaurant }
        };

        context.DiningTables.AddRange(tables);
        await context.SaveChangesAsync();

        var reservations = new[]
        {
            new Reservation
            {
                CustomerName = "Nguyễn Thanh Mạnh",
                CustomerPhone = "0912 888 555",
                PartySize = 4,
                ReservedAt = DateTime.UtcNow.AddHours(6),
                Notes = "Kỷ niệm 5 năm, cần hoa tươi",
                Status = ReservationStatus.Confirmed,
                Restaurant = mainRestaurant,
                DiningTable = tables[0]
            },
            new Reservation
            {
                CustomerName = "Trần Gia Hân",
                CustomerPhone = "0987 222 111",
                PartySize = 2,
                ReservedAt = DateTime.UtcNow.AddHours(3),
                Notes = "Bàn view sông Hàn",
                Status = ReservationStatus.Pending,
                Restaurant = skyRestaurant,
                DiningTable = tables[3]
            }
        };

        context.Reservations.AddRange(reservations);
        await context.SaveChangesAsync();

        var inventoryItems = new[]
        {
            new InventoryItem { Name = "Tenderloin bò Wagyu A5", Unit = "kg", Quantity = 12, ReorderLevel = 5, Supplier = suppliers[0], Restaurant = mainRestaurant, Notes = "Giữ 2kg cho tiệc tối" },
            new InventoryItem { Name = "Tôm hùm Canada", Unit = "kg", Quantity = 18, ReorderLevel = 8, Supplier = suppliers[1], Restaurant = mainRestaurant },
            new InventoryItem { Name = "Gin craft", Unit = "chai", Quantity = 30, ReorderLevel = 10, Supplier = suppliers[2], Restaurant = skyRestaurant },
            new InventoryItem { Name = "Rượu vang đỏ", Unit = "chai", Quantity = 45, ReorderLevel = 15, Supplier = suppliers[2], Restaurant = mainRestaurant }
        };

        context.InventoryItems.AddRange(inventoryItems);
        await context.SaveChangesAsync();

        var orders = new[]
        {
            new Order
            {
                Restaurant = mainRestaurant,
                DiningTable = tables[2],
                Type = OrderType.DineIn,
                Status = OrderStatus.InProgress,
                Notes = "Bàn VIP, phục vụ nhanh",
                CreatedBy = managerUser
            },
            new Order
            {
                Restaurant = skyRestaurant,
                DiningTable = tables[3],
                Type = OrderType.DineIn,
                Status = OrderStatus.New,
                Notes = "Khách dùng cocktail flight",
                CreatedBy = staffUser
            }
        };

        var orderItems = new[]
        {
            new OrderItem { Order = orders[0], MenuItem = menuItems[0], Quantity = 2, UnitPrice = menuItems[0].Price, LineTotal = menuItems[0].Price * 2 },
            new OrderItem { Order = orders[0], MenuItem = menuItems[5], Quantity = 2, UnitPrice = menuItems[5].Price, LineTotal = menuItems[5].Price * 2 },
            new OrderItem { Order = orders[1], MenuItem = menuItems[3], Quantity = 2, UnitPrice = menuItems[3].Price, LineTotal = menuItems[3].Price * 2 },
            new OrderItem { Order = orders[1], MenuItem = menuItems[4], Quantity = 1, UnitPrice = menuItems[4].Price, LineTotal = menuItems[4].Price }
        };

        orders[0].Total = orderItems.Where(x => x.Order == orders[0]).Sum(i => i.LineTotal);
        orders[1].Total = orderItems.Where(x => x.Order == orders[1]).Sum(i => i.LineTotal);

        context.Orders.AddRange(orders);
        context.OrderItems.AddRange(orderItems);

        var staffProfiles = new[]
        {
            new StaffProfile { User = managerUser, Restaurant = mainRestaurant, Position = "General Manager", Active = true, HiredDate = DateTime.UtcNow.AddMonths(-6) },
            new StaffProfile { User = staffUser, Restaurant = mainRestaurant, Position = "Head Waiter", Active = true, HiredDate = DateTime.UtcNow.AddMonths(-3) }
        };

        context.StaffProfiles.AddRange(staffProfiles);

        var loyaltyMembers = new[]
        {
            new LoyaltyMember { CustomerName = "Lê Minh Châu", Phone = "0911222333", Email = "chau@example.com", Points = 180, Tier = "Gold", Restaurant = mainRestaurant },
            new LoyaltyMember { CustomerName = "Hoàng Đức", Phone = "0933444555", Email = "duc@example.com", Points = 95, Tier = "Silver", Restaurant = skyRestaurant }
        };

        context.LoyaltyMembers.AddRange(loyaltyMembers);

        await context.SaveChangesAsync();
    }
}
