using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EXE201.API.Authorization
{
    /// <summary>
    /// Handler that validates RoleId claim against the required RoleId
    /// </summary>
    public class RoleIdRequirementHandler : AuthorizationHandler<RoleIdRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RoleIdRequirement requirement)
        {
            // Get RoleId claim from user's token
            var roleIdClaim = context.User.FindFirst("RoleId");
            
            if (roleIdClaim == null)
            {
                return Task.CompletedTask; // Fail - no RoleId claim
            }

            // Parse the RoleId value
            if (!int.TryParse(roleIdClaim.Value, out var userRoleId))
            {
                return Task.CompletedTask; // Fail - invalid RoleId format
            }

            // Check if user's RoleId matches the required RoleId
            if (userRoleId == requirement.RequiredRoleId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
