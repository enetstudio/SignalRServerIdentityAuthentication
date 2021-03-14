using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.IdentityModel.Tokens;

using Microsoft.IdentityModel;

using System.Net.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Json;

namespace SignalRServerIdentityAuthentication.Hubs
{
    [Authorize()]
    public class ChatHub : Hub
    {
        private static List<ConnectedUser> connectedUsers = new List<ConnectedUser>();
        public async Task InitializeUserList() 
        {
            var list = (from user in connectedUsers
                       select user.Name ).ToList();

            await Clients.All.SendAsync("ReceiveInitializeUserList", list);
        }
        public async Task SendMessage(string userID, string message)
        {
            if (string.IsNullOrEmpty(userID)) // If All selected
            {
                await Clients.All.SendAsync("ReceiveMessage", Context.User.Identity.Name ?? "anonymous", message);
            }
            else
            {
                var userIdentifier = (from _connectedUser in connectedUsers
                                      where _connectedUser.Name == userID
                                      select _connectedUser.UserIdentifier).FirstOrDefault();

                await Clients.User(userIdentifier).SendAsync("ReceivePrivateMessage",
                                       Context.User.Identity.Name ?? "anonymous", message);
            }

        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
           
            var user = connectedUsers.Where(cu => cu.UserIdentifier == Context.UserIdentifier).FirstOrDefault();

            var connection = user.Connections.Where(c => c.ConnectionID == Context.ConnectionId).FirstOrDefault();
            var count = user.Connections.Count;

            if(count == 1) // A single connection: remove user
            {
                connectedUsers.Remove(user);

            }
            if (count > 1) // Multiple connection: Remove current connection
            {
                user.Connections.Remove(connection);
            }

            var list = (from _user in connectedUsers
                        select new { _user.Name }).ToList();

           await Clients.All.SendAsync("ReceiveInitializeUserList", list);

           await   Clients.All.SendAsync("MessageBoard", 
                      $"{Context.User.Identity.Name}  has left");
        
        }

       
        public override async Task OnConnectedAsync()
        {
            var user = connectedUsers.Where(cu => cu.UserIdentifier == Context.UserIdentifier).FirstOrDefault();

            if (user == null) // User does not exist
            {
                ConnectedUser connectedUser = new ConnectedUser
                {
                    UserIdentifier = Context.UserIdentifier,
                    Name = Context.User.Identity.Name,
                    Connections = new List<Connection> { new Connection { ConnectionID = Context.ConnectionId } }
                };

                connectedUsers.Add(connectedUser);
            }
            else
            {
                user.Connections.Add(new Connection { ConnectionID = Context.ConnectionId });
            }

            await Clients.All.SendAsync("MessageBoard", $"{Context.User.Identity.Name}  has joined");

            await  Clients.Client(Context.ConnectionId).SendAsync("ReceiveUserName", Context.User.Identity.Name);
        }
     }
}
