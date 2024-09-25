using GD.Api.Controllers.Base;
using GD.Api.DB;
using GD.Api.DB.Models;
using GD.Shared.Request;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer", Policy = "client")]
public class ClientController : CustomController
{
    private readonly AppDbContext _appDbContext;

    public ClientController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    [HttpPost("balance/add")]
    public async Task<IActionResult> AddToBalance(AddToBalanceRequest request)
    {
        var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == ContextUserId);
        if (user is null) return BadRequest("пользователь не найден");
        
        user.Balance += request.Amount;
        _appDbContext.Update(user);
        await _appDbContext.SaveChangesAsync();
        return Ok(user);
    }

    [HttpPost("balance/location")]
    public async Task<IActionResult> SetLocation(LocationRequest request)
    {
        var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == ContextUserId);
        if (user is null) return BadRequest("пользователь не найден");

        user.Address = request.Address;
        user.PosLati = request.PosLati;
        user.PosLong = request.PosLong;

        _appDbContext.Update(user);
        await _appDbContext.SaveChangesAsync();
        return Ok(user);
    }

}
