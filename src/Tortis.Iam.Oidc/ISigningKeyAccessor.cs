using Microsoft.IdentityModel.Tokens;

namespace Tortis.Iam.Oidc;

public interface ISigningKeyAccessor
{
    SecurityKey GetKey();
}

class DevelopmentSigningKey : ISigningKeyAccessor
{
    SymmetricSecurityKey _key;
    
    public DevelopmentSigningKey()
    {
        var bytes = Guid.NewGuid().ToByteArray().Concat(Guid.NewGuid().ToByteArray()).ToArray();
        _key = new SymmetricSecurityKey(bytes);
    }

    public SecurityKey GetKey() => _key;
}
