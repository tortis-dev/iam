# Tortis IAM

This project aims to be a FOSS IAM solution.

## Roadmap

### Phases

#### Release milestone 1
[ ] Make it work—just get a basic prototype fully functional.
[ ] Make it right—tidy up the code, make it look good, make it work well, make it secure.

#### Release milestone 2
[ ] Make it fast—add additional database support beyond SQLite and focus on performance and scalerability.

#### Release milestone 3
[ ] Financial API compliant—make it compliant with the Financial API Baseline.

### Features

- [ ] OpenIdConnect/OAuth2.0 Flows and Grant Types
    - [x] Client Credentials
    - [x] Authorization Code + PKCE
    - [X] Refresh Token
    - [ ] Resource Owner Password
    - [ ] Implicit
    - [X] Hybrid
    - [ ] Delegation/On-Behalf-Of (https://datatracker.ietf.org/doc/html/rfc8693)
    - [ ] Device Flow
- [x] Scopes
- [x] Local logins/users
    - [x] MFA
- [ ] External logins and SSO
    - [ ] LDAP
    - [x] OpenIdConnect Federation
    - [ ] SAML Federation
- [x] Roles/RBAC
- [ ] Financial API Baseline compliant (maybe certified) (https://openid.net/wg/fapi/)
- [ ] SIEM auditing events
- [ ] Local auditing

### Database Support

Thanks to Entity Framework Core, multiple database platforms can be supported. SQLite will be first as it is easy to
iterate on during development and should be able to support 100's, if not 1,000's of users in production.

- [X] Sqlite
- [ ] Microsoft SQL Server
- [ ] Oracle
- [ ] PostgreSQL
- [ ] MySQL

While other drivers are available for EF, OpenIddict uses Quartz.NET which only supports the databases above.
[https://www.quartz-scheduler.net/documentation/quartz-3.x/configuration/reference.html#quartz-jobstore-driverdelegatetype](https://www.quartz-scheduler.net/documentation/quartz-3.x/configuration/reference.html#quartz-jobstore-driverdelegatetype)

## Certificates

- HTTPS certificate
- OpenIdConnect certificate - Token signing
- Master Certificate - generate data protection keys


## Concepts

### Application/Client

An application (aka client) is software that requests tokens for access resources. 
Example: A web application, a mobile application, a service, etc.

In some cases, an application may be a resource itself. 
Example: An ASP Net Core MVC application is also a resource.

### Resource

A resource is something you to protect that is to be accessed by clients/applications. 
This is usually an API (e.g. microservice), but could also be a database, message queue, etc.

### Scope

A scope is a permission associated with a resource that a client can request.

### User

A user is a person that uses an application to access resources.

### Role

A role is a permission that a user can request.


## Third Party vs Build From Scratch

The immediate need is a full IAM solution that is not Keycloak and is not based on Duende, so step one is "make it work".
In that vain, the initial release will leverage [OpenIddict](https://openiddict.com/) and 
[AspNet Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity).

Once I have a working IAM solution with all the features desired, I'll consider replacing OpenIddict and AspNet Core
identity with my own libraries. This will be a major undertaking and require a lot of maintenance effort. For that reason,
I _may_ stick with these dependencies. At the very least, I'll "Clean" up things so OpenIddict and AspNet Core Identity
are abstracted from the IAM solution itself to limit the impact of breaking changes to either of those dependencies.

## Certification

I would like to get the solution OpenId Connect certified and Financial API certified. 

## Delivery

Considering delivering as a container as well as a nuget package(s) that consumers can pull into their own applications.

## Trusted Servers (https://datatracker.ietf.org/doc/html/rfc8693)

To perform on-behalf-of where the user's token comes from a different issuer, Tortis IAM must first trust the issuer.

Example: Users log in using Okta, but backend microservices get their tokens from Tortis IAM.

- https://datatracker.ietf.org/doc/html/rfc8693

- https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow

Okta has a similar concept.

- https://developer.okta.com/docs/guides/set-up-token-exchange/main/#trusted-servers

This mechanism could also be used to configure Federation?

## License

The complete application is licensed under GPL-3. Libraries delivered for devs to create their own IAM solution will be
licensed under Apache-2 should that day come.

OpenIddict is licensed under Apache-2 (https://github.com/openiddict/openiddict-core/blob/dev/LICENSE.md)

AspNet Core Identity is licensed under MIT (https://github.com/dotnet/aspnetcore/blob/main/LICENSE.txt)

## Items of Note

There is some janky null handling and forgiving in the code because AspNet Core Identity has required fields--i.e.
Username, Role Name--marked as nullable in both code and the database. All the more reason to abstract away Identity.
