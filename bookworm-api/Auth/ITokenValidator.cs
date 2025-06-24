namespace bookworm_api.Auth;

public interface ITokenValidator
{
    public bool IsValid(Guid token);
    public bool IsExpired(Guid token);
}
