using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using SOEPEP.Application.DTOs;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.Manage
{
    public class ChangePasswordBase : ComponentBase
    {
        #region Fields

        public string? message;
        private ApplicationUser user = default!;
        private bool hasPassword;
        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject] protected IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] protected ILogger<ChangePassword> Logger { get; set; } = default!;

        #endregion Fields

        #region Properties

        [SupplyParameterFromForm]
        public ChangePasswordInputDto Input { get; set; } = new();

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            hasPassword = await UserManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                RedirectManager.RedirectTo("Account/Manage/SetPassword");
            }
        }

        #endregion Protected Methods

        #region Private Methods

        public async Task OnValidSubmitAsync()
        {
            var changePasswordResult = await UserManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                message = $"Error: {string.Join(",", changePasswordResult.Errors.Select(error => error.Description))}";
                return;
            }

            await SignInManager.RefreshSignInAsync(user);
            Logger.LogInformation("User changed their password successfully.");

            RedirectManager.RedirectToCurrentPageWithStatus("Your password has been changed", HttpContext);
        }

        #endregion Private Methods
    }
}