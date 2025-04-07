using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SOEPEP.Application.DTOs;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.AuthPages
{
    public class LoginWith2faBase : ComponentBase
    {
        #region Fields

        public string? message;
        private ApplicationUser user = default!;

        #endregion Fields

        #region Properties

        [SupplyParameterFromForm]
        public LoginWith2faInputDto Input { get; set; } = new();

        [SupplyParameterFromQuery]
        public string? ReturnUrl { get; set; }

        [SupplyParameterFromQuery]
        public bool RememberMe { get; set; }

        [Inject] protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] protected ILogger<LoginWith2fa> Logger { get; set; } = default!;

        #endregion Properties

        #region Public Methods

        public async Task OnValidSubmitAsync()
        {
            var authenticatorCode = Input.TwoFactorCode!.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await SignInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, RememberMe, Input.RememberMachine);
            var userId = await UserManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                Logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", userId);
                RedirectManager.RedirectTo(ReturnUrl);
            }
            else if (result.IsLockedOut)
            {
                Logger.LogWarning("User with ID '{UserId}' account locked out.", userId);
                RedirectManager.RedirectTo("Account/Lockout");
            }
            else
            {
                Logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", userId);
                message = "Error: Invalid authenticator code.";
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