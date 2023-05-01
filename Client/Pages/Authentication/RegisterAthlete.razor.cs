using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using iSportsRecruiting.Shared.DTO;
using MudBlazor;
using System.Net.Http;

namespace iSportsRecruiting.Client.Pages.Authentication
{
    public partial class RegisterAthlete
    {
        private async Task SubmitAsync()
        {
            IsLoading = true;
            StateHasChanged();

            _model.CreditCardTransaction = new CreditCardTransactionDTO
            {
                FirstName = _model.Info.FirstName,
                LastName = _model.Info.LastName,
                CardCode = _billing.Card_Code,
                CardNumber = _billing.Card_Number,
                Zip = _billing.Zip,
                ExpirationDate = _billing.Exp_Date
            };

            HttpResponseMessage response;

            response = await _httpClient.PostAsJsonAsync(
                $"/api/v1/account/register/athlete/{_billing.Plan}?isTempOffer={TempOffer != null}&promoCode={PromotionCode}", _model);

            if (response.IsSuccessStatusCode)
            {
                var athleteResponse = await response.Content.ReadFromJsonAsync<Response<AthleteDTO>>();

                if (athleteResponse is not null)
                {
                    if (athleteResponse?.Status == ResponseStatus.Ok)
                    {
                        if (Name is not null)
                        {
                            await _httpClient.GetAsync(
                                    $"/api/v1/account/referral/{Id}/{athleteResponse.Payload.Id}");
                        }
                        else if (Referral is not null)
                        {
                            var id = Referral.Substring(1, Referral.Length - 1);
                            if (int.TryParse(id, out int parsedId))
                            {
                                await _httpClient.GetAsync(
                                    $"/api/v1/account/referral/{parsedId}/{athleteResponse.Payload.Id}");
                            }
                            else
                            {
                                id = Referral.Substring(2, Referral.Length - 2);
                                
                                if (int.TryParse(id, out int parsedId2))
                                {
                                    await _httpClient.GetAsync(
                                        $"/api/v1/account/referral/{parsedId2}/{athleteResponse.Payload.Id}");
                                }
                            }
                        }

                        _navigationManager.NavigateTo("/login");
                        _snackBar.Add("Successfully registered, please log in", Severity.Success);

                        return;
                    }

                    IsLoading = false;
                    StateHasChanged();

                    _snackBar.Add(athleteResponse.Message, Severity.Error,
                        options => options.VisibleStateDuration = 10000);
                }
            }
        }

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

#pragma warning disable CS0649 // El campo 'Register.pwField' nunca se asigna y siempre tendrá el valor predeterminado null
        private MudTextField<string> pwField;
#pragma warning restore CS0649 // El campo 'Register.pwField' nunca se asigna y siempre tendrá el valor predeterminado null

        private string PasswordMatch(string arg)
        {
            if (pwField.Value != arg)
                return "Passwords don't match";
            return null;
        }

        private bool PasswordVisibility;
#pragma warning disable CS0414 // El campo 'Register.PasswordInput' está asignado pero su valor nunca se usa
        private InputType PasswordInput = InputType.Password;
#pragma warning restore CS0414 // El campo 'Register.PasswordInput' está asignado pero su valor nunca se usa
        private string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        private void TogglePasswordVisibility()
        {
            if (PasswordVisibility)
            {
                PasswordVisibility = false;
                PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                PasswordInput = InputType.Password;
            }
            else
            {
                PasswordVisibility = true;
                PasswordInputIcon = Icons.Material.Filled.Visibility;
                PasswordInput = InputType.Text;
            }
        }
    }
}