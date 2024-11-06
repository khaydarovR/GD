using GD.Api.DB;
using GD.Shared.Response;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GD.Api.Hubs
{
    public class PosHub: Hub
    {
        private readonly AppDbContext context;

        public PosHub(AppDbContext context)
        {
            this.context = context;
        }
        public async Task SendPos(HubPosInfo info)
        {
            Console.WriteLine($"================ {info.UserId} {info.TargetPosLati} {info.TargetPosLong}");

            var u = await context.Users.FirstOrDefaultAsync(u => u.Id == info.UserId);
            await this.Clients.All.SendAsync("SharePos", info);

            if (u == null) return;

            u.PosLati = info.TargetPosLati;
            u.PosLong = info.TargetPosLong;

            context.Users.Update(u);
            await context.SaveChangesAsync();

        }
    }
}
