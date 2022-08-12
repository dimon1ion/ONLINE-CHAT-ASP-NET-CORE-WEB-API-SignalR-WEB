using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Data;
using MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Models;

namespace MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private UserContext _userContext;
        public ChatHub(UserContext userContext)
        {
            this._userContext = userContext;
        }

        public async Task SendAllUsers(string message)
        {
            string? name = GetNameFromClaims();
            if (name != null)
            {
                bool toMe = true;
                await Clients.AllExcept(Context.ConnectionId).SendAsync("AllReceive", name, message, !toMe);
                await Clients.Caller.SendAsync("AllReceive", "Me", message, toMe);
            }
        }

        public async Task NotifyAsync(string message)
        {
            await Clients.AllExcept(Context.ConnectionId).SendAsync("Notify", message, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        }

        public async Task Connect(string name, bool connected)
        {
            string id = Context.ConnectionId;
            await Clients.AllExcept(Context.ConnectionId).SendAsync("UserConnect", name, id, connected);
        }

        public async Task UsersOnline(string name)
        {
            User[] users = _userContext.Users.Where(u => u.Status == true && u.ConnectionId != null && u.Name != name).ToArray();
            await Clients.Caller.SendAsync("UsersOnline", users);
        }

        public override async Task OnConnectedAsync()
        {
            //await Clients.Caller.SendAsync("OnConnected");
            string? name = GetNameFromClaims();
            if (name != null)
            {
                User? user = _userContext.Users.FirstOrDefault(u => u.Name == name);
                if (user != null)
                {
                    user.Status = true;
                    user.ConnectionId = Context.ConnectionId;
                    _userContext.SaveChanges();
                    await Connect(name, true);
                    await UsersOnline(name);
                    await Clients.AllExcept(Context.ConnectionId).SendAsync("Notify", $"{name} has connected!", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
            }
            //else
            //{
            //    await Clients.Caller.SendAsync("OnConnected");
            //}
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string? name = GetNameFromClaims();
            if (name != null)
            {
                User? user = _userContext.Users.FirstOrDefault(u => u.Name == name);
                if (user != null)
                {
                    user.Status = false;
                    user.ConnectionId = null;
                    _userContext.SaveChanges();
                    await Connect(name, false);
                    await Clients.AllExcept(Context.ConnectionId).SendAsync("Notify", $"{name} has disconnected!", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
            }
        }

        public async Task SendPrivateUsers(string message, string[] names, string[] connectionIds)
        {
            string? name = GetNameFromClaims();
            if (name != null && names.Length > 0 && connectionIds.Length > 0)
            {
                await Clients.Clients(connectionIds).SendAsync("PrivateReceive", name, message);
                await Clients.Caller.SendAsync("PrivateReceive", "Me", message, names);
            }
        }

        private string? GetNameFromClaims()
        {
            var claims = Context.User?.Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });
            return claims?.Where(c => c.Type.EndsWith("/name")).FirstOrDefault()?.Value;
        }
    }
}
