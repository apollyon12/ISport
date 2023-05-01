using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace iSportsRecruiting.Client.Pages.Identity
{
    public partial class Forgot
    {
        private bool success;
        private string[] errors = { };
        private MudForm form;

        [Parameter]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        private async Task SubmitAsync()
        {
            var response = await _httpClient.GetAsync($"/api/v1/account/forgot/{Email}");

            if (response.IsSuccessStatusCode)
            {
                _snackBar.Add("Please check your email", Severity.Success);
                success = true;
            }
            else
            {
                _snackBar.Add("We can't reset your password, please contact the system administrator", Severity.Error);
            }
        }
    }
}