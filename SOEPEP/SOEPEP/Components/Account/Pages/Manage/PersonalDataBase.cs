using Microsoft.AspNetCore.Components;

namespace SOEPEP.Components.Account.Pages.Manage
{
    public class PersonalDataBase : ComponentBase
    {
        #region Properties

        [Inject] protected IdentityUserAccessor UserAccessor { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        #endregion Properties

        #region Protected Methods

        protected override async Task OnInitializedAsync()
        {
            _ = await UserAccessor.GetRequiredUserAsync(HttpContext);
        }

        #endregion Protected Methods
    }
}