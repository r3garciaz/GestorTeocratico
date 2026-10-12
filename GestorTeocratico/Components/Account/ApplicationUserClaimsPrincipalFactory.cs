using System.Security.Claims;
using GestorTeocratico.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GestorTeocratico.Components.Account;

/// <summary>
/// Adds the user's public ID as a claim when the sign-in principal is created,
/// so layouts can display it without querying the database on every request
/// (which would compete with the page's own DbContext usage).
/// Inherits the role-aware base factory so role claims are still generated.
/// </summary>
public class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    public const string PublicIdClaim = "public_id";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(user.PublicId))
        {
            identity.AddClaim(new Claim(PublicIdClaim, user.PublicId));
        }

        return identity;
    }
}
