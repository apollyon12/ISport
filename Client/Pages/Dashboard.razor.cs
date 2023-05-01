using System.Net.Http.Json;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace iSportsRecruiting.Client.Pages
{
    public partial class Dashboard
    {
        [Parameter]
        public int ProductCount { get; set; }

        [Parameter]
        public int BrandCount { get; set; }

        [Parameter]
        public int UserCount { get; set; }

        [Parameter]
        public int RoleCount { get; set; }

        AthleteDTO Athlete { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

            // hubConnection = new HubConnectionBuilder()
            // .WithUrl(_navigationManager.ToAbsoluteUri("/signalRHub"))
            // .WithUrl(_navigationManager.ToAbsoluteUri("/signalRHub"))
            // .Build();
            // hubConnection.On("UpdateDashboard", async () =>
            // {
            //     await LoadDataAsync();
            //     StateHasChanged();
            // });
            // await hubConnection.StartAsync();
        }

        private int _qualifyUniversities;
        private int _favoriteUniversities;
        private int _coachesViews;
        public int _emails;
        private async Task LoadDataAsync()
        {
            Athlete = (await _localStorage.GetItemAsync<AccountDto>("account")).Athlete;

            var responseQualification = await _httpClient.GetFromJsonAsync<Response<UniversityModel[]>>("api/v1/university/qualify?" +
               $"sportName={Athlete.SportName}&" +
               $"gpa={Athlete.GPA}");

            _qualifyUniversities = responseQualification?.Payload?.Length ?? 0;

            var responseFavorites = await _httpClient.GetFromJsonAsync<Response<IEnumerable<int>>>($"api/v1/university/favorites/{Athlete.Id}");
            _favoriteUniversities = responseFavorites?.Payload?.Count() ?? 0;

            var responseCoachViews = await _httpClient.GetFromJsonAsync<Response<int>>($"api/v1/coach/views/{Athlete.Id}/count");
            _coachesViews = responseCoachViews?.Payload ?? 0;

            var responseEmails = await _httpClient.GetFromJsonAsync<Response<int>>($"api/v1/email/receive/{Athlete.Id}/count");
            _emails = responseEmails?.Payload ?? 0;
        }

        //[CascadingParameter] public HubConnection hubConnection { get; set; }
    }
}