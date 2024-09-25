using GD.Api.Controllers.Base;
using GD.Api.DB.Models;
using GD.Shared.Common;
using GD.Shared.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourierController : CustomController
{
    private readonly UserManager<GDUser> _userManager;

    public CourierController(UserManager<GDUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("new")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "admin")]
    public async Task<IActionResult> MakeCourier([FromBody] NewCourierRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.ClientId.ToString());
        if (user is null) return BadRequest("пользователь не найден");

        var isClient = await _userManager.IsInRoleAsync(user, "Client");
        if (!isClient) return BadRequest("роль пользователя либо админ, либо курьер");
        
        await _userManager.RemoveFromRoleAsync(user, GDUserRoles.Client);
        await _userManager.AddToRoleAsync(user, GDUserRoles.Courier);
        
        return Ok();
    }
}
