using System.ComponentModel.DataAnnotations;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace Tortis.Iam.Server.Components.Applications;

public class ApplicationViewModel
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

    public List<string> Scopes { get; } = [];
    
    public ApplicationViewModel Clone()
    {
        var newModel = new ApplicationViewModel
        {
            Name = this.Name,
            ApplicationType = this.ApplicationType,
            ClientType = this.ClientType,
            ClientId = this.ClientId,
            ClientSecret = this.ClientSecret,
            AllowClientCredentials = this.AllowClientCredentials,
            AllowAuthCode = this.AllowAuthCode,
            AllowRefreshToken = this.AllowRefreshToken,
            AllowEmail = this.AllowEmail,
            AllowProfile = this.AllowProfile,
            AllowRoles = this.AllowRoles,
            AllowAddress = this.AllowAddress,
            AllowPhone = this.AllowPhone,
            AllowCode = this.AllowCode,
            AllowIdToken = this.AllowIdToken,
            AllowCodeIdToken = this.AllowCodeIdToken,
            RequiredProofKeyForCodeExchange = this.RequiredProofKeyForCodeExchange,
            RedirectUris = this.RedirectUris
        };
        
        this.Scopes.ForEach(s => newModel.Scopes.Add(new String(s)));
        
        return newModel;
    }
}

static class MappingExtensions
{
    /// <summary>
    /// Maps an OpenIddict application to a view model.
    /// </summary>
    /// <param name="application"></param>
    /// <returns></returns>
    public static ApplicationViewModel ToViewModel(this OpenIddictApplicationDescriptor application)
    {
        var model = new ApplicationViewModel();
               
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
        
        if(application.Permissions is not null)
            model.Scopes.AddRange(
                application.Permissions
                    .Where(p => 
                        p.StartsWith(OpenIddictConstants.Permissions.Prefixes.Scope) &&
                        !SystemScopes.Contains(p))
                    .Select(p => p.Substring(OpenIddictConstants.Permissions.Prefixes.Scope.Length)));
        
        return model;
    }

    private static string[] SystemScopes = [OpenIddictConstants.Permissions.Scopes.Address, OpenIddictConstants.Permissions.Scopes.Email, OpenIddictConstants.Permissions.Scopes.Phone, OpenIddictConstants.Permissions.Scopes.Profile, OpenIddictConstants.Permissions.Scopes.Roles];
    
    /// <summary>
    /// Maps an application view model to an OpenIddict application.
    /// </summary>
    /// <param name="applicationViewModel"></param>
    /// <returns></returns>
    public static OpenIddictApplicationDescriptor ToApplicationDescriptor(this ApplicationViewModel applicationViewModel)
    {
        var app = new OpenIddictApplicationDescriptor()
        {
            ClientId = applicationViewModel.ClientId,
            DisplayName = applicationViewModel.Name,
            ApplicationType = applicationViewModel.ApplicationType,
            ClientType = applicationViewModel.ClientType,
        };

        if (!string.IsNullOrWhiteSpace(applicationViewModel.ClientSecret) && applicationViewModel.ClientType == OpenIddictConstants.ClientTypes.Confidential)
            app.ClientSecret = applicationViewModel.ClientSecret;
        
        if (!string.IsNullOrWhiteSpace(applicationViewModel.RedirectUris))
        {
            foreach (var uri in applicationViewModel.RedirectUris.Split(","))
                app.RedirectUris.Add(new Uri(uri.Trim()));
        }

        app.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
        
        if (applicationViewModel.AllowAuthCode)
        {
            app.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            app.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
        }
        if (applicationViewModel.AllowClientCredentials) app.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
        if (applicationViewModel.AllowRefreshToken) app.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);

        if (applicationViewModel.AllowEmail) app.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Email);
        if (applicationViewModel.AllowAddress) app.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Address);
        if (applicationViewModel.AllowPhone) app.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Phone);
        if (applicationViewModel.AllowProfile) app.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Profile);
        if (applicationViewModel.AllowRoles) app.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Roles);

        if (applicationViewModel.AllowCode) app.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
        if (applicationViewModel.AllowIdToken) app.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdToken);
        if (applicationViewModel.AllowCodeIdToken) app.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);

        if (applicationViewModel is { AllowAuthCode: true, RequiredProofKeyForCodeExchange: true })
            app.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);

        foreach (var scope in applicationViewModel.Scopes)
            app.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
        
        return app;
    }
}