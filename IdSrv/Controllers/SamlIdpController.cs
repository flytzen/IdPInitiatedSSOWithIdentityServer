namespace IdSrv.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using Kentor.AuthServices.Saml2P;

    using Thinktecture.IdentityServer.Core;
    using Thinktecture.IdentityServer.Core.Models;

    [RoutePrefix("SamlIdp")]
    public class SamlIdpController : Controller
    {
        [Route("Login")]
        [HttpPost]
        public async Task<ActionResult> Login(string SAMLResponse)
        {
            // Get the stuff we need out of SAML
            var token = SamlTokenHandling.GetSamlToken(SAMLResponse); // Note the token is not yet validated
            // First build an ExternalIdentity based on the data we get from the external IdP
            var externalIdentity = BuildExternalIdentity(token);

            // This will do all the interaction with IdSrv
            // The implementation here is just POC and the use of a class is just to make it easy
            // to show Dominick and Brock where the touchpoints are
            var owinContext = this.Request.GetOwinContext();
            var idSrvInteractor = new IdSrvInteraction(owinContext);
            await idSrvInteractor.LogInToIdSrv(externalIdentity);

            // Redirect to the RELYING PARTY 
            // This needs to be a URL that will cause the RP to redirect back to IdSrv for authentication
            // You could do a whole bunch of stuff with determining this path based on the IdP
            // or pass in a returnUrl etc. Probably want some security on allowed redirectUrls though!
            return this.Redirect("https://localhost:44307/home/secure");
        }


        private static ExternalIdentity BuildExternalIdentity(Saml2Response token)
        {
            var claimsIdentities = SamlTokenHandling.GetClaimsIdentities(token); // This will trigger the validation of the token
            
            // Very unsafe - this is POC code!!
            var nameIdClaim =
                claimsIdentities.First()
                    .Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            
            var externalIdentity = new ExternalIdentity()
            {
                Provider = token.Issuer.Id,
                ProviderId = nameIdClaim.Value,
                Claims =
                    new List<Claim>()
                                {
                                    new Claim(Constants.ClaimTypes.Name, "Billy Bob"),
                                    new Claim(Constants.ClaimTypes.Role, "Boss")
                                }
            };
            return externalIdentity;
        }
    }
}