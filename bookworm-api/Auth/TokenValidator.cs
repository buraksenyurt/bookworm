namespace bookworm_api.Auth;

public class TokenValidator
    : ITokenValidator
{
    private readonly List<IdentityToken> _tokens = [];

    public TokenValidator()
    {
        _tokens.Add(new IdentityToken
        {
            Value = Guid.Parse("3105F476-5C0F-48FA-AF47-59D0122FE4C5"),
            IsExpired = true
        });

        _tokens.Add(new IdentityToken
        {
            Value = Guid.Parse("3176FF45-6719-40B0-8534-0728FDCE04E2"),
            IsExpired = false
        });
    }
    public bool IsValid(Guid token)
    {
        var identityToken = _tokens.FirstOrDefault(t => t.Value == token);
        if (identityToken == null)
        {
            return false;
        }
        return !identityToken.IsExpired;
    }
    public bool IsExpired(Guid token)
    {
        var identityToken = _tokens.FirstOrDefault(t => t.Value == token);
        if (identityToken == null)
        {
            return false;
        }
        return identityToken.IsExpired;
    }
}
