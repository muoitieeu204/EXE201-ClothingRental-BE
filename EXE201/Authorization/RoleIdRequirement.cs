using Microsoft.AspNetCore.Authorization;

namespace EXE201.API.Authorization
{
    /// <summary>
    /// Authorization requirement that checks if user has a specific RoleId
    /// </summary>
    public class RoleIdRequirement : IAuthorizationRequirement
    {
        public int RequiredRoleId { get; }

        public RoleIdRequirement(int requiredRoleId)
        {
            RequiredRoleId = requiredRoleId;
        }
    }
}
