using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Security;
using MudBlazor;

namespace iSportsRecruiting.Client.Pages.Authentication
{
    public partial class RegisterCoach
    {
        public bool _creatingAccount;

        private async Task SubmitAsync()
        {
            if (_confirmEmail is null)
            {
                CoachName = $"{Model.FirstName} {Model.LastName}";

                _confirmEmail = new();
                _confirmEmail.Full_Name = CoachName;
                _confirmEmail.Email = Model.Email;
                _confirmEmail.Sport_Id = Model.SportId;
                _confirmEmail.University_Id = Model.UniversityId;
                _confirmEmail.Contact_Id = SportContact?.Id ?? 0;
                _confirmEmail.Is_Coach = Model.IsCoach;
                _confirmEmail.Cipher = Cripto.EncryptPassword(_password);
                
                if (await CheckLastStep())
                {
                    IsLoading = true;
                    StateHasChanged();

                    await _httpClient.PostAsJsonAsync("api/v1/account/register/confirmation/create", _confirmEmail);

                    UpdateContact = false;
                    _hiddeRegisterBtn = true;
                    IsLoading = false;
                    StateHasChanged();
                }
            }
            else
            {
                IsLoading = true;
                _creatingAccount = true;
                StateHasChanged();

                var response = await _httpClient.PostAsJsonAsync("/api/v1/account/register/confirmation", _confirmEmail);

                if (response.IsSuccessStatusCode)
                {
                    var coachResponse = await response.Content.ReadFromJsonAsync<Response<CoachDTO>>();

                    if (coachResponse is not null)
                    {
                        if (coachResponse.Status == ResponseStatus.Ok)
                        {
                            _navigationManager.NavigateTo("/login");
                            _snackBar.Add("Successfully registered, please log in", Severity.Success);

                            return;
                        }

                        IsLoading = false;
                        StateHasChanged();

                        _snackBar.Add(coachResponse.Message, Severity.Error, options => options.VisibleStateDuration = 10000);
                    }
                }

                IsLoading = false;
                _creatingAccount = false;
                StateHasChanged();
            }
        }

        private string _password;
        private string _confirmPassword;
        private MudTextField<string> _passwordField;

        private IEnumerable<string> PasswordStrength(string pw)
        {
            if (string.IsNullOrWhiteSpace(pw))
            {
                yield return "Password is required!";
                yield break;
            }

            if (pw.Length < 8)
                yield return "Password must be at least of length 8";
            if (!Regex.IsMatch(pw, @"[A-Z]"))
                yield return "Password must contain at least one capital letter";
            if (!Regex.IsMatch(pw, @"[a-z]"))
                yield return "Password must contain at least one lowercase letter";
            if (!Regex.IsMatch(pw, @"[0-9]"))
                yield return "Password must contain at least one digit";
        }

        private string PasswordMatch(string arg)
        {
            if (_password != arg)
                return "Passwords don't match";

            if (_confirmEmail is null)
            {
                //Model.Cipher = Cripto.EncryptPassword(_password);
            }
            else
            {
                //_confirmEmail.Cipher = Cripto.EncryptPassword(_password);
            }

            return null;
        }

        private bool _passwordVisibility;
        private InputType _passwordInput = InputType.Password;
        private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

        private void TogglePasswordVisibility()
        {
            if (_passwordVisibility)
            {
                _passwordVisibility = false;
                _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
                _passwordInput = InputType.Password;
            }
            else
            {
                _passwordVisibility = true;
                _passwordInputIcon = Icons.Material.Filled.Visibility;
                _passwordInput = InputType.Text;
            }
        }
    }
}
