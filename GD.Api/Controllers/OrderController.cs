using GD.Api.Controllers.Base;
using GD.Api.DB;
using GD.Api.DB.Models;
using GD.Shared.Common;
using GD.Shared.Request;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : CustomController
{
    private readonly AppDbContext _appDbContext;
    private readonly UserManager<GDUser> _userManager;

    public OrderController(AppDbContext appDbContext, UserManager<GDUser> userManager)
    {
        _appDbContext = appDbContext;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "client")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest orderRequest)
    {
        var product = await _appDbContext.Products.FirstOrDefaultAsync(p => p.Id == orderRequest.ProductId);
        if (product is null) return BadRequest("товар не найден");
        
        if (orderRequest.PayMethod.ToLower() == "online")
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == ContextUserId);
            if (user!.Balance < orderRequest.Amount * product.Price) return BadRequest("недостаточно средств");
        }
        
        var order = orderRequest.Adapt<Order>();
        order.CreatedAt = DateTime.UtcNow;
        order.Status = "Waiting";
        order.TotalPrice = product.Price * orderRequest.Amount;
        order.ClientId = ContextUserId;
        
        
        await _appDbContext.Orders.AddAsync(order);
        await _appDbContext.SaveChangesAsync();

        return Ok(order);
    }
    
    [HttpPost("take")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "courier")]
    public async Task<IActionResult> TakeOrder([FromQuery] Guid id)
    {
        var order = await _appDbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return BadRequest("заказ не найден");
        
        order.StartDeliveryAt = DateTime.UtcNow;
        order.Status = "In delivery";
        order.CourierId = ContextUserId;
        
        _appDbContext.Orders.Update(order);
        await _appDbContext.SaveChangesAsync();

        return Ok(order);
    }    
    
    [HttpPost("change/{id:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "courier")]
    public async Task<IActionResult> ChangeCourier([FromRoute] Guid id)
    {
        var order = await _appDbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return BadRequest("заказ не найден");

        var courier = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (courier is null) 
            return BadRequest("курьер не найден");
        if(!await _userManager.IsInRoleAsync(courier, GDUserRoles.Courier))
            return BadRequest("пользователь не является курьером");
        
        order.CourierId = id;
        
        _appDbContext.Orders.Update(order);
        await _appDbContext.SaveChangesAsync();

        return Ok(order);
    }

    [HttpPost("close/{id:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "courier")]
    public async Task<IActionResult> CloseOrder([FromRoute] Guid id)
    {
        var order = await _appDbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return BadRequest("заказ не найден");

        order.OrderClosedAt = DateTime.UtcNow;
        order.Status = "Delivered";

        if (order.PayMethod.ToLower() == "online")
        {
            var client = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == order.ClientId);
            client!.Balance -= order.TotalPrice;
        }
        
        _appDbContext.Orders.Update(order);
        await _appDbContext.SaveChangesAsync();

        return Ok(order);
    }
}
