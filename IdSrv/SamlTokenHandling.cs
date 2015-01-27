namespace IdSrv
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Metadata;
    using System.IO;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Web;

    using Kentor.AuthServices;
    using Kentor.AuthServices.Configuration;
    using Kentor.AuthServices.Saml2P;
    using Kentor.AuthServices.WebSso;

    /// <summary>
    /// This is just a wrapper class for dealing with the SAML token
    /// It is mainly here to hide this detail from the discussion about IdentityServer
    /// </summary>
    internal static class SamlTokenHandling
    {
        private static IOptions samlOptions;

        static SamlTokenHandling()
        {
            PopulateOptions();
        }

        public static Saml2Response GetSamlToken(string tokenAsBase64String)
        {
            var xml = Encoding.UTF8.GetString(Convert.FromBase64String(tokenAsBase64String));

            var token = Saml2Response.Read(xml);

            IdentityProvider identityProvider;
            if (!samlOptions.IdentityProviders.TryGetValue(token.Issuer, out identityProvider))
            {
                throw new HttpException(401, "Unkown identity provider"); // Probably not the right thing to do...
            }
            Debug.WriteLine(token.Issuer.Id);
            
            // token.Validate has been made private in the latest AuthServices release so I can't validate the response
            // It will eventually be validated when I do the CreateClaims thing but would be nice to do it explicitly
            
            //if (!token.Validate(samlOptions))
            //{
            //    throw new HttpException(401, "Invalid SAML token signature");
            //}
            return token;
        }

        public static IEnumerable<ClaimsIdentity> GetClaimsIdentities(Saml2Response token)
        {
            try
            {
                return token.GetClaims(samlOptions);
            }
            catch (Saml2ResponseFailedValidationException exception)
            {
                Debug.WriteLine(exception.Message);
                throw new HttpException(401, exception.Message);
            }
        }

        private static void PopulateOptions()
        {
            samlOptions = new Options(new SPOptions()
            {
                EntityId = new EntityId("http://dummy.com") // This is required or the private CreateClaims method will throw an exception
            });

            var provider1Id = new EntityId("http://stubidp.kentor.se/Metadata");
            var certPath = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "Kentor.AuthServices.StubIdp.pfx");
            var cert = new X509Certificate2(certPath);
            var provider1 = new IdentityProvider(provider1Id, new SPOptions() { EntityId = provider1Id })
            {
                AllowUnsolicitedAuthnResponse = true,
                SigningKey = cert.PublicKey.Key,
                Binding = Saml2BindingType.HttpPost
            };
            samlOptions.IdentityProviders[provider1Id] = provider1;

            var provider2Id = new EntityId("http://test2.com");
            var provider2 = new IdentityProvider(provider1Id, new SPOptions() { EntityId = provider2Id });
            samlOptions.IdentityProviders[provider2Id] = provider2;
        }
    }
}