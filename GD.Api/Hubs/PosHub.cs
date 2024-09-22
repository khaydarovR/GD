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

        public async Task Send(string username, string message)
        {
            Console.WriteLine($"Send {username} {message} to all");

            await this.Clients.All.SendAsync("Receive", username, message);
        }
    }
}
