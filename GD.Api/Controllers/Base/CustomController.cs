using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GD.Api.Controllers.Base
{
    public class CustomController: ControllerBase
    {
        internal Guid ContextUserId
        {
            get
            {
                return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            }
        }
        internal string? CurrentIp => HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }
}
