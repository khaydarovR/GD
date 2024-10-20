using Microsoft.AspNetCore.SignalR;

namespace GD.Api.Hubs
{
    public class PosHub: Hub
    {
        public async Task SendPos(string user, double lat, double lon)
        {
            Console.WriteLine($"{user} {lat} {lon}");
        }
    }
}
