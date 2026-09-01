namespace Codium.Template.Domain.Shared.Permissions;

public static class PermissionConsts
{
    public const int NameMaxLength = 256;
    
    
    private const string Separator = ".";
    
    public static class Permission
    {
        private const string Group = "Permission";
        public const string GetAll = Group + Separator + "GetAll";
    }
    
    public static class Role
    {
        private const string Group = "Role";
        public const string GetById = Group + Separator + "GetById";
        public const string GetAll = Group + Separator + "GetAll";
        public const string Paged = Group + Separator + "Paged";
        public const string Create = Group + Separator + "Create";
        public const string Update = Group + Separator + "Update";
        public const string Delete = Group + Separator + "Delete";
        public const string AssignPermission = Group + Separator + "AssignPermission";
        public const string UnAssignPermission = Group + Separator + "UnAssignPermission";
    }
    
    public static class User
    {
        private const string Group = "User";
        public const string GetById = Group + Separator + "GetById";
        public const string GetAll = Group + Separator + "GetAll";
        public const string Paged = Group + Separator + "Paged";
        public const string Create = Group + Separator + "Create";
        public const string Update = Group + Separator + "Update";
        public const string Delete = Group + Separator + "Delete";
        public const string AssignRole = Group + Separator + "AssignRole";
        public const string UnAssignRole = Group + Separator + "UnAssignRole";
        public const string Lock = Group + Separator + "Lock";
        public const string Unlock = Group + Separator + "Unlock";
        public const string ResetPassword = Group + Separator + "ResetPassword";
    }
}