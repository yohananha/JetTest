using JetTest.BL.Exceptions;
using JetTest.BL.Interfaces;
using JetTest.DAL.Interfaces;
using JetTest.Data;
using JetTest.DTOs.Orders;
using JetTest.Models;
using JetTest.Models.Enums;

namespace JetTest.BL.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IRestaurantRepository _restaurantRepo;
    private readonly IDishRepository _dishRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repo,
        ICustomerRepository customerRepo,
        IRestaurantRepository restaurantRepo,
        IDishRepository dishRepo,
        AppDbContext context,
        ILogger<OrderService> logger)
    {
        _repo = repo;
        _customerRepo = customerRepo;
        _restaurantRepo = restaurantRepo;
        _dishRepo = dishRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllAsync()
    {
        var orders = await _repo.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} orders", orders.Count());
        return orders.Select(MapToDto);
    }

    public async Task<OrderResponseDto> GetByIdAsync(int id)
    {
        var order = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Order {id} not found");
        return MapToDto(order);
    }

    public async Task<OrderResponseDto> CreateAsync(CreateOrderDto dto)
    {
        _ = await _customerRepo.GetByIdAsync(dto.CustomerId)
            ?? throw new NotFoundException($"Customer {dto.CustomerId} not found");

        _ = await _restaurantRepo.GetByIdAsync(dto.RestaurantId)
            ?? throw new NotFoundException($"Restaurant {dto.RestaurantId} not found");

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            RestaurantId = dto.RestaurantId,
            DeliveryAddress = dto.DeliveryAddress,
            Notes = dto.Notes
        };

        foreach (var itemDto in dto.Items)
        {
            var dish = await _dishRepo.GetByIdAsync(itemDto.DishId)
                ?? throw new NotFoundException($"Dish {itemDto.DishId} not found");

            order.Items.Add(new OrderItem
            {
                DishId = dish.Id,
                Dish = dish,
                Quantity = itemDto.Quantity,
                UnitPrice = dish.Price // snapshot price at order time
            });
        }

        var created = await _repo.CreateAsync(order);
        _logger.LogInformation("Created order {Id} for customer {CustomerId} with {ItemCount} item(s)",
            created.Id, created.CustomerId, created.Items.Count);
        return MapToDto(created);
    }

    public async Task<OrderResponseDto> UpdateAsync(int id, UpdateOrderDto dto)
    {
        var order = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Order {id} not found");

        if (dto.DeliveryDriverId.HasValue)
            order.DeliveryDriverId = dto.DeliveryDriverId;

        if (dto.DeliveryAddress is not null)
            order.DeliveryAddress = dto.DeliveryAddress;

        if (dto.Notes is not null)
            order.Notes = dto.Notes;

        order.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(order);
        _logger.LogInformation("Updated order {Id}", id);
        return MapToDto(order);
    }

    public async Task DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Order {id} not found");

        await _repo.DeleteAsync(id);
        _logger.LogInformation("Deleted order {Id}", id);
    }

    public async Task<OrderResponseDto> UpdateStatusAsync(int id, OrderStatus newStatus, string? notes = null)
    {
        var order = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Order {id} not found");

        if (order.Status == newStatus)
            throw new InvalidOperationException($"Order {id} is already in status {newStatus}");

        var history = new OrderStatusHistory
        {
            OrderId = id,
            OldStatus = order.Status,
            NewStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            Notes = notes
        };

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.OrderStatusHistories.AddAsync(history);
        await _repo.UpdateAsync(order);

        _logger.LogInformation("Order {Id} status changed from {Old} to {New}", id, history.OldStatus, newStatus);
        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderStatusHistory>> GetHistoryAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Order {id} not found");

        return await _repo.GetOrderHistoryAsync(id);
    }

    private static OrderResponseDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        CustomerId = o.CustomerId,
        RestaurantId = o.RestaurantId,
        DeliveryDriverId = o.DeliveryDriverId,
        Status = o.Status,
        DeliveryAddress = o.DeliveryAddress,
        Notes = o.Notes,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
        Items = o.Items.Select(i => new OrderItemResponseDto
        {
            DishId = i.DishId,
            DishName = i.Dish?.Name ?? string.Empty,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList()
    };
}
