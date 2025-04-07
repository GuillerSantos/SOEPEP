using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.Manage
{
    public class GenerateRecoveryCodesBase : ComponentBase
    {
        #region Fields

        public string? message;
        public ApplicationUser user = default!;
        public IEnumerable<string>? recoveryCodes;

        #endregion Fields

        #region Properties

        [CascadingParameter]
        public HttpContext HttpContext { get; set; } = default!;

        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] protected ILogger<GenerateRecoveryCodes> Logger { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            user = await UserAccessor.GetRequiredUserAsync(HttpContext);

            var isTwoFactorEnabled = await UserManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                throw new InvalidOperationException("Cannot generate recovery codes for user because they do not have 2FA enabled.");
            }
        }

        #endregion Protected Methods

        #region Private Methods

        public async Task OnSubmitAsync()
        {
            var userId = await UserManager.GetUserIdAsync(user);
            recoveryCodes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            message = "You have generated new recovery codes.";

            Logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);
        }

        #endregion Private Methods
    }
}