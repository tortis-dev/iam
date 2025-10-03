// Licensed under GPL-3 (https://www.gnu.org/licenses/gpl-3.0.en.html)

using Microsoft.EntityFrameworkCore;

using Tortis.Iam.Server.Data;

namespace Tortis.Iam.Server.Components.Resources;

public class IamResourceManager
{
    readonly IamDbContext _dbContext;

    public IamResourceManager(IamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<IamResource> ApiResources => _dbContext.ApiResources;

    public async Task<IamResource?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApiResources.FindAsync([id], cancellationToken);
    }
    
    public async Task<IamResource?> FindByAudienceAsync(string audience, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApiResources.FirstOrDefaultAsync(a => a.Uri == audience, cancellationToken);
    }
    
    public async Task CreateAsync(IamResource resource, CancellationToken cancellationToken = default)
    {
        await _dbContext.ApiResources.AddAsync(resource, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UpdateAsync(IamResource resource, CancellationToken cancellationToken = default)
    {
        _dbContext.Attach(resource).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(IamResource resource, CancellationToken cancellationToken = default)
    {
        _dbContext.ApiResources.Remove(resource);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}