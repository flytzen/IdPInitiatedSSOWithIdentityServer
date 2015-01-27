namespace IdSrv.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Autofac.Core.Lifetime;

    using Microsoft.Owin;

    using Thinktecture.IdentityServer.Core;
    using Thinktecture.IdentityServer.Core.Models;
    using Thinktecture.IdentityServer.Core.Services;

    internal class IdSrvInteraction
    {
        private readonly IOwinContext owinContext;

        public IdSrvInteraction(IOwinContext owinContext)
        {
            this.owinContext = owinContext;
        }

        public async Task LogInToIdSrv(ExternalIdentity externalIdentity)
        {
            // Now ask the user service to check if the user exist or potentially create a new user
            // The InMemoryStore will automatically create users
            // You will almost certainly want a custom user store that knows your rules for handling external logins
            // and mapping them to "local" users ... though I guess that is not unique to IdP-initiated 
            var userService = this.GetUserService();
            var p = await userService.AuthenticateExternalAsync(externalIdentity, new SignInMessage());

            // TODO: Check if the userservice was happy and do some sensible error handling

            // Get IdentityServer to set an authentication cookie
            this.SetTheIdSrvAuthCookie(p);
        }

        /// <summary>
        /// This does the mechanics of getting IdSrv to log you in and set an auth cookie
        /// Basically copy & paste from the auth controller
        /// Suggest this is refactored in IdSrv to call it easily
        /// </summary>
        private void SetTheIdSrvAuthCookie(AuthenticateResult p)
        {
            var user = CreateFromPrincipal(p.User, Constants.PrimaryAuthenticationType);
            this.owinContext.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);

            var props = new Microsoft.Owin.Security.AuthenticationProperties();
            var id = user.Identities.First();

            this.owinContext.Authentication.SignIn(props, id);
        }

        /// <summary>
        /// Horrible hack to get the IUserService
        /// Would like to replace with a cleaner way but requires changes to IdSrv
        /// </summary>
        /// <returns></returns>
        private IUserService GetUserService()
        {
            // NOTE: This line works only if you run against the source of IdentityServer rather than the Nuget package
            
            var autofacScope = this.owinContext.Environment["idsrv:AutofacScope"] as Autofac.Core.Lifetime.LifetimeScope;

            if (autofacScope == null)
            {
                throw new Exception("Unable to get the autofacscope");
            }

            var userService = autofacScope.GetService(typeof(IUserService)) as IUserService;
            if (userService == null)
            {
                throw new Exception("Failed to get IUserService");
            }
            return userService;
        }

        /// <summary>
        /// Complete copy of IdentityServerPrincipal.CreateFromPrincipal as that class is now internal
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="authenticationType"></param>
        /// <returns></returns>
        private static ClaimsPrincipal CreateFromPrincipal(ClaimsPrincipal principal, string authenticationType)
        {
            // we require the following claims
            var subject = principal.FindFirst(Constants.ClaimTypes.Subject);
            if (subject == null) throw new InvalidOperationException("sub claim is missing");

            var name = principal.FindFirst(Constants.ClaimTypes.Name);
            if (name == null) throw new InvalidOperationException("name claim is missing");

            var authenticationMethod = principal.FindFirst(Constants.ClaimTypes.AuthenticationMethod);
            if (authenticationMethod == null) throw new InvalidOperationException("amr claim is missing");

            var authenticationTime = principal.FindFirst(Constants.ClaimTypes.AuthenticationTime);
            if (authenticationTime == null) throw new InvalidOperationException("auth_time claim is missing");

            var idp = principal.FindFirst(Constants.ClaimTypes.IdentityProvider);
            if (idp == null) throw new InvalidOperationException("idp claim is missing");

            var id = new ClaimsIdentity(principal.Claims, authenticationType, Constants.ClaimTypes.Name, Constants.ClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }

    }
}