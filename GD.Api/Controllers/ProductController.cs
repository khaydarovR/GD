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
public class ProductController : CustomController
{
    private readonly AppDbContext _appDbContext;

    public ProductController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "admin")]
    public async Task<IActionResult> AddProduct([FromBody] ProductRequest productRequest)
    {
        var product = productRequest.Adapt<Product>();
        _appDbContext.Products.Add(product);
        await _appDbContext.SaveChangesAsync();
        return Ok(product);
    }
    
    [HttpDelete]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "admin")]
    public async Task<IActionResult> DeleteProduct([FromBody] DeleteRequest request)
    {
        var product = _appDbContext.Products.FirstOrDefault(p => p.Id == request.Id);
        if (product is null) return BadRequest("товар не найден");
        
        _appDbContext.Products.Remove(product);
        await _appDbContext.SaveChangesAsync();
        
        return Ok(request.Id);
    }

    [HttpGet]
    public IActionResult GetAllProducts()
    {
        return Ok(_appDbContext.Products.Include(p => p.Feedbacks));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById([FromRoute] Guid id)
    {
        var product = await _appDbContext.Products.Include(p => p.Feedbacks).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return BadRequest("товар не найден");

        return Ok(product);
    }
}
