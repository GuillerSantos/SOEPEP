using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SOEPEP.Application.DTOs;
using SOEPEP.Data;
using System.Text;
using System.Text.Encodings.Web;

namespace SOEPEP.Components.Account.Pages.Manage
{
    public class EmailBase : ComponentBase
    {
        #region Properties

        [CascadingParameter]
        [SupplyParameterFromForm(FormName = "change-email")]
        public EmailInputDto Input { get; set; } = new();

        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;

        [Inject] protected IEmailSender<ApplicationUser> EmailSender { get; set; } = default!;

        [Inject] protected IdentityUserAccessor UserAccessor { get; set; } = default!;

        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        #endregion Properties

        #region Fields

        public string? message;
        public ApplicationUser user = default!;
        public string? email;
        public bool isEmailConfirmed;

        #endregion Fields

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            email = await UserManager.GetEmailAsync(user);
            isEmailConfirmed = await UserManager.IsEmailConfirmedAsync(user);

            Input.NewEmail ??= email;
        }

        #endregion Protected Methods

        #region Private Methods

        public async Task OnValidSubmitAsync()
        {
            if (Input.NewEmail is null || Input.NewEmail == email)
            {
                message = "Your email is unchanged.";
                return;
            }

            var userId = await UserManager.GetUserIdAsync(user);

            var code = await UserManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmailChange").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["email"] = Input.NewEmail, ["code"] = code });

            await EmailSender.SendConfirmationLinkAsync(user, Input.NewEmail, HtmlEncoder.Default.Encode(callbackUrl));

            message = "Confirmation link to change email sent. Please check your email.";
        }

        public async Task OnSendEmailVerificationAsync()
        {
            if (email is null)
            {
                return;
            }

            var userId = await UserManager.GetUserIdAsync(user);
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });

            await EmailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));

            message = "Verification email sent. Please check your email.";
        }

        #endregion Private Methods
    }
}