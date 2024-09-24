using GD.Api.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : CustomController
{
    [HttpGet("test")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public IResult Test()
    {
        return Results.Json(User.Claims.Select(c => new { c.Type, c.Value }));
    }
}
