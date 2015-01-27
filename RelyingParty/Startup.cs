namespace RelyingParty
{
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.OpenIdConnect;

    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "https://localhost:44306",
                Scope = "openid profile roles",
                ClientId = "mvc",
                RedirectUri = "https://localhost:44307/",
                ResponseType = "id_token",
                //AuthenticationMode = AuthenticationMode.Passive, // This line would cause it to not do a redirect which would be useful for the initial Centrica situation
                AuthenticationMode = AuthenticationMode.Active,

                SignInAsAuthenticationType = "Cookies"
            });
        }
    }
}