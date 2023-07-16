using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace Tortis.Iam.Server.OpenIdConnect;

[Route("api/applications")]
public class ApplicationController : Controller
{
    readonly IOpenIddictApplicationManager _applicationManager;

    public ApplicationController(IOpenIddictApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApplicationListItem), 200, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var apps = _applicationManager.ListAsync(cancellationToken: cancellationToken);

        var response = new List<object>();
        await foreach (var app in apps.WithCancellation(cancellationToken))
        {
            response.Add(new ApplicationListItem(
            
                Id: await _applicationManager.GetIdAsync(app, cancellationToken) ?? string.Empty,
                DisplayName: await _applicationManager.GetDisplayNameAsync(app, cancellationToken) ?? string.Empty
            ));
        }
        return Ok(response);
    }
    record ApplicationListItem(string Id, string DisplayName);
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OpenIddictApplicationDescriptor), 200, MediaTypeNames.Application.Json)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
    {
        var app = await _applicationManager.FindByIdAsync(id, cancellationToken);
        if (app is null)
            return NotFound();
        
        return Ok(app);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(OpenIddictApplicationDescriptor), 200, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Create([FromBody] OpenIddictApplicationDescriptor app, CancellationToken cancellationToken)
    {
        app.ClientId ??= Guid.NewGuid().ToString();
        app.ClientSecret ??= Guid.NewGuid().ToString();
        
        var savedApp = await _applicationManager.CreateAsync(app, cancellationToken);
        return Ok(savedApp);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OpenIddictApplicationDescriptor), 200, MediaTypeNames.Application.Json)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] OpenIddictApplicationDescriptor app, CancellationToken cancellationToken)
    {
        var existingApp = await _applicationManager.FindByIdAsync(id, cancellationToken);
        if (existingApp is null)
            return NotFound();

        await _applicationManager.UpdateAsync(existingApp, app, cancellationToken);

        var updatedApp = _applicationManager.FindByIdAsync(id, cancellationToken);
        return Ok(updatedApp);
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken)
    {
        var existingApp = await _applicationManager.FindByIdAsync(id, cancellationToken);
        if (existingApp is not null)
            await _applicationManager.DeleteAsync(existingApp, cancellationToken);
        
        return NoContent();
    }
}