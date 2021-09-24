using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RevitService.SignalR.Hubs
{
    public class RSOHub : Hub
    {
        public async Task Create(string authorization, string urn)
        {
            await Task.Delay(100);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
