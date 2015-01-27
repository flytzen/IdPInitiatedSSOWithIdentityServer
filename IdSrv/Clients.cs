using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdSrv
{
    using Thinktecture.IdentityServer.Core.Models;

    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
        {
            new Client 
            {
                Enabled = true,
                ClientName = "MVC Client",
                ClientId = "mvc",
                Flow = Flows.Implicit, 

                RedirectUris = new List<string>
                {
                    "https://localhost:44307/"
                },
                    PostLogoutRedirectUris = new List<string>
                {
                    "https://localhost:44307/"
                }
            }
        };
        }
    }
}