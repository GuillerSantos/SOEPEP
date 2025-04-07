using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using SOEPEP.Data;
using SOEPEP.Application.DTOs;

namespace SOEPEP.Components.Account.Pages.AuthPages
{
    public class ForgotPasswordBase : ComponentBase
    {


        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected IEmailSender<ApplicationUser> EmailSender { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;

        [SupplyParameterFromForm]
        public ForgotPasswordInputDto Input { get; set; } = new();

        public async Task OnValidSubmitAsync()
        {
            var user = await UserManager.FindByEmailAsync(Input.Email);
            if (user is null || !(await UserManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                RedirectManager.RedirectTo("Account/ForgotPasswordConfirmation");
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await UserManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ResetPassword").AbsoluteUri,
                new Dictionary<string, object?> { ["code"] = code });

            await EmailSender.SendPasswordResetLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            RedirectManager.RedirectTo("Account/ForgotPasswordConfirmation");
        }
    }
}
