using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tortis.Iam.Oidc.Clients;

//TODO: Require authentication/authorization
[Route("api/clients")]
public class ClientController : ControllerBase
{
    readonly ClientService _service;

    public ClientController(ClientService service)
    {
        _service = service;
    }

    [HttpGet]
    public Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id, CancellationToken cancellationToken)
    {
        var client = await _service.FindByClientIdAsync(id, cancellationToken);
        if (client is null)
            return NotFound();

        return Ok(client);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Client client, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.CreateAsync(client, cancellationToken));
        }
        catch (Exception)
        {
            return Problem(title: "Failed to create client.", statusCode: 500);
        }
    }
    
    [HttpPut("{id}")]
    public Task<IActionResult> Update([FromRoute] string id, [FromBody] Client client, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    [HttpDelete("{id}")]
    public Task<IActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}