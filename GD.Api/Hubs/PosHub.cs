using Microsoft.AspNetCore.SignalR;

namespace GD.Api.Hubs
{
    public class PosHub: Hub
    {
        public async Task SendPos(string user, string pos)
        {
            Console.WriteLine($"Send {user} {pos} to all");
            await Clients.All.SendAsync("GetPos", user, pos);
        }
    }
}
