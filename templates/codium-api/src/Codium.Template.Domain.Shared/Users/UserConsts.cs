namespace Codium.Template.Domain.Shared.Users;

public static class UserConsts
{
    public const int EmailMaxLength = 256;
    public const int PasswordHashMaxLength = 2048;
    public const int PhoneNumberMaxLength = 32;
    
    public const int FirstNameMaxLength = 128;
    public const int LastNameMaxLength = 128;
    
    // Lockout settings
    public const bool AllowedForNewUsers = true;
    public const int MaxFailedAccessAttempts = 5;
    public static readonly TimeSpan DefaultLockoutTimeSpanMinutes = TimeSpan.FromMinutes(15);
    
    // Password settings
    public const int PasswordRequiredLength = 8;
    public const int PasswordMaxLength = 32;
    public const int PasswordRequiredUniqueChars = 1;
    public const bool PasswordRequireDigit = true;
    public const bool PasswordRequireLowercase = true;  
    public const bool PasswordRequireUppercase = true;
    public const bool PasswordRequireNonAlphanumeric = true;
    
    // SignIn settings
    public const int MaxActiveSessionsPerUser = 5;
    public const bool RequireConfirmedEmail = true;
    public const bool RequireConfirmedPhoneNumber = false;
}