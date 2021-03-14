using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRServerIdentityAuthentication.Hubs
{
    public class ConnectedUser
    {
        public string Name { get; set; }
        public string UserIdentifier { get; set; }

        public List<Connection> Connections { get; set; } 
    }
    public class Connection
    {
         public string ConnectionID { get; set; }
              
    }
}
