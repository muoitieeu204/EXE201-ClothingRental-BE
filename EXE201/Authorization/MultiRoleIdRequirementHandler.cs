using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace EXE201.API.Authorization
{
    /// <summary>
    /// Handler that validates RoleId claim against multiple allowed RoleIds
    /// </summary>
    public class MultiRoleIdRequirementHandler : AuthorizationHandler<MultiRoleIdRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MultiRoleIdRequirement requirement)
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

            // Check if user's RoleId is in the allowed list
            if (requirement.AllowedRoleIds.Contains(userRoleId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
