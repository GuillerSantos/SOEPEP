using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using SOEPEP.Data;
using SOEPEP.Application.DTOs;

namespace SOEPEP.Components.Account.Pages.AuthPages
{
    public class ResendEmailConfirmationBase : ComponentBase
    {
        #region Fields

        public string? message;

        #endregion Fields

        #region Properties

        [SupplyParameterFromForm]
        public ResendEmailConfirmationInputDto Input { get; set; } = new();

        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected IEmailSender<ApplicationUser> EmailSender { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;

        #endregion Properties

        #region Public Methods

        public async Task OnValidSubmitAsync()
        {
            var user = await UserManager.FindByEmailAsync(Input.Email!);
            if (user is null)
            {
                message = "Verification email sent. Please check your email.";
                return;
            }

            var userId = await UserManager.GetUserIdAsync(user);
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });
            await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            message = "Verification email sent. Please check your email.";
        }

        #endregion Public Methods
    }
}