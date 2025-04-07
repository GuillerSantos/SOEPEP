using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SOEPEP.Data;
using System.Text.Encodings.Web;
using System.Text;
using SOEPEP.Application.DTOs;

namespace SOEPEP.Components.Account.Pages.AuthPages
{
    public class RegisterBase : ComponentBase
    {
        #region Fields

        private IEnumerable<IdentityError>? identityErrors;

        #endregion Fields

        #region Properties

        [SupplyParameterFromForm]
        public RegisterInputDto Input { get; set; } = new();

        public string? Message => identityErrors is null ? null : $"Error: {string.Join(", ", identityErrors.Select(error => error.Description))}";

        [SupplyParameterFromQuery]
        public string? ReturnUrl { get; set; }

        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected IUserStore<ApplicationUser> UserStore { get; set; }
        [Inject] protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject] protected IEmailSender<ApplicationUser> EmailSender { get; set; } = default!;
        [Inject] protected ILogger<Register> Logger { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;

        #endregion Properties

        #region Public Methods

        public async Task RegisterUser(EditContext editContext)
        {
            var user = CreateUser();

            await UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            var emailStore = GetEmailStore();
            await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await UserManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                identityErrors = result.Errors;
                return;
            }

            Logger.LogInformation("User created a new account with password.");

            var userId = await UserManager.GetUserIdAsync(user);
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });

            await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            if (UserManager.Options.SignIn.RequireConfirmedAccount)
            {
                RedirectManager.RedirectTo(
                    "Account/RegisterConfirmation",
                    new() { ["email"] = Input.Email, ["returnUrl"] = ReturnUrl });
            }

            await SignInManager.SignInAsync(user, isPersistent: false);
            RedirectManager.RedirectTo(ReturnUrl);
        }

        #endregion Public Methods

        #region Private Methods

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!UserManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)UserStore;
        }

        #endregion Private Methods
    }
}