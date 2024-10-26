using GD.Shared.Response;
using Microsoft.AspNetCore.SignalR;

namespace GD.Api.Hubs
{
    public class PosHub: Hub
    {
        public async Task SendPos(HubPosInfo info)
        {
            Console.WriteLine($"================ {info.UserId} {info.TargetPosLati} {info.TargetPosLong}");

            await this.Clients.All.SendAsync("SharePos", info);
        }
    }
}
