using iSportsRecruiting.Client.Shared.Utils;
using iSportsRecruiting.Shared.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace iSportsRecruiting.Client.Shared
{
    public partial class UserCard
    {
        [Parameter] public string Class { get; set; }
        [Parameter] public bool CutEmail { get; set; }

        [Parameter] public AccountDto? Account { get; set; }

        private string _temporaryUrl;

        protected override Task OnInitializedAsync()
        {
            return LoadDataAsync();
        }

        private bool _tutFinalized;

        private async Task LoadDataAsync()
        {
            Account = await LocalStorage.GetItemAsync<AccountDto>("account");
            if (Account.Type == UserType.Athlete)
            {
                _tutFinalized = !Account.Athlete.ShowTut;
                StateHasChanged();
            }
        }
    }
}