namespace LaboratorioTlahuac.Application.Authentication;

public sealed record LoginResult(
    bool Succeeded,
    AuthenticatedUser? User,
    LoginFailureReason? FailureReason)
{
    public static LoginResult Success(AuthenticatedUser user)
    {
        return new LoginResult(true, user, null);
    }

    public static LoginResult Failure(LoginFailureReason reason)
    {
        return new LoginResult(false, null, reason);
    }
}
