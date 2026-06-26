using JetTest.BL.Interfaces;
using JetTest.BL.Services;
using JetTest.DAL.Interfaces;
using JetTest.DAL.Repositories;
using JetTest.Data;
using Microsoft.EntityFrameworkCore;

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

// BL
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IDeliveryDriverService, DeliveryDriverService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
