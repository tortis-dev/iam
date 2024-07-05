using Microsoft.AspNetCore.Mvc.Testing;

namespace Tortis.Iam.Oidc.Tests;

public class HostFixure : IDisposable
{
    internal WebApplicationFactory<Program> Host { get; }
    
    public HostFixure()
    {
        Host = new WebApplicationFactory<Program>();
    }
    
    public void Dispose()
    {
        Host.Dispose();
    }
}