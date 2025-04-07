using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.Manage
{
    public class Disable2faBase : ComponentBase
    {
        #region Fields

        private ApplicationUser user = default!;

        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] protected ILogger<Disable2fa> Logger { get; set; } = default!;

        #endregion Fields

        #region Properties

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            user = await UserAccessor.GetRequiredUserAsync(HttpContext);

            if (HttpMethods.IsGet(HttpContext.Request.Method) && !await UserManager.GetTwoFactorEnabledAsync(user))
            {
                throw new InvalidOperationException("Cannot disable 2FA for user as it's not currently enabled.");
            }
        }

        #endregion Protected Methods

        #region Private Methods

        public async Task OnSubmitAsync()
        {
            var disable2faResult = await UserManager.SetTwoFactorEnabledAsync(user, false);

            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException("Unexpected error occurred disabling 2FA.");
            }

            var userId = await UserManager.GetUserIdAsync(user);

            Logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", userId);

            RedirectManager.RedirectToWithStatus(
                "Account/Manage/TwoFactorAuthentication",
                "2fa has been disabled. You can reenable 2fa when you setup an authenticator app",
                HttpContext);
        }

        #endregion Private Methods
    }
}