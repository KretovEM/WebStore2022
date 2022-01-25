using Microsoft.AspNetCore.Mvc;
using WebStore.Domain.DTO;
using WebStore.Interfaces.Services;

namespace WebStore.WebAPI.Controllers;

[Route("api/orders")]
[ApiController]
public class OrdersApiController : ControllerBase
{
    private readonly IOrderService _orderSevices;

    public OrdersApiController(IOrderService orderSevices)
    {
        _orderSevices = orderSevices;
    }

    [HttpGet("user/{UserName}")]
    public async Task<IActionResult> GetUserOrders(string UserName)
    {
        var orders = await _orderSevices.GetUserOrdersAsync(UserName);
        return Ok(orders.ToDTO());
    }

    [HttpGet("{Id}")]
    public async Task<IActionResult> GetOrderById(int Id)
    {
        var order = await _orderSevices.GetOrderByIdAsync(Id);
        if (order is null)
            return NotFound();

        return Ok(order.ToDTO());
    }

    [HttpPost("{UserName}")]
    public async Task<IActionResult> CreateOrder(string UserName, [FromBody] CreateOrderDTO Model)
    {
        var order = await _orderSevices.CreateOrderAsync(UserName, Model.Items.ToCartView(), Model.Order);
        return CreatedAtAction(nameof(GetOrderById), new { order.Id }, order.ToDTO());
    }
}
