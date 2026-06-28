using JetTest.BL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JetTest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService service, ILogger<ReportsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET /api/reports/top-dishes?restaurantId=&topN=5
    [HttpGet("top-dishes")]
    public async Task<IActionResult> GetTopDishes([FromQuery] int? restaurantId, [FromQuery] int topN = 5)
    {
        try
        {
            return Ok(await _service.GetTopDishesPerCategoryAsync(restaurantId, topN));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating top-dishes report");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // GET /api/reports/delivery-efficiency?restaurantId=
    [HttpGet("delivery-efficiency")]
    public async Task<IActionResult> GetDeliveryEfficiency([FromQuery] int? restaurantId)
    {
        try
        {
            return Ok(await _service.GetDeliveryEfficiencyAsync(restaurantId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating delivery-efficiency report");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
