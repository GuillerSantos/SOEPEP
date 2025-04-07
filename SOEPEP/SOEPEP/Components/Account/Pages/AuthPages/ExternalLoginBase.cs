using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SOEPEP.Application.DTOs;
using SOEPEP.Components.Account;
using SOEPEP.Data;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

public class ExternalLoginBase : ComponentBase
{
    public const string LoginCallbackAction = "LoginCallback";

    [Inject] protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
    [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] protected IUserStore<ApplicationUser> UserStore { get; set; } = default!;
    [Inject] protected IEmailSender<ApplicationUser> EmailSender { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected IdentityRedirectManager RedirectManager { get; set; } = default!;
    [Inject] protected ILogger<ExternalLoginBase> Logger { get; set; } = default!;

    [SupplyParameterFromForm]
    protected LoginInputDto Input { get; set; } = new LoginInputDto();
    protected string? Message;
    protected ExternalLoginInfo? ExternalLoginInfo;

    [CascadingParameter] protected HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromQuery] protected string? RemoteError { get; set; }
    [SupplyParameterFromQuery] protected string? ReturnUrl { get; set; }
    [SupplyParameterFromQuery] protected string? Action { get; set; }

    protected string? ProviderDisplayName => ExternalLoginInfo?.ProviderDisplayName;

    protected override async Task OnInitializedAsync()
    {
        if (RemoteError is not null)
        {
            RedirectManager.RedirectToWithStatus("Account/Login", $"Error from external provider: {RemoteError}", HttpContext);
            return;
        }

        ExternalLoginInfo = await SignInManager.GetExternalLoginInfoAsync();
        if (ExternalLoginInfo is null)
        {
            RedirectManager.RedirectToWithStatus("Account/Login", "Error loading external login information.", HttpContext);
            return;
        }

        if (HttpMethods.IsGet(HttpContext.Request.Method) && Action == LoginCallbackAction)
        {
            await OnLoginCallbackAsync();
        }
    }

    protected async Task OnLoginCallbackAsync()
    {
        if (ExternalLoginInfo is null)
        {
            RedirectManager.RedirectToWithStatus("Account/Login", "Error loading external login information.", HttpContext);
            return;
        }

        var result = await SignInManager.ExternalLoginSignInAsync(
            ExternalLoginInfo.LoginProvider, ExternalLoginInfo.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded)
        {
            Logger.LogInformation("{Name} logged in with {LoginProvider} provider.", ExternalLoginInfo.Principal.Identity?.Name, ExternalLoginInfo.LoginProvider);
            RedirectManager.RedirectTo(ReturnUrl);
            return;
        }
        if (result.IsLockedOut)
        {
            RedirectManager.RedirectTo("Account/Lockout");
            return;
        }

        if (ExternalLoginInfo.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            Input.Email = ExternalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
        }
    }

    protected async Task OnValidSubmitAsync()
    {
        if (ExternalLoginInfo is null)
        {
            RedirectManager.RedirectToWithStatus("Account/Login", "Error loading external login information during confirmation.", HttpContext);
            return;
        }

        var user = CreateUser();
        await UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
        await ((IUserEmailStore<ApplicationUser>)UserStore).SetEmailAsync(user, Input.Email, CancellationToken.None);

        var result = await UserManager.CreateAsync(user);
        if (result.Succeeded)
        {
            result = await UserManager.AddLoginAsync(user, ExternalLoginInfo);
            if (result.Succeeded)
            {
                Logger.LogInformation("User created an account using {Name} provider.", ExternalLoginInfo.LoginProvider);

                var userId = await UserManager.GetUserIdAsync(user);
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                    NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                    new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });

                await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

                if (UserManager.Options.SignIn.RequireConfirmedAccount)
                {
                    RedirectManager.RedirectTo("Account/RegisterConfirmation", new() { ["email"] = Input.Email });
                }

                await SignInManager.SignInAsync(user, isPersistent: false, ExternalLoginInfo.LoginProvider);
                RedirectManager.RedirectTo(ReturnUrl);
                return;
            }
        }

        Message = $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}";
    }

    private ApplicationUser CreateUser()
    {
        return Activator.CreateInstance<ApplicationUser>()
            ?? throw new InvalidOperationException
            ("Can't create an instance of ApplicationUser.");
    }
}
