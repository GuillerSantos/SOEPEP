using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using SOEPEP.Application.DTOs;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.Manage
{
    public class DeletePersonalDataBase : ComponentBase
    {
        #region Fields

        public string? message;
        public bool requirePassword;
        private ApplicationUser user = default!;
        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject] protected IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] protected ILogger<DeletePersonalData> Logger { get; set; } = default!;

        #endregion Fields

        #region Properties

        [SupplyParameterFromForm]
        public DeletePersonalDataInputDto Input { get; set; } = new();

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            Input ??= new();
            user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            requirePassword = await UserManager.HasPasswordAsync(user);
        }

        #endregion Protected Methods

        #region Private Methods

        public async Task OnValidSubmitAsync()
        {
            if (requirePassword && !await UserManager.CheckPasswordAsync(user, Input.Password))
            {
                message = "Error: Incorrect password.";
                return;
            }

            var result = await UserManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Unexpected error occurred deleting user.");
            }

            await SignInManager.SignOutAsync();

            var userId = await UserManager.GetUserIdAsync(user);
            Logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            RedirectManager.RedirectToCurrentPage();
        }

        #endregion Private Methods
    }
}