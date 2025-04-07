using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SOEPEP.Application.DTOs;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.AuthPages
{
    public class LoginWithRecoveryCodeBase : ComponentBase
    {
        #region Fields

        public string? message;
        private ApplicationUser user;

        #endregion Fields

        #region Properties

        [SupplyParameterFromForm]
        public LoginWithRecoveryCodeInputDto Input { get; set; } = new();

        [Inject] protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] protected ILogger<LoginWithRecoveryCode> Logger { get; set; } = default!;

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        #endregion Properties

        #region Public Methods

        public async Task OnValidSubmitAsync()
        {
            var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

            var result = await SignInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            var userId = await UserManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                Logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", userId);
                RedirectManager.RedirectTo(ReturnUrl);
            }
            else if (result.IsLockedOut)
            {
                Logger.LogWarning("User account locked out.");
                RedirectManager.RedirectTo("Account/Lockout");
            }
            else
            {
                Logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", userId);
                message = "Error: Invalid recovery code entered.";
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            // Ensure the user has gone through the username & password screen first
            user = await SignInManager.GetTwoFactorAuthenticationUserAsync() ??
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        #endregion Protected Methods
    }
}