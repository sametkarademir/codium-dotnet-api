namespace Codium.Template.Domain.Shared.Result;

public class SignInResult
{
    public bool Succeeded { get; private set; }
    public bool IsLockedOut { get; private set; }
    
    private SignInResult(bool succeeded = false, bool isLockedOut = false)
    {
        Succeeded = succeeded;
        IsLockedOut = isLockedOut;
    }

    public static SignInResult Success() => new SignInResult(succeeded: true);
    public static SignInResult Failed() => new SignInResult();
    public static SignInResult LockedOut() => new SignInResult(isLockedOut: true);
}