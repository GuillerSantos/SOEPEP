using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using SOEPEP.Application.DTOs;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.Manage
{
    public class SetPasswordBase : ComponentBase
    {
        #region Properties

        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject] protected IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;

        #endregion Properties

        #region Fields

        public string? message;
        private ApplicationUser user = default!;

        #endregion Fields



        #region Properties

        [SupplyParameterFromForm]
        public SetPasswordInputDto Input { get; set; } = new();

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            user = await UserAccessor.GetRequiredUserAsync(HttpContext);

            var hasPassword = await UserManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                RedirectManager.RedirectTo("Account/Manage/ChangePassword");
            }
        }

        #endregion Protected Methods

        #region Private Methods

        public async Task OnValidSubmitAsync()
        {
            var addPasswordResult = await UserManager.AddPasswordAsync(user, Input.NewPassword!);
            if (!addPasswordResult.Succeeded)
            {
                message = $"Error: {string.Join(",", addPasswordResult.Errors.Select(error => error.Description))}";
                return;
            }

            await SignInManager.RefreshSignInAsync(user);
            RedirectManager.RedirectToCurrentPageWithStatus("Your password has been set.", HttpContext);
        }

        #endregion Private Methods
    }
}