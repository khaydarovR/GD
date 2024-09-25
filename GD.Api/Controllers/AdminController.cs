using GD.Api.Controllers.Base;
using GD.Api.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer", Policy = "admin")]
public class AdminController : CustomController
{
    private readonly AppDbContext _appDbContext;

    public AdminController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetOrdersDashboard()
    {
        return Ok(_appDbContext.Orders.Where(o => o.Status == "Waiting" || o.Status == "In delivery"));
    }
}
