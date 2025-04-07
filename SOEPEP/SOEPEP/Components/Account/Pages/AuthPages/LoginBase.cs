using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using SOEPEP.Application.DTOs;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.AuthPages
{
    public class LoginBase : ComponentBase
    {
        #region Properties

        public string? errorMessage;
        [Inject] public IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] public SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject] public NavigationManager NavigationManager { get; set; } = default!;
        [Inject] public ILogger<Login> Logger { get; set; } = default!;

        [SupplyParameterFromForm]
        public LoginInputDto Input { get; set; } = new LoginInputDto();

        [CascadingParameter]
        public HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromQuery]
        public string? ReturnUrl { get; set; }

        public async Task LoginUser()
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await SignInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                Logger.LogInformation("User logged in.");
                RedirectManager.RedirectTo(ReturnUrl);
            }
            else if (result.RequiresTwoFactor)
            {
                RedirectManager.RedirectTo(
                    "Account/LoginWith2fa",
                    new() { ["returnUrl"] = ReturnUrl, ["rememberMe"] = Input.RememberMe });
            }
            else if (result.IsLockedOut)
            {
                Logger.LogWarning("User account locked out.");
                RedirectManager.RedirectTo("Account/Lockout");
            }
            else
            {
                errorMessage = "Error: Invalid login attempt.";
            }
        }

        protected override async Task OnInitializedAsync()
        {
            if (HttpMethods.IsGet(HttpContext.Request.Method))
            {
                // Clear the existing external cookie to ensure a clean login process
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            }
        }

        #endregion Properties
    }
}