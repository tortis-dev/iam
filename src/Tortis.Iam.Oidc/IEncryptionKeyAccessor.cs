using Microsoft.IdentityModel.Tokens;

namespace Tortis.Iam.Oidc;

public interface IEncryptionKeyAccessor
{
    SecurityKey GetKey();
}

sealed class DevelopmentEncryptionKey : IEncryptionKeyAccessor
{
    SymmetricSecurityKey _key;
    
    public DevelopmentEncryptionKey()
    {
        var bytes = Guid.NewGuid().ToByteArray().Concat(Guid.NewGuid().ToByteArray()).ToArray();
        _key = new SymmetricSecurityKey(bytes.Concat(bytes).ToArray());
    }

    public SecurityKey GetKey() => _key;
}