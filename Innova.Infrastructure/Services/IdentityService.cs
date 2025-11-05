using Innova.Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Innova.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    private async Task<bool> Exsits(Expression<Func<AppUser, bool>> specification)
    {
        return await _userManager.Users.AnyAsync(specification);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await Exsits(u => u.Email == email);
    }

    public async Task<bool> UserNameExistsAsync(string userName)
    {
        return await Exsits(u => u.UserName == userName);
    }

    public async Task<List<string>> CreateUserAsync(string firstName, string lastName, string email, DateTime? dateOfBirth, string userName, string password)
    {
        var user = new AppUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = userName,
            CreatedAt = DateTime.UtcNow,
            DateOfBirth = dateOfBirth
        };
        var errors = new List<string>();
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            errors.AddRange(result.Errors.Select(e => e.Description));
        return errors;
    }

    public async Task<List<string>> AddToRoleAsync(string userName, string roleName)
    {
        var errors = new List<string>();

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            errors.Add("User not found.");
            return errors;
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!createRoleResult.Succeeded)
            {
                errors.AddRange(createRoleResult.Errors.Select(e => e.Description));
                return errors;
            }
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
            errors.AddRange(result.Errors.Select(e => e.Description));

        return errors;
    }

    public async Task<IList<string>> GetRolesAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            return new List<string>();

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IList<Claim>> GetClaimsAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            return new List<Claim>();

        return await _userManager.GetClaimsAsync(user);
    }

    public async Task<IList<Claim>> GetAllUserClaimsAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            return new List<Claim>();

        var claims = new List<Claim>();

        // Add user claims
        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        // Add roles and role claims
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                claims.AddRange(roleClaims);
            }
        }
        return claims;
    }
}

