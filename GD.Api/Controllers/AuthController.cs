using GD.Api.Controllers.Base;
using GD.Shared.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static GD.Api.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GD.Shared.Response;
using GD.Api.DB;
using Microsoft.AspNetCore.Identity;
using GD.Api.DB.Models;
using GD.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace GD.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AppDbContext context, UserManager<GDUser> um, ILogger<AuthController> l): CustomController
    {
        
        [HttpGet(nameof(SignIn))]
        public async Task<ActionResult<SignInResponse>> SignIn([FromQuery] SignInRequest dto)
        {
            l.LogInformation($"Login request {dto.Email} {dto.Pwd}");
            var u = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            var res = await CreateIfNull(u, dto);

            if (res.IsSuccess == false)
            {
                return BadRequest(res.ErrorList);
            }

            u = await um.FindByEmailAsync(dto.Email);
            var uClaims = await um.GetClaimsAsync(u);

            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: uClaims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromDays(360)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            var rsp = new SignInResponse() { Jwt = token };

            return Ok(rsp);
        }

        private static List<Claim> CreateClaims(GDUser? u)
        {
            return new List<Claim>
            {
                new Claim(GDUserClaimTypes.Email, u.Email!),
                new Claim(GDUserClaimTypes.UserName, u.UserName.ToString()),
                new Claim(GDUserClaimTypes.Id, u.Id.ToString()),
                new Claim(GDUserClaimTypes.Roles, GDUserRoles.ClientRole),
            };
        }

        private async Task<Res<bool>> CreateIfNull(GDUser? u, SignInRequest dto)
        {
            if (u != null) { return new Res<bool>(true); }

            var newuser = new GDUser()
            {
                Email = dto.Email,
                UserName = dto.Email,
            };

            var res = await um.CreateAsync(newuser, dto.Pwd);
            if (res.Succeeded)
            {
                u = await um.FindByEmailAsync(dto.Email);
                var resClaim = await um.AddClaimsAsync(u, CreateClaims(u));
                if (resClaim.Succeeded)
                {
                    return new Res<bool>(true);
                }
                else
                {
                    return new Res<bool>(resClaim.Errors.Select(e => e.Description).ToArray());
                }
            }
            else
            {
                return new Res<bool>(res.Errors.Select(e => e.Description).ToArray());
            }
        }
    }
}
