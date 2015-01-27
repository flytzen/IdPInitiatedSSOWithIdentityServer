using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdSrv
{
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Web.Http;

    using Owin;

    using Thinktecture.IdentityServer.Core.Configuration;
    using Thinktecture.IdentityServer.Core.Resources;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseIdentityServer(
                new IdentityServerOptions
                    {
                        SiteName = "Test IdentityServer",
                        SigningCertificate = LoadCertificate(),
                        Factory =
                            InMemoryFactory.Create(
                                users: Users.Get(),
                                clients: Clients.Get(),
                                scopes: Scopes.Get()),
                    });

        }

        private X509Certificate2 LoadCertificate()
        {
            var certPath = string.Format(@"{0}bin\assets\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory);
            Debug.Assert(File.Exists(certPath));
            return new X509Certificate2(certPath, "idsrv3test");
        }
    }
}