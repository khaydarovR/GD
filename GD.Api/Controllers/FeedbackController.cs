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
public class FeedbackController : CustomController
{
    private readonly AppDbContext _appDbContext;

    public FeedbackController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    [HttpPost("feedback")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "client")]
    public async Task<IActionResult> NewFeedback([FromBody] FeedbackRequest request)
    {
        if(!await _appDbContext.Products.AnyAsync(p => p.Id == request.ProductId))
            return BadRequest("товар не найден");
        
        if(!await _appDbContext.Users.AnyAsync(u => u.Id == request.ClientId))
            return BadRequest("пользователь не найден");
        
        var feedback = request.Adapt<Feedback>();
        feedback.CreatedAt = DateTime.UtcNow;
        await _appDbContext.Feedbacks.AddAsync(feedback);
        return Ok(feedback);
    }
    
    [HttpDelete("feedback/{id:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "admin")]
    public async Task<IActionResult> RemoveFeedback([FromRoute] Guid id)
    {
        var feedback = await _appDbContext.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);
        if (feedback is null) return BadRequest("отзыв не найден");

        _appDbContext.Feedbacks.Remove(feedback);
        await _appDbContext.SaveChangesAsync();
        
        return Ok(id);
    }
}
