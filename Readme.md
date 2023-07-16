# Tortis IAM

Licensed under MIT.

This project aims to be a FOSS replacement for Duende Identity Server as well as a fully functional, customizable, IAM solution.

## Features

- [ ] OpenIdConnect Flows
  - [ ] Client Credentials
  - [ ] Authorization Code + PKCE
  - [ ] Password
- [ ] Scopes
- [ ] Local logins/users
  - [ ] MFA
- [ ] External logins and SSO
  - [ ] OpenIdConnect Federation
  - [ ] SAML Federation
- [ ] Roles

## Delivery

Considering delivering as a container as well as a nuget package(s) consumers can pull into their own applications.

## Trusted Servers

To perform on-behalf-of where the user's token comes from a different issuer, Tortis IAM must first trust the issuer.

Example: Users log in using Okta, but backend microservices get their tokens from Tortis IAM.
https://datatracker.ietf.org/doc/html/rfc8693
https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow

Okta has a similar concept.
https://developer.okta.com/docs/guides/set-up-token-exchange/main/#trusted-servers

This mechanism could also be use to configure Federation?
