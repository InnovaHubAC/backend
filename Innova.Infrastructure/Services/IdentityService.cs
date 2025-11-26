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

    public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return false;
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public Task<string> GetUserNameByEmailAsync(string email)
    {
        return _userManager.Users
            .Where(u => u.Email == email)
            .Select(u => u.UserName!)
            .FirstOrDefaultAsync()!;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return string.Empty;

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<bool> IsEmailConfirmedAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        return await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<bool> UserExistsAsync(string id)
    {
        return await _userManager.FindByIdAsync(id) is not null;
    }
    public async Task<(string FirstName, string LastName, string UserName)?> GetUserForIdeaAsync(string userId)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.FirstName, u.LastName, u.UserName })
            .FirstOrDefaultAsync();

        if (user == null)
            return null;

        return (user.FirstName!, user.LastName!, user.UserName!);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return string.Empty;

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    public async Task<(bool Success, List<string> Errors)> CreateExternalUserAsync(string email, string userName, string firstName, string lastName, string provider, string providerKey)
    {
        var errors = new List<string>();

        var user = new AppUser
        {
            Email = email,
            UserName = userName,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            errors.AddRange(result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        var info = new UserLoginInfo(provider, providerKey, provider);
        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            errors.AddRange(addLoginResult.Errors.Select(e => e.Description));
            await _userManager.DeleteAsync(user);
            return (false, errors);
        }

        return (true, errors);
    }

    public async Task<string?> GetUserNameByProviderAsync(string provider, string providerKey)
    {
        var user = await _userManager.FindByLoginAsync(provider, providerKey);
        return user?.UserName;
    }

    public async Task<(IReadOnlyList<(string Id, string? UserName, string? Email, string? FirstName,
     string? LastName, DateTime? DateOfBirth, bool EmailConfirmed)> Items, int TotalCount)>
     GetUsersPagedAsync(int pageIndex, int pageSize, string? sort)
    {
        var query = _userManager.Users.AsQueryable();

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrEmpty(sort))
        {
            switch (sort.ToLower())
            {
                case "usernameasc":
                    query = query.OrderBy(u => u.UserName);
                    break;
                case "usernamedesc":
                    query = query.OrderByDescending(u => u.UserName);
                    break;
                case "emailasc":
                    query = query.OrderBy(u => u.Email);
                    break;
                case "emaildesc":
                    query = query.OrderByDescending(u => u.Email);
                    break;
                default:
                    query = query.OrderBy(u => u.UserName);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(u => u.UserName);
        }

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.FirstName,
                u.LastName,
                u.DateOfBirth,
                u.EmailConfirmed
            })
            .ToListAsync();

        var resultItems = items.Select(u => (u.Id, u.UserName, u.Email, u.FirstName,
         u.LastName, u.DateOfBirth, u.EmailConfirmed)).ToList();

        return (resultItems, totalCount);
    }

    public async Task<bool> UpdateUserAsync(string userId, string firstName, string lastName, DateTime? dateOfBirth)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.FirstName = firstName;
        user.LastName = lastName;
        
        if(user.DateOfBirth.HasValue)
            user.DateOfBirth = dateOfBirth;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<(string Id, string? UserName, string? Email, string? FirstName,
     string? LastName, DateTime? DateOfBirth, bool EmailConfirmed)?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.FirstName,
                u.LastName,
                u.DateOfBirth,
                u.EmailConfirmed
            })
            .FirstOrDefaultAsync();

        if (user == null) return null;

        return (user.Id, user.UserName, user.Email, user.FirstName, user.LastName,
         user.DateOfBirth, user.EmailConfirmed);
    }
}

