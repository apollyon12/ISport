using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using MudBlazor;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Client.Shared.Utils;
using iSportsRecruiting.Client.Shared.Dialogs;
using Microsoft.JSInterop;

namespace iSportsRecruiting.Client.Shared
{
    public partial class MainLayout
    {
        private long CurrentUserId { get; set; }

        private async Task LoadDataAsync()
        {
            _account = await LocalStorage.GetItemAsync<AccountDto>("account");

            if (_account is null)
                _navigationManager.NavigateTo("login");

            if (!_account.Notifications.Any())
            {
                if (_account.Type == UserType.Athlete)
                {
                    if (_account.Athlete.PercentCompletionProfile < 99)
                    {
                        var notification = new Notification
                        {
                            Title = "Incomplete Profile",
                            Body =
                                $"Please complete your profile, it's at {_account.Athlete.PercentCompletionProfile}%",
                            Link = "athlete/profile",
                            Type = "none"
                        };

                        notification.SetIcon(Icons.Filled.Info);

                        _account.Notifications.Add(notification);

                        await LocalStorage.SetItemAsync("LastNotification",
                            _account.Notifications.First(n => n.Guid == notification.Guid));
                        await LocalStorage.SetItemAsync("account", _account);
                    }
                }
            }
        }
        private async Task ChangeTheme()
        {
            if (_themeName == "light")
            {
                _currentTheme = ThemeDark;
                _themeName = "dark";
            }
            else
            {
                _currentTheme = ThemeLight;
                _themeName = "light";
            }
            await LocalStorage.SetItemAsStringAsync("theme", _themeName);
        }

        private AccountDto? _account;

        private bool _drawerOpen = true;

        //private HubConnection _hubConnection;
        private Notification _lastNotification;

        protected override async Task OnInitializedAsync()
        {
            _account = await LocalStorage.GetItemAsync<AccountDto>("account");
            
            _ = _pushMessage.SendAsync("Welcome to ISR!", "dashboard", "images/ronnie.png");

            if (_account is null)
                _navigationManager.NavigateTo("login");

            if (await LocalStorage.ContainKeyAsync("theme"))
                _themeName = await LocalStorage.GetItemAsStringAsync("theme");
            else
                _themeName = "light";

            _currentTheme = _themeName == "light" ? ThemeLight : ThemeDark;

            _ = Task.Run(async () =>
            {
                var notificationsResponse =
                    await Http.GetFromJsonAsync<Response<Notification[]>>($"api/v1/hub/{_account.Id}");
                if (notificationsResponse is not null && notificationsResponse.Payload.Any())
                {
                    foreach (var notification in notificationsResponse.Payload)
                    {
                        if (!_account.Notifications.Any(n => n.Id == notification.Id))
                        {
                            _account.Notifications.Add(notification);
                            _ = _pushMessage.SendAsync(notification.Title, notification.Link, notification.Image);
                        }
                    }

                    _account.Notifications = _account.Notifications.GroupBy(n => n.External_Relate_Id).Select(a =>
                    {
                        var notification = a.OrderByDescending(a => a.Id).First();
                        notification.Count = a.Count();
                        return notification;
                    }).ToList();

                    await LocalStorage.SetItemAsync("account", _account);
                    StateHasChanged();
                }
            });
        }

        public async Task GoBack()
        {
            await _jsRuntime.InvokeVoidAsync("goBack");
        }

        private void ShowReferralUrl()
        {
            _dialogService.Show<ReferralUrl>();
        }

        private void Logout()
        {
            string logoutConfirmationText = "Logout Confirmation";
            string logoutText = "Logout";

            var parameters = new DialogParameters
            {
                {"ContentText", logoutConfirmationText},
                {"ButtonText", logoutText},
                {"Color", Color.Error}
            };

            _dialogService.Show<Logout>("Logout", parameters);
        }

        private void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }

        private async Task DeleteNotificationAsync(Notification notification, bool navigate = true)
        {
            if (notification.Type == "none")
            {
                _navigationManager.NavigateTo(notification.Link);
                return;
            }

            int id = notification.Id;

            _account.Notifications = _account.Notifications
                .Where(x => x.External_Relate_Id != notification.External_Relate_Id).ToList();
            StateHasChanged();

            await LocalStorage.SetItemAsync("account", _account);

            _ = Http.DeleteAsync($"api/v1/hub/{notification.External_Relate_Id}/{notification.Entity_Id}");

            if (navigate)
                _navigationManager.NavigateTo(notification.Link);
        }

        public Task OpenConversationAsync(Notification notification)
        {
            //_dialogService.Show<SendEmail>("Send Email",
            //    new DialogParameters
            //    {
            //        ["EmailId"] = notification.External_Relate_Id
            //    },
            //    new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

            _navigationManager.NavigateTo("chat/conversations");

            return DeleteNotificationAsync(notification, false);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            StateManagement<AccountDto>.AddRender("ImageProfile", account =>
            {
                _account = account;
                StateHasChanged();
            }, GetHashCode());
        }
    }

    public static class Nav
    {
        public static string CurrentLocation;
        public static string LastLocation;
    }
}