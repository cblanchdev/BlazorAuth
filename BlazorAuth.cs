using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BlazorAuth;

public class BlazorAuth(SignInManager<IdentityUser> _signInManager, UserManager<IdentityUser> _userManager, ProtectedLocalStorage _localStorage, ProtectedSessionStorage _sessionStorage, IOptions<BlazorAuthOptions> _options) : AuthenticationStateProvider
{
    #region Properties

    public static ClaimsPrincipal Anonymous => new(new ClaimsIdentity());
    public ClaimsPrincipal User { get; set; } = Anonymous;

    #endregion

    #region Public Methods

    public async Task<SignInResult> SignIn(string email, string password)
    {
        IdentityUser? user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (result.Succeeded)
            {
                User = GetClaimsPrincipalFromIdentityUser(user);
                await SaveUserIdToStorage(user.Id);

                Task<AuthenticationState> authState = GetAuthenticationStateAsync();
                NotifyAuthenticationStateChanged(authState);
                return SignInResult.Success;
            }
        }

        return SignInResult.Failed;
    }

    public async Task SignOut()
    {
        User = Anonymous;
        if (_options.Value.StorageType == BlazorAuthOptions.eStorageType.Session)
        {
            await _sessionStorage.DeleteAsync("BlazorAuthUser");
        }
        else if (_options.Value.StorageType == BlazorAuthOptions.eStorageType.Local)
        {
            await _localStorage.DeleteAsync("BlazorAuthUser");
        }
        ;

        Task<AuthenticationState> authState = GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(authState);
    }

    #endregion

    #region AuthenticationStateProvider Implementation

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string userId = await GetUserIdFromStorage();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            IdentityUser? sessionUser = await _userManager.FindByIdAsync(userId);
            if (sessionUser != null)
            {
                User = GetClaimsPrincipalFromIdentityUser(sessionUser);
                return new AuthenticationState(User);
            }
        }

        User = Anonymous;
        return new AuthenticationState(User);
    }

    #endregion

    #region Private Methods

    private ClaimsPrincipal GetClaimsPrincipalFromIdentityUser(IdentityUser user)
    {
        Claim id = new Claim("BlazorAuthId", user.Id);
        Claim email = new Claim(ClaimTypes.Email, user.Email ?? "");
        Claim username = new Claim(ClaimTypes.Name, user.UserName ?? "");
        return new ClaimsPrincipal(new ClaimsIdentity([id, email, username], "BlazorAuth"));
    }

    private async Task SaveUserIdToStorage(string userId)
    {
        if (_options.Value.StorageType == BlazorAuthOptions.eStorageType.Session)
        {
            await _sessionStorage.SetAsync("BlazorAuthUser", userId);
        }
        else if (_options.Value.StorageType == BlazorAuthOptions.eStorageType.Local)
        {
            await _localStorage.SetAsync("BlazorAuthUser", userId);
        }
    }

    private async Task<string> GetUserIdFromStorage()
    {
        try
        {
            if (_options.Value.StorageType == BlazorAuthOptions.eStorageType.Session)
            {
                return (await _sessionStorage.GetAsync<string>("BlazorAuthUser")).Value ?? "";
            }
            else if (_options.Value.StorageType == BlazorAuthOptions.eStorageType.Local)
            {
                return (await _localStorage.GetAsync<string>("BlazorAuthUser")).Value ?? "";
            }
        }
        catch (Exception) { }

        return string.Empty;
    }

    #endregion
}