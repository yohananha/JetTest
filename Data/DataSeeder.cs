using JetTest.Models;
using JetTest.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace JetTest.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        if (await context.Customers.AnyAsync())
            return;

        // ── Categories ────────────────────────────────────────────────────────
        var catBurgers  = new Category { Name = "Burgers" };
        var catPizza    = new Category { Name = "Pizza & Pasta" };
        var catSushi    = new Category { Name = "Sushi & Rolls" };
        var catSalads   = new Category { Name = "Salads" };
        var catDesserts = new Category { Name = "Desserts" };
        var catDrinks   = new Category { Name = "Drinks" };
        context.Categories.AddRange(catBurgers, catPizza, catSushi, catSalads, catDesserts, catDrinks);

        // ── Restaurants ───────────────────────────────────────────────────────
        var r1 = new Restaurant { Name = "Shake Bros",     Address = "12 Rothschild Blvd, Tel Aviv",  Phone = "03-5551234", CuisineType = "American", IsActive = true, CreatedAt = Ago(120) };
        var r2 = new Restaurant { Name = "Napoli Express", Address = "8 Jaffa St, Jerusalem",         Phone = "02-5559876", CuisineType = "Italian",  IsActive = true, CreatedAt = Ago(90)  };
        var r3 = new Restaurant { Name = "Tokyo Garden",   Address = "45 Hanassi Ave, Haifa",         Phone = "04-5550011", CuisineType = "Japanese", IsActive = true, CreatedAt = Ago(60)  };
        var r4 = new Restaurant { Name = "Green Bowl",     Address = "3 Ibn Gabirol St, Tel Aviv",    Phone = "03-5557788", CuisineType = "Healthy",  IsActive = true, CreatedAt = Ago(75)  };
        var r5 = new Restaurant { Name = "Sweet Corner",   Address = "22 Herzl St, Ramat Gan",        Phone = "03-5553344", CuisineType = "Desserts", IsActive = true, CreatedAt = Ago(45)  };
        context.Restaurants.AddRange(r1, r2, r3, r4, r5);

        // ── Customers ─────────────────────────────────────────────────────────
        var c1 = new Customer { Name = "Lior Cohen",     Email = "lior.cohen@gmail.com",     Phone = "050-1112233", Address = "5 Dizengoff St, Tel Aviv",    CreatedAt = Ago(200) };
        var c2 = new Customer { Name = "Maya Levi",      Email = "maya.levi@outlook.com",    Phone = "052-3334455", Address = "17 Ben Yehuda St, Jerusalem", CreatedAt = Ago(180) };
        var c3 = new Customer { Name = "Nir Shapiro",    Email = "nir.shapiro@yahoo.com",    Phone = "054-5556677", Address = "9 HaCarmel St, Haifa",        CreatedAt = Ago(150) };
        var c4 = new Customer { Name = "Tal Mizrahi",    Email = "tal.mizrahi@gmail.com",    Phone = "058-7778899", Address = "33 Weizmann St, Ramat Gan",   CreatedAt = Ago(130) };
        var c5 = new Customer { Name = "Shira Katz",     Email = "shira.katz@gmail.com",     Phone = "050-9990011", Address = "2 HaHashmonaim St, Tel Aviv", CreatedAt = Ago(110) };
        var c6 = new Customer { Name = "Avi Ben-David",  Email = "avi.bendavid@walla.co.il", Phone = "052-1113355", Address = "14 Allenby St, Tel Aviv",     CreatedAt = Ago(95)  };
        var c7 = new Customer { Name = "Dana Peretz",    Email = "dana.peretz@gmail.com",    Phone = "054-2224466", Address = "6 Remez St, Petah Tikva",     CreatedAt = Ago(80)  };
        var c8 = new Customer { Name = "Yossi Goldberg", Email = "yossi.goldberg@gmail.com", Phone = "058-8880022", Address = "28 Nordau Blvd, Netanya",     CreatedAt = Ago(60)  };
        context.Customers.AddRange(c1, c2, c3, c4, c5, c6, c7, c8);

        // ── Delivery Drivers ──────────────────────────────────────────────────
        var d1 = new DeliveryDriver { Name = "Eitan Rosen",   Phone = "050-6660001", VehicleType = "Scooter",    Status = DriverStatus.Available, CreatedAt = Ago(100) };
        var d2 = new DeliveryDriver { Name = "Rotem Avraham", Phone = "052-6660002", VehicleType = "Motorcycle", Status = DriverStatus.Busy,      CreatedAt = Ago(90)  };
        var d3 = new DeliveryDriver { Name = "Hila Ofer",     Phone = "054-6660003", VehicleType = "Bicycle",    Status = DriverStatus.Available, CreatedAt = Ago(70)  };
        var d4 = new DeliveryDriver { Name = "Moti Segal",    Phone = "058-6660004", VehicleType = "Car",        Status = DriverStatus.Offline,   CreatedAt = Ago(50)  };
        context.DeliveryDrivers.AddRange(d1, d2, d3, d4);

        await context.SaveChangesAsync();

        // ── Dishes ────────────────────────────────────────────────────────────
        // Shake Bros (r1)
        var dSmashBurger  = Dish("Classic Smash Burger",  58m, catBurgers,  r1);
        var dBBQDouble    = Dish("BBQ Bacon Double",       72m, catBurgers,  r1);
        var dVeggieBurger = Dish("Veggie Mushroom Burger", 54m, catBurgers,  r1);
        var dLemonade     = Dish("Craft Lemonade",         18m, catDrinks,   r1);
        var dMilkshake    = Dish("Vanilla Milkshake",      26m, catDrinks,   r1);
        // Napoli Express (r2)
        var dMargherita   = Dish("Margherita Pizza",       62m, catPizza,    r2);
        var dProsciutto   = Dish("Prosciutto & Arugula",   74m, catPizza,    r2);
        var dCarbonara    = Dish("Spaghetti Carbonara",    68m, catPizza,    r2);
        var dArrabiata    = Dish("Penne Arrabiata",         58m, catPizza,    r2);
        var dPellegrino   = Dish("San Pellegrino",         14m, catDrinks,   r2);
        // Tokyo Garden (r3)
        var dSalmonNigiri = Dish("Salmon Nigiri (8 pcs)", 78m, catSushi,    r3);
        var dSpicyTuna    = Dish("Spicy Tuna Roll",        64m, catSushi,    r3);
        var dRainbowRoll  = Dish("Rainbow Roll (12 pcs)", 92m, catSushi,    r3);
        var dEdamame      = Dish("Edamame",                22m, catSushi,    r3);
        var dMisoSoup     = Dish("Miso Soup",              16m, catDrinks,   r3);
        // Green Bowl (r4)
        var dCaesar       = Dish("Caesar Salad",           52m, catSalads,   r4);
        var dGreekBowl    = Dish("Greek Bowl",             56m, catSalads,   r4);
        var dQuinoa       = Dish("Quinoa & Avocado",       62m, catSalads,   r4);
        var dJuice        = Dish("Cold-Pressed Juice",     22m, catDrinks,   r4);
        var dSparklingOff = Dish("Sparkling Water",        12m, catDrinks,   r4, available: false);
        // Sweet Corner (r5)
        var dMoltenCake   = Dish("Molten Chocolate Cake", 44m, catDesserts, r5);
        var dCheesecake   = Dish("NY Cheesecake Slice",   38m, catDesserts, r5);
        var dTiramisu     = Dish("Tiramisu",               42m, catDesserts, r5);
        var dEspresso     = Dish("Espresso",               14m, catDrinks,   r5);
        var dHotChoc      = Dish("Hot Chocolate",          18m, catDrinks,   r5);

        context.Dishes.AddRange(
            dSmashBurger, dBBQDouble, dVeggieBurger, dLemonade, dMilkshake,
            dMargherita, dProsciutto, dCarbonara, dArrabiata, dPellegrino,
            dSalmonNigiri, dSpicyTuna, dRainbowRoll, dEdamame, dMisoSoup,
            dCaesar, dGreekBowl, dQuinoa, dJuice, dSparklingOff,
            dMoltenCake, dCheesecake, dTiramisu, dEspresso, dHotChoc);

        await context.SaveChangesAsync();

        // ── Driver Status Histories ───────────────────────────────────────────
        context.DriverStatusHistories.AddRange(
            new DriverStatusHistory { DriverId = d1.Id, OldStatus = DriverStatus.Offline,   NewStatus = DriverStatus.Available, ChangedAt = Ago(99) },
            new DriverStatusHistory { DriverId = d1.Id, OldStatus = DriverStatus.Available, NewStatus = DriverStatus.Busy,      ChangedAt = Ago(5)  },
            new DriverStatusHistory { DriverId = d1.Id, OldStatus = DriverStatus.Busy,      NewStatus = DriverStatus.Available, ChangedAt = Ago(3)  },
            new DriverStatusHistory { DriverId = d2.Id, OldStatus = DriverStatus.Offline,   NewStatus = DriverStatus.Available, ChangedAt = Ago(89) },
            new DriverStatusHistory { DriverId = d2.Id, OldStatus = DriverStatus.Available, NewStatus = DriverStatus.Busy,      ChangedAt = Ago(1)  },
            new DriverStatusHistory { DriverId = d3.Id, OldStatus = DriverStatus.Offline,   NewStatus = DriverStatus.Available, ChangedAt = Ago(69) },
            new DriverStatusHistory { DriverId = d4.Id, OldStatus = DriverStatus.Offline,   NewStatus = DriverStatus.Available, ChangedAt = Ago(49) },
            new DriverStatusHistory { DriverId = d4.Id, OldStatus = DriverStatus.Available, NewStatus = DriverStatus.Busy,      ChangedAt = Ago(30) },
            new DriverStatusHistory { DriverId = d4.Id, OldStatus = DriverStatus.Busy,      NewStatus = DriverStatus.Offline,   ChangedAt = Ago(20) });

        // ── Orders ────────────────────────────────────────────────────────────
        AddOrder(context, c1, r1, d1, "5 Dizengoff St, Tel Aviv", null,
            OrderStatus.Delivered, Ago(30),
            History(OrderStatus.Placed, OrderStatus.Accepted,       Ago(30).AddMinutes(4)),
            History(OrderStatus.Accepted, OrderStatus.Preparing,    Ago(30).AddMinutes(16)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(30).AddMinutes(19)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,  Ago(30).AddMinutes(27)),
            History(OrderStatus.PickedUp, OrderStatus.Delivered,    Ago(30).AddMinutes(41)));
        AddItem(context, 1, dSmashBurger, 2);
        AddItem(context, 1, dMilkshake, 2);

        AddOrder(context, c2, r2, d2, "17 Ben Yehuda St, Jerusalem", "Extra cheese please",
            OrderStatus.Delivered, Ago(28),
            History(OrderStatus.Placed, OrderStatus.Accepted,       Ago(28).AddMinutes(6)),
            History(OrderStatus.Accepted, OrderStatus.Preparing,    Ago(28).AddMinutes(24)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(28).AddMinutes(28)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,  Ago(28).AddMinutes(38)),
            History(OrderStatus.PickedUp, OrderStatus.Delivered,    Ago(28).AddMinutes(58)));
        AddItem(context, 2, dMargherita, 1);
        AddItem(context, 2, dCarbonara, 1);
        AddItem(context, 2, dPellegrino, 2);

        AddOrder(context, c3, r3, d1, "9 HaCarmel St, Haifa", null,
            OrderStatus.Delivered, Ago(25),
            History(OrderStatus.Placed, OrderStatus.Accepted,       Ago(25).AddMinutes(5)),
            History(OrderStatus.Accepted, OrderStatus.Preparing,    Ago(25).AddMinutes(25)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(25).AddMinutes(30)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,  Ago(25).AddMinutes(42)),
            History(OrderStatus.PickedUp, OrderStatus.Delivered,    Ago(25).AddMinutes(60)));
        AddItem(context, 3, dSalmonNigiri, 2);
        AddItem(context, 3, dSpicyTuna, 1);
        AddItem(context, 3, dMisoSoup, 2);

        AddOrder(context, c4, r1, d3, "33 Weizmann St, Ramat Gan", "No onions",
            OrderStatus.Delivered, Ago(22),
            History(OrderStatus.Placed, OrderStatus.Accepted,       Ago(22).AddMinutes(3)),
            History(OrderStatus.Accepted, OrderStatus.Preparing,    Ago(22).AddMinutes(13)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(22).AddMinutes(16)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,  Ago(22).AddMinutes(23)),
            History(OrderStatus.PickedUp, OrderStatus.Delivered,    Ago(22).AddMinutes(35)));
        AddItem(context, 4, dBBQDouble, 1);
        AddItem(context, 4, dLemonade, 1);

        AddOrder(context, c5, r4, d1, "2 HaHashmonaim St, Tel Aviv", null,
            OrderStatus.Delivered, Ago(20),
            History(OrderStatus.Placed, OrderStatus.Accepted,       Ago(20).AddMinutes(4)),
            History(OrderStatus.Accepted, OrderStatus.Preparing,    Ago(20).AddMinutes(12)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(20).AddMinutes(14)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,  Ago(20).AddMinutes(20)),
            History(OrderStatus.PickedUp, OrderStatus.Delivered,    Ago(20).AddMinutes(30)));
        AddItem(context, 5, dCaesar, 1);
        AddItem(context, 5, dQuinoa, 1);
        AddItem(context, 5, dJuice, 2);

        AddOrder(context, c6, r2, d2, "14 Allenby St, Tel Aviv", "Ring the bell twice",
            OrderStatus.Delivered, Ago(18),
            History(OrderStatus.Placed, OrderStatus.Accepted,       Ago(18).AddMinutes(5)),
            History(OrderStatus.Accepted, OrderStatus.Preparing,    Ago(18).AddMinutes(21)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(18).AddMinutes(25)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,  Ago(18).AddMinutes(34)),
            History(OrderStatus.PickedUp, OrderStatus.Delivered,    Ago(18).AddMinutes(50)));
        AddItem(context, 6, dProsciutto, 1);
        AddItem(context, 6, dArrabiata, 1);

        AddOrder(context, c7, r5, d3, "6 Remez St, Petah Tikva", null,
            OrderStatus.Delivered, Ago(15),
            History(OrderStatus.Placed, OrderStatus.Accepted,       Ago(15).AddMinutes(3)),
            History(OrderStatus.Accepted, OrderStatus.Preparing,    Ago(15).AddMinutes(13)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(15).AddMinutes(15)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,  Ago(15).AddMinutes(23)),
            History(OrderStatus.PickedUp, OrderStatus.Delivered,    Ago(15).AddMinutes(35)));
        AddItem(context, 7, dMoltenCake, 2);
        AddItem(context, 7, dTiramisu, 1);
        AddItem(context, 7, dEspresso, 2);

        AddOrder(context, c8, r3, d1, "28 Nordau Blvd, Netanya", "Leave at door",
            OrderStatus.Delivered, Ago(12),
            History(OrderStatus.Placed, OrderStatus.Accepted,       Ago(12).AddMinutes(6)),
            History(OrderStatus.Accepted, OrderStatus.Preparing,    Ago(12).AddMinutes(28)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(12).AddMinutes(33)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,  Ago(12).AddMinutes(47)),
            History(OrderStatus.PickedUp, OrderStatus.Delivered,    Ago(12).AddMinutes(67)));
        AddItem(context, 8, dRainbowRoll, 1);
        AddItem(context, 8, dEdamame, 2);
        AddItem(context, 8, dMisoSoup, 1);

        AddOrder(context, c1, r2, d2, "5 Dizengoff St, Tel Aviv", null,
            OrderStatus.Delivered, Ago(10),
            History(OrderStatus.Placed, OrderStatus.Accepted,       Ago(10).AddMinutes(4)),
            History(OrderStatus.Accepted, OrderStatus.Preparing,    Ago(10).AddMinutes(18)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(10).AddMinutes(21)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,  Ago(10).AddMinutes(29)),
            History(OrderStatus.PickedUp, OrderStatus.Delivered,    Ago(10).AddMinutes(44)));
        AddItem(context, 9, dMargherita, 2);
        AddItem(context, 9, dPellegrino, 1);

        AddOrder(context, c3, r1, d3, "9 HaCarmel St, Haifa", "Extra sauce",
            OrderStatus.Delivered, Ago(8),
            History(OrderStatus.Placed, OrderStatus.Accepted,       Ago(8).AddMinutes(3)),
            History(OrderStatus.Accepted, OrderStatus.Preparing,    Ago(8).AddMinutes(14)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(8).AddMinutes(17)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,  Ago(8).AddMinutes(24)),
            History(OrderStatus.PickedUp, OrderStatus.Delivered,    Ago(8).AddMinutes(37)));
        AddItem(context, 10, dVeggieBurger, 1);
        AddItem(context, 10, dLemonade, 2);

        // ── In-progress: Accepted ─────────────────────────────────────────────
        AddOrder(context, c4, r4, null, "33 Weizmann St, Ramat Gan", "Dressing on the side",
            OrderStatus.Accepted, Ago(0, 25),
            History(OrderStatus.Placed, OrderStatus.Accepted, Ago(0, 18)));
        AddItem(context, 11, dGreekBowl, 1);

        // ── In-progress: Preparing ────────────────────────────────────────────
        AddOrder(context, c5, r3, null, "2 HaHashmonaim St, Tel Aviv", null,
            OrderStatus.Preparing, Ago(0, 40),
            History(OrderStatus.Placed,   OrderStatus.Accepted,  Ago(0, 32)),
            History(OrderStatus.Accepted, OrderStatus.Preparing, Ago(0, 20)));
        AddItem(context, 12, dSpicyTuna, 2);
        AddItem(context, 12, dEdamame, 1);

        // ── In-progress: ReadyForPickup ───────────────────────────────────────
        AddOrder(context, c6, r1, d2, "14 Allenby St, Tel Aviv", null,
            OrderStatus.ReadyForPickup, Ago(0, 50),
            History(OrderStatus.Placed,    OrderStatus.Accepted,       Ago(0, 44)),
            History(OrderStatus.Accepted,  OrderStatus.Preparing,      Ago(0, 30)),
            History(OrderStatus.Preparing, OrderStatus.ReadyForPickup, Ago(0, 8)));
        AddItem(context, 13, dSmashBurger, 3);

        // ── In-progress: PickedUp ─────────────────────────────────────────────
        AddOrder(context, c7, r5, d2, "6 Remez St, Petah Tikva", null,
            OrderStatus.PickedUp, Ago(0, 60),
            History(OrderStatus.Placed,         OrderStatus.Accepted,       Ago(0, 52)),
            History(OrderStatus.Accepted,       OrderStatus.Preparing,      Ago(0, 35)),
            History(OrderStatus.Preparing,      OrderStatus.ReadyForPickup, Ago(0, 12)),
            History(OrderStatus.ReadyForPickup, OrderStatus.PickedUp,       Ago(0, 5)));
        AddItem(context, 14, dCheesecake, 2);
        AddItem(context, 14, dHotChoc, 2);

        // ── Cancelled ─────────────────────────────────────────────────────────
        AddOrder(context, c8, r2, null, "28 Nordau Blvd, Netanya", "Changed my mind",
            OrderStatus.Cancelled, Ago(5),
            History(OrderStatus.Placed,   OrderStatus.Accepted,   Ago(4, 55), "Confirmed by restaurant"),
            History(OrderStatus.Accepted, OrderStatus.Cancelled,  Ago(4, 50), "Customer requested cancellation"));
        AddItem(context, 15, dProsciutto, 1);

        await context.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static int _orderSeq = 0;
    private static int _itemSeq  = 0;

    private static void AddOrder(
        AppDbContext ctx,
        Customer customer, Restaurant restaurant, DeliveryDriver? driver,
        string address, string? notes,
        OrderStatus status, DateTime createdAt,
        params OrderStatusHistory[] history)
    {
        _orderSeq++;
        var updatedAt = history.Length > 0 ? history[^1].ChangedAt : createdAt;
        var order = new Order
        {
            Id = _orderSeq,
            CustomerId = customer.Id,
            RestaurantId = restaurant.Id,
            DeliveryDriverId = driver?.Id,
            Status = status,
            DeliveryAddress = address,
            Notes = notes,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
        foreach (var h in history)
        {
            h.OrderId = _orderSeq;
            order.StatusHistory.Add(h);
        }
        ctx.Orders.Add(order);
    }

    private static void AddItem(AppDbContext ctx, int orderId, Dish dish, int qty)
    {
        _itemSeq++;
        ctx.OrderItems.Add(new OrderItem
        {
            OrderId   = orderId,
            DishId    = dish.Id,
            Quantity  = qty,
            UnitPrice = dish.Price
        });
    }

    private static OrderStatusHistory History(
        OrderStatus from, OrderStatus to, DateTime changedAt, string? notes = null) =>
        new() { OldStatus = from, NewStatus = to, ChangedAt = changedAt, Notes = notes };

    private static Dish Dish(string name, decimal price, Category category, Restaurant restaurant, bool available = true) =>
        new() { Name = name, Price = price, Category = category, CategoryId = category.Id, Restaurant = restaurant, RestaurantId = restaurant.Id, IsAvailable = available };

    private static DateTime Ago(int days, int minutes = 0) =>
        DateTime.UtcNow.AddDays(-days).AddMinutes(-minutes);
}
