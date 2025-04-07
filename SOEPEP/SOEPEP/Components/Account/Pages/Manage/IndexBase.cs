using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using SOEPEP.Application.DTOs;
using SOEPEP.Data;

namespace SOEPEP.Components.Account.Pages.Manage
{
    public class IndexBase : ComponentBase
    {
        #region Fields

        public ApplicationUser user = default!;
        public string? username;
        public string? phoneNumber;

        #endregion Fields

        #region Properties

        [SupplyParameterFromForm]
        public IndexInputDto Input { get; set; } = new();

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
            user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            username = await UserManager.GetUserNameAsync(user);
            phoneNumber = await UserManager.GetPhoneNumberAsync(user);

            Input.PhoneNumber ??= phoneNumber;
        }

        #endregion Protected Methods

        #region Private Methods

        public async Task OnValidSubmitAsync()
        {
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await UserManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    RedirectManager.RedirectToCurrentPageWithStatus("Error: Failed to set phone number.", HttpContext);
                }
            }

            await SignInManager.RefreshSignInAsync(user);
            RedirectManager.RedirectToCurrentPageWithStatus("Your profile has been updated", HttpContext);
        }

        #endregion Private Methods
    }
}