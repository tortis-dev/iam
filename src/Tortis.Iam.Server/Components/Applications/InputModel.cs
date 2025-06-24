using System.ComponentModel.DataAnnotations;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace Tortis.Iam.Server.Components.Applications;

public class InputModel
{
    [Required]
    public string Name { get; set; }
    public string ApplicationType { get; set; }
    public string ClientType { get; set; }
    public string RedirectUris { get; set; }
    public string ClientId { get; set; }
    public string? ClientSecret { get; set; }
    
    public bool AllowClientCredentials { get; set; }
    public bool AllowAuthCode { get; set; }
    public bool AllowRefreshToken { get; set; }
        
    public bool AllowEmail { get; set; }
    public bool AllowProfile { get; set; }
    public bool AllowRoles { get; set; }
    public bool AllowAddress { get; set; }
    public bool AllowPhone { get; set; }

    public bool AllowCode { get; set; }
    public bool AllowIdToken { get; set; }
    public bool AllowCodeIdToken { get; set; }

    public bool RequiredProofKeyForCodeExchange { get; set; }

    public List<string> Scopes { get; set; } = new List<string>(){"Test"};
}

static class MappingExtensions
{
    public static InputModel ToInputModel(this OpenIddictEntityFrameworkCoreApplication<Guid> application)
    {
        var model = new InputModel();
               
        model.Name = application.DisplayName ?? string.Empty;
        model.ApplicationType = application.ApplicationType ?? string.Empty;
        model.ClientType = application.ClientType ?? string.Empty;
        model.ClientId = application.ClientId ?? string.Empty;
        model.ClientSecret = application.ClientSecret;

        model.AllowClientCredentials = application.Permissions?.Contains(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials) ?? false;
        model.AllowAuthCode = application.Permissions?.Contains(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode) ?? false;
        model.AllowRefreshToken = application.Permissions?.Contains(OpenIddictConstants.Permissions.GrantTypes.RefreshToken) ?? false;
        model.AllowEmail = application.Permissions?.Contains(OpenIddictConstants.Permissions.Scopes.Email) ?? false;
        model.AllowProfile = application.Permissions?.Contains(OpenIddictConstants.Permissions.Scopes.Profile) ?? false;
        model.AllowRoles = application.Permissions?.Contains(OpenIddictConstants.Permissions.Scopes.Roles) ?? false;
        model.AllowAddress = application.Permissions?.Contains(OpenIddictConstants.Permissions.Scopes.Address) ?? false;
        model.AllowPhone = application.Permissions?.Contains(OpenIddictConstants.Permissions.Scopes.Phone) ?? false;
        model.AllowCode = application.Permissions?.Contains(OpenIddictConstants.Permissions.ResponseTypes.Code) ?? false;
        model.AllowIdToken = application.Permissions?.Contains(OpenIddictConstants.Permissions.ResponseTypes.IdToken) ?? false;
        model.AllowCodeIdToken = application.Permissions?.Contains(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken) ?? false;
        model.RequiredProofKeyForCodeExchange = application.Requirements?.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange) ?? false;
        
        return model;
    }

    public static OpenIddictApplicationDescriptor ToApplicationDescriptor(this InputModel inputModel)
    {
        var app = new OpenIddictApplicationDescriptor()
        {
            ClientId = inputModel.ClientId,
            DisplayName = inputModel.Name,
            ApplicationType = inputModel.ApplicationType,
            ClientType = inputModel.ClientType,
        };

        if (!string.IsNullOrWhiteSpace(inputModel.ClientSecret) && inputModel.ClientType == OpenIddictConstants.ClientTypes.Confidential)
            app.ClientSecret = inputModel.ClientSecret;
        
        if (!string.IsNullOrWhiteSpace(inputModel.RedirectUris))
        {
            foreach (var uri in inputModel.RedirectUris.Split(","))
                app.RedirectUris.Add(new Uri(uri.Trim()));
        }

        if (inputModel.AllowAuthCode) app.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
        if (inputModel.AllowClientCredentials) app.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
        if (inputModel.AllowRefreshToken) app.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);

        if (inputModel.AllowEmail) app.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Email);
        if (inputModel.AllowAddress) app.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Address);
        if (inputModel.AllowPhone) app.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Phone);
        if (inputModel.AllowProfile) app.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Profile);
        if (inputModel.AllowRoles) app.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Roles);

        if (inputModel.AllowCode) app.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
        if (inputModel.AllowIdToken) app.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdToken);
        if (inputModel.AllowCodeIdToken) app.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);

        if (inputModel is { AllowAuthCode: true, RequiredProofKeyForCodeExchange: true })
            app.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);

        return app;
    }
}