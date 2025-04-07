using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SOEPEP.Application.DTOs;
using SOEPEP.Data;
using System.Text;

namespace SOEPEP.Components.Account.Pages.AuthPages
{
    public class ResetPasswordBase : ComponentBase
    {
        #region Fields

        private IEnumerable<IdentityError>? identityErrors;

        #endregion Fields

        #region Properties

        [SupplyParameterFromForm]
        public ResetPasswordInputDto Input { get; set; } = new();

        [SupplyParameterFromQuery]
        public string? Code { get; set; }

        public string? Message => identityErrors is null ? null : $"Error: {string.Join(", ", identityErrors.Select(error => error.Description))}";
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected override void OnInitialized()
        {
            if (Code is null)
            {
                RedirectManager.RedirectTo("Account/InvalidPasswordReset");
            }

            Input.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
        }

        #endregion Protected Methods

        #region Private Methods

        public async Task OnValidSubmitAsync()
        {
            var user = await UserManager.FindByEmailAsync(Input.Email);
            if (user is null)
            {
                // Don't reveal that the user does not exist
                RedirectManager.RedirectTo("Account/ResetPasswordConfirmation");
            }

            var result = await UserManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                RedirectManager.RedirectTo("Account/ResetPasswordConfirmation");
            }

            identityErrors = result.Errors;
        }

        #endregion Private Methods
    }
}