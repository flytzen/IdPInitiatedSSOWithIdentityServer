# IdP initiated SSO with ThinkTecture Identity Server
POC code for doing IdP initiated SSO with ThinkTecture Identity Server

**DISCLAIMER: I am not a security expert. The code example here may have huge security holes in it.**

NOTE: For simplicity, this project just uses the IdentityServer Nuget packages. Unfortunately, that causes the code to not compile as a certain class is internal.
It can be made to compile if you use the source of IdentityServer and also install the Autofac Nuget package into the IdSrv project.
However, the purpose of this is to show the POC.

All the code that is relevant to interacting with Identity Server is encapsulated in the class IdSrvInteraction, which is called from the controller SamlIdpController.

## What it's for
In the SAML world it is common to do what is known as IdP-initiated SAML. This means that the user *starts* at the Identifying Party who posts an unsolicited token to the relying party.
This is in contrast to most other SSO, where you start at the Relying Party which then sends you to the Identifying Party where you login and are sent back.
Indeed, OIDC protocol explicitly requires a flow like that as the IdP need to respond with some value(s) provided by the RP in the initial request.

In the scenario where you want to use Identity Server as your primary authentication mechanism, you need to be able to push unsolicited SAML tokens to Identity Server and Identity Server need to then do something with that and send you on to the application you really want to get to.

## The Flow
As mentioned above, OIDC does not support IdP-initiated SSO, so the trick to making this work is this flow;

1. SAML IdP posts a SAML token to Identity Server
2. Identity Server processes the SAML Token and logs the user in to the Identity Server in the same way as if Identity Server had sent a request to the IdP in the first place.
  a. IUserService.AuthenticateExternalAsync need to be called to create/update the external user in the user repository
  b. The correct IdentityServer authentication cookies need to be set
3. The user is redirected to the Relying Party on a URL that will cause the RP to redirect the user *back* to Identity Server to ask for a token. Depending on the configuration of the RP, this can be done with the Authorize attribute or by a controller method that explicitly invokes the appropriate middleware.
4. Because the user is already logged in to IdentityServer, the token is issued and the user is redirected back to the RP.

## Implementation notes
In the POC code the interaction is in an MVC controller. This is just to make it easier to see what is going on and it should be moved to middleware in an actual solution. 
This is especially important if you want to use app.Map to put IdentityServer under a "folder" as the IdentityServer cookies will then be scoped to that path and the ones set from an MVC controller won't work (at least I had some problems in my testing).


