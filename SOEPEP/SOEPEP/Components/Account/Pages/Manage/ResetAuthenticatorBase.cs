using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.Manage
{
    public class ResetAuthenticatorBase : ComponentBase
    {
        #region Properties

        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject] protected IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] protected ILogger<ResetAuthenticator> Logger { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        #endregion Properties

        #region Private Methods

        public async Task OnSubmitAsync()
        {
            var user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            await UserManager.SetTwoFactorEnabledAsync(user, false);
            await UserManager.ResetAuthenticatorKeyAsync(user);
            var userId = await UserManager.GetUserIdAsync(user);
            Logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", userId);

            await SignInManager.RefreshSignInAsync(user);

            RedirectManager.RedirectToWithStatus(
                "Account/Manage/EnableAuthenticator",
                "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.",
                HttpContext);
        }

        #endregion Private Methods
    }
}