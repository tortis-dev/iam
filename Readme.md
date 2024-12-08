# Tortis IAM

Licensed under Apache-2 and GPL-3.

This project aims to be a FOSS replacement for Duende Identity Server as well as a fully functional, customizable, 
IAM solution.

## Features

- [ ] OpenIdConnect/OAuth2.0 Flows/Grant Types
  - [ ] Client Credentials
  - [ ] Authorization Code + PKCE
  - [ ] Password
  - [ ] Implicit
  - [ ] Hybrid
  - [ ] Delegation/On-Behalf-Of (https://datatracker.ietf.org/doc/html/rfc8693)
- [ ] Scopes
- [ ] Local logins/users
  - [ ] MFA
- [ ] External logins and SSO
  - [ ] LDAP
  - [ ] OpenIdConnect Federation
  - [ ] SAML Federation
- [ ] Roles/RBAC
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

I would like to get the solution OpenId Connect certified. 

## Delivery

Considering delivering as a container as well as a nuget package(s) consumers can pull into their own applications.

## Trusted Servershttps://datatracker.ietf.org/doc/html/rfc8693

To perform on-behalf-of where the user's token comes from a different issuer, Tortis IAM must first trust the issuer.

Example: Users log in using Okta, but backend microservices get their tokens from Tortis IAM.

- https://datatracker.ietf.org/doc/html/rfc8693

- https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow

Okta has a similar concept.

- https://developer.okta.com/docs/guides/set-up-token-exchange/main/#trusted-servers

This mechanism could also be used to configure Federation?
