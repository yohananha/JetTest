using JetTest.BL.Interfaces;
using JetTest.BL.Services;
using JetTest.DAL.Interfaces;
using JetTest.DAL.Repositories;
using JetTest.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DAL
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IDeliveryDriverRepository, DeliveryDriverRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IDishRepository, DishRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// BL
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IDeliveryDriverService, DeliveryDriverService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    try
    {
        await DataSeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Data seeder skipped or failed — continuing startup.");
    }
}

app.Run();
