using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SOEPEP.Data;
using System.Text;

namespace SOEPEP.Components.Account.Pages.AuthPages
{
    public class ConfirmEmailBase : ComponentBase
    {

        [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;

        public string? statusMessage;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromQuery]
        private string? UserId { get; set; }

        [SupplyParameterFromQuery]
        private string? Code { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (UserId is null || Code is null)
            {
                RedirectManager.RedirectTo("");
            }

            var user = await UserManager.FindByIdAsync(UserId);
            if (user is null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                statusMessage = $"Error loading user with ID {UserId}";
            }
            else
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
                var result = await UserManager.ConfirmEmailAsync(user, code);
                statusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            }
        }
    }
}
