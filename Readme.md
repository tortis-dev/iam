# Tortis IAM

This project aims to be a FOSS IAM solution.

## Features

- [ ] OpenIdConnect/OAuth2.0 Flows/Grant Types
  - [x] Client Credentials
  - [x] Authorization Code + PKCE
  - [ ] Password
  - [ ] Implicit
  - [x] Hybrid
  - [ ] Delegation/On-Behalf-Of (https://datatracker.ietf.org/doc/html/rfc8693)
- [x] Scopes
- [x] Local logins/users
  - [x] MFA
- [ ] External logins and SSO
  - [ ] LDAP
  - [x] OpenIdConnect Federation
  - [ ] SAML Federation
- [x] Roles/RBAC
- [ ] Financial API Baseline compliant (maybe certified) (https://openid.net/wg/fapi/)

## Database Support

Thanks to Entity Framework Core, multiple database platforms can be supported: 

- [X] Microsoft SQL Server
- [ ] Sqlite
- [ ] Oracle
- [ ] PostgreSQL
- [ ] MySQL
- [ ] Azure CosmosDB
- [ ] MongoDB

## Third Party vs Build From Scratch

The immediate need is a full IAM solution that is not Keycloak and is not based on Duende, so step one is "make it work".
In that vain, the initial release will leverage [OpenIddict](https://openiddict.com/) and 
[AspNet Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity).

Once I have a working IAM solution with all the features desired, I'll consider replacing OpenIddict and AspNet Core
identity with my own libraries. This will be a major undertaking and require a lot of maintenance effort. For that reason,
I _may_ stick with these dependencies. At the very least, I'll Clean up things so OpenIddict and AspNet Core Identity
are abstracted from the IAM solution itself.

## Certification

I would like to get the solution OpenId Connect certified and Financial API. 

## Delivery

Considering delivering as a container as well as a nuget package(s) consumers can pull into their own applications.

## Trusted Servers (https://datatracker.ietf.org/doc/html/rfc8693)

To perform on-behalf-of where the user's token comes from a different issuer, Tortis IAM must first trust the issuer.

Example: Users log in using Okta, but backend microservices get their tokens from Tortis IAM.

- https://datatracker.ietf.org/doc/html/rfc8693

- https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow

Okta has a similar concept.

- https://developer.okta.com/docs/guides/set-up-token-exchange/main/#trusted-servers

This mechanism could also be used to configure Federation?

## License

The complete application is licensed under GPL-3. Libraries delivered for devs to create their own IAM solution are
licensed under Apache-2.

OpenIDDict is licensed under Apache-2 (https://github.com/openiddict/openiddict-core/blob/dev/LICENSE.md)

AspNet Core Identity is licensed under MIT (https://github.com/dotnet/aspnetcore/blob/main/LICENSE.txt)
