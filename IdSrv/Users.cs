using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdSrv
{
    using System.Security.Claims;

    using Thinktecture.IdentityServer.Core;
    using Thinktecture.IdentityServer.Core.Services.InMemory;

    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
        {
            new InMemoryUser
            {
                Username = "bob",
                Password = "secret",
                Subject = "1",

                Claims = new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                    new Claim(Constants.ClaimTypes.Role, "Geek"),
                    new Claim(Constants.ClaimTypes.Role, "Foo")
                }
            }
        };
        }
    }
}