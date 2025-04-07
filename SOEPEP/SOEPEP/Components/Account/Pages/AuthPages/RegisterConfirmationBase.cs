using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SOEPEP.Data;
using System.Text;

namespace SOEPEP.Components.Account.Pages.AuthPages
{
    public class RegisterConfirmationBase : ComponentBase
    {
        #region Fields

        public string? emailConfirmationLink;
        public string? statusMessage;

        #endregion Fields

        #region Properties

        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected IEmailSender<ApplicationUser> EmailSender { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromQuery]
        private string? Email { get; set; }

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            if (Email is null)
            {
                RedirectManager.RedirectTo("");
            }

            var user = await UserManager.FindByEmailAsync(Email);
            if (user is null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                statusMessage = "Error finding user for unspecified email";
            }
            else if (EmailSender is IdentityNoOpEmailSender)
            {
                // Once you add a real email sender, you should remove this code that lets you confirm the account
                var userId = await UserManager.GetUserIdAsync(user);
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                emailConfirmationLink = NavigationManager.GetUriWithQueryParameters(
                    NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                    new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });
            }
        }

        #endregion Protected Methods
    }
}