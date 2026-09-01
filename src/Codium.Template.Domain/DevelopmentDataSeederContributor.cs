using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Permissions;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Roles;
using Codium.Template.Domain.UserRoles;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Codium.Template.Domain;

public class DevelopmentDataSeederContributor(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IPermissionRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher<User> passwordHasher,
    ILogger<DevelopmentDataSeederContributor> logger)
{
    public async Task SeedAsync()
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Starting seeding...");

            await SyncPermissionsAsync();
            var adminRole = await EnsureAdminRoleAsync();
            await SyncAdminRolePermissionsAsync(adminRole);
            await EnsureAdminUserAsync(adminRole);

            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation("Seeding completed.");
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "Error occurred during seeding");
        }
    }

    private async Task SyncPermissionsAsync()
    {
        var definedPermissions = GetAllPermissionsFromConsts();

        var existingPermissions = await permissionRepository.GetAllAsync(enableTracking: false);
        var existingNormalizedNames = existingPermissions
            .Select(p => p.NormalizedName)
            .ToHashSet(StringComparer.Ordinal);

        var missingPermissions = definedPermissions
            .Where(p => !existingNormalizedNames.Contains(p.NormalizedName))
            .ToList();

        if (missingPermissions.Count > 0)
        {
            await permissionRepository.AddRangeAsync(missingPermissions);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation(
                "Added {Count} new permission(s): {Names}",
                missingPermissions.Count,
                string.Join(", ", missingPermissions.Select(p => p.Name)));
        }

        var definedNormalizedNames = definedPermissions
            .Select(p => p.NormalizedName)
            .ToHashSet(StringComparer.Ordinal);
        var orphanPermissions = existingPermissions
            .Where(p => !definedNormalizedNames.Contains(p.NormalizedName))
            .ToList();

        if (orphanPermissions.Count > 0)
        {
            logger.LogWarning(
                "Found {Count} permission(s) in the database that no longer exist in PermissionConsts: {Names}",
                orphanPermissions.Count,
                string.Join(", ", orphanPermissions.Select(p => p.Name)));
        }
    }

    private List<Permission> GetAllPermissionsFromConsts()
    {
        var permissions = new List<Permission>();
        var permissionConstType = typeof(PermissionConsts);

        var nestedTypes = permissionConstType.GetNestedTypes();

        foreach (var nestedType in nestedTypes)
        {
            var fields = nestedType.GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    var value = (string)field.GetValue(null)!;
                    permissions.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = value,
                        NormalizedName = value.NormalizeValue()
                    });
                }
            }
        }

        return permissions.DistinctBy(p => p.NormalizedName).ToList();
    }

    private async Task<Role> EnsureAdminRoleAsync()
    {
        var normalizedAdminRoleName = RoleConsts.Admin.NormalizeValue();

        var existingAdminRole = await roleRepository.SingleOrDefaultAsync(
            predicate: r => r.NormalizedName == normalizedAdminRoleName,
            enableTracking: false
        );
        if (existingAdminRole != null)
        {
            return existingAdminRole;
        }

        var newAdminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = RoleConsts.Admin,
            NormalizedName = normalizedAdminRoleName
        };
        await roleRepository.AddAsync(newAdminRole);
        await unitOfWork.SaveChangesAsync();

        return newAdminRole;
    }

    private async Task SyncAdminRolePermissionsAsync(Role adminRole)
    {
        var allPermissions = await permissionRepository.GetAllAsync(enableTracking: false);

        var assignedPermissionIds = (await rolePermissionRepository.GetAllAsync(
                predicate: rp => rp.RoleId == adminRole.Id,
                enableTracking: false))
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        var missingRolePermissions = allPermissions
            .Where(p => !assignedPermissionIds.Contains(p.Id))
            .Select(p => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = adminRole.Id,
                PermissionId = p.Id
            })
            .ToList();

        if (missingRolePermissions.Count == 0)
        {
            return;
        }

        await rolePermissionRepository.AddRangeAsync(missingRolePermissions);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Assigned {Count} missing permission(s) to the Admin role.",
            missingRolePermissions.Count);
    }

    private async Task EnsureAdminUserAsync(Role adminRole)
    {
        const string email = "admin@codium.com";
        var normalizedEmail = email.NormalizeValue();

        var matchedAdminUser = await userRepository.SingleOrDefaultAsync(
            predicate: u => u.Email == email,
            enableTracking: false
        );

        if (matchedAdminUser != null)
        {
            var hasAdminUserRole = await userRoleRepository.AnyAsync(
                ur => ur.UserId == matchedAdminUser.Id && ur.RoleId == adminRole.Id);

            if (!hasAdminUserRole)
            {
                await userRoleRepository.AddAsync(new UserRole
                {
                    Id = Guid.NewGuid(),
                    RoleId = adminRole.Id,
                    UserId = matchedAdminUser.Id
                });
                await unitOfWork.SaveChangesAsync();
            }

            return;
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = normalizedEmail,
            EmailConfirmed = true,
            PhoneNumber = null,
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = false,
            LockoutEnd = null,
            LockoutEnabled = false,
            AccessFailedCount = 0,
            FirstName = "Admin",
            LastName = "User",
            PasswordChangedTime = null,
            IsActive = true
        };
        newUser.PasswordHash = passwordHasher.HashPassword(newUser, "Pp123456*");
        await userRepository.AddAsync(newUser);

        var newUserRole = new UserRole
        {
            Id = Guid.NewGuid(),
            RoleId = adminRole.Id,
            UserId = newUser.Id
        };
        await userRoleRepository.AddAsync(newUserRole);

        await unitOfWork.SaveChangesAsync();
    }
}