using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.Manage
{
    public class TwoFactorAuthenticationBase : ComponentBase
    {
        #region Fields

        public bool canTrack;
        public bool hasAuthenticator;
        public int recoveryCodesLeft;
        public bool is2faEnabled;
        public bool isMachineRemembered;

        #endregion Fields

        #region Properties

        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject] protected IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            var user = await UserAccessor.GetRequiredUserAsync(HttpContext);

            canTrack = HttpContext.Features.Get<ITrackingConsentFeature>()?.CanTrack ?? true;

            hasAuthenticator = await UserManager.GetAuthenticatorKeyAsync(user) is not null;

            is2faEnabled = await UserManager.GetTwoFactorEnabledAsync(user);

            isMachineRemembered = await SignInManager.IsTwoFactorClientRememberedAsync(user);

            recoveryCodesLeft = await UserManager.CountRecoveryCodesAsync(user);
        }

        #endregion Protected Methods

        #region Private Methods

        public async Task OnSubmitForgetBrowserAsync()
        {
            await SignInManager.ForgetTwoFactorClientAsync();

            RedirectManager.RedirectToCurrentPageWithStatus(
                "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.",
                HttpContext);
        }

        #endregion Private Methods
    }
}