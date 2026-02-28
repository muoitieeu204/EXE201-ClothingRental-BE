using Microsoft.AspNetCore.Authorization;

namespace EXE201.API.Authorization
{
    /// <summary>
    /// Authorization requirement that checks if user has one of the allowed RoleIds
    /// </summary>
    public class MultiRoleIdRequirement : IAuthorizationRequirement
    {
        public int[] AllowedRoleIds { get; }

        public MultiRoleIdRequirement(params int[] allowedRoleIds)
        {
            AllowedRoleIds = allowedRoleIds;
        }
    }
}
