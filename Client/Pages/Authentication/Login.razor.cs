using System.Net.Http.Headers;
using System.Net.Http.Json;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Security;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace iSportsRecruiting.Client.Pages.Authentication
{
    public partial class Login
    {
        public string Email { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await _localStorage.RemoveItemAsync("account");
            
            var rememberEmail = await _localStorage.GetItemAsStringAsync("rememberEmail");
            var rememberPassword = await _localStorage.GetItemAsStringAsync("rememberPassword");
            if (!string.IsNullOrWhiteSpace(rememberEmail))
            {
                RememberMe = true;
                Email = rememberEmail;
                Password = rememberPassword;
            }
        }

        private void SubmitAsync()
        {
            IsLoading = true;
            StateHasChanged();

            Task.Run(async () =>
            {
                var result = await LogIn();

                if (result.Success)
                {
                    if (RememberMe)
                    {
                        await _localStorage.SetItemAsStringAsync("rememberEmail", Email);
                        await _localStorage.SetItemAsStringAsync("rememberPassword", Password);
                    }
                    else
                    {
                        await _localStorage.RemoveItemAsync("rememberEmail");
                        await _localStorage.RemoveItemAsync("rememberPassword");
                    }
                    
                    var account = await _localStorage.GetItemAsync<AccountDto>("account");

                    if (account.ShouldResetPassword)
                    {
                        await _localStorage.SetItemAsync("email", Email);
                        _snackBar.Add("Password should be update", Severity.Warning,
                            options => options.HideTransitionDuration = 10000);
                        _navigationManager.NavigateTo($"/login/password/create");
                        return;
                    }

                    _navigationManager.NavigateTo("/dashboard");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(result.Message))
                        _snackBar.Add(result.Message, Severity.Error);

                    IsLoading = false;
                    StateHasChanged();
                }
            });
        }

        private async Task<LoginResult> LogIn()
        {
            if(string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                return await LoginResult.FailAsync("Please fill all the fields");
            
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var message = await _httpClient.PostAsJsonAsync($"api/v1/account/login",
                new LoginInfoDTO
                {
                    Email = Email, Cipher = Cripto.EncryptPassword(Password)
                });

            var response = await message.Content.ReadFromJsonAsync<Response<AccountDto>>();

            if (!message.IsSuccessStatusCode)
                return await LoginResult.FailAsync("Unexpected error");

            if (response is not null && response.Status != ResponseStatus.Ok)
                return await LoginResult.FailAsync(response.Message);

            await _localStorage.SetItemAsync("account", response.Payload);

            if (response.Payload.ShouldResetPassword)
            {
                return await LoginResult.SuccessAsync();
            }

            await _localStorage.SetItemAsync("authToken", response.Payload.Token);
            await _localStorage.SetItemAsync("refreshToken", response.Payload.RefreshToken);


            if (response.Payload.Type == UserType.Athlete &&
                !string.IsNullOrEmpty(response.Payload.Athlete.ImageProfile))
                await _localStorage.SetItemAsync("userImageURL", response.Payload.Athlete.ImageProfile);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", response.Payload.Token);

            return await LoginResult.SuccessAsync();
        }

        public void SubmitByEnterAsync(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
                SubmitAsync();
        }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static Task<LoginResult> SuccessAsync()
        {
            var result = new LoginResult
            {
                Success = true
            };

            return Task.FromResult(result);
        }

        public static Task<LoginResult> FailAsync(string message)
        {
            var result = new LoginResult
            {
                Message = message
            };

            return Task.FromResult(result);
        }
    }
}