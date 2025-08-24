// Licensed under GPL-3 (https://www.gnu.org/licenses/gpl-3.0.en.html)

using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace Tortis.Iam.Server.Components.Applications;

public class IamApplicationManager : OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>>
{
    public IamApplicationManager(
        IOpenIddictApplicationCache<OpenIddictEntityFrameworkCoreApplication<Guid>> cache,
        ILogger<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>>> logger,
        IOptionsMonitor<OpenIddictCoreOptions> options,
        IOpenIddictApplicationStore<OpenIddictEntityFrameworkCoreApplication<Guid>> store) 
        : base(cache, logger, options, store)
    {
    }

    protected override ValueTask<string> ObfuscateClientSecretAsync(string secret, CancellationToken cancellationToken = new CancellationToken())
    {
        //TODO: Use the DataProtectionAPI to obfuscate the secret
        return base.ObfuscateClientSecretAsync(secret, cancellationToken);
    }

    public override ValueTask<bool> ValidateClientSecretAsync(OpenIddictEntityFrameworkCoreApplication<Guid> application, string secret,
        CancellationToken cancellationToken = new CancellationToken())
    {
        //TODO: Use the DataProtectionAPI to validate the secret
        return base.ValidateClientSecretAsync(application, secret, cancellationToken);
    }
}