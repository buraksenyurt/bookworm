namespace bookworm_api.Auth;

public class IdentityToken
{
    public Guid Value { get; set; }
    public bool IsExpired { get; set; }
}
