using Leagify.AuctionDrafter.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Leagify.AuctionDrafter.Server
{
    public class AuctionHub : Hub
    {
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task UserJoined(string groupName, User user)
        {
            await Clients.Group(groupName).SendAsync("UserJoined", user);
        }

        public async Task RoleAssigned(string groupName, int userId, Role role)
        {
            await Clients.Group(groupName).SendAsync("RoleAssigned", userId, role);
        }
    }
}
