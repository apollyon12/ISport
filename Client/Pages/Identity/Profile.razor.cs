using iSportsRecruiting.Shared.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using iSportsRecruiting.Client.Shared.Utils;
using iSportsRecruiting.Shared.Constants;
using iSportsRecruiting.Shared.Models;
using Microsoft.JSInterop;

namespace iSportsRecruiting.Client.Pages.Identity
{
    public partial class Profile
    {
        private bool _loadedFlag;
        private AthleteDTO _athlete;

        [Parameter]
        public AthleteDTO Athlete
        {
            get => _athlete;
            set
            {
                _athlete = value;

                if (_athlete.Email is not null && !_loadedFlag)
                {
                    Height = _athlete.Height;
                    Weight = _athlete.Weight;

                    if (int.TryParse(_athlete.Birth, out int birth))
                        Date = new DateTime(birth, 1, 1);

                    _loadedFlag = true;
                }
            }
        }

        [Parameter] public EventCallback<AthleteDTO> AthleteChanged { get; set; }

        private char FirstLetterOfName { get; set; }

        private decimal? _height = 4;

        public decimal? Height
        {
            get => _height;
            set
            {
                if (value == 0 || value == null) return;

                if (value < 4)
                {
                    _height = 4.0M;
                    return;
                }
                if (value > 8)
                {
                    _height = 8.0M;
                    return;
                }

                if (value.ToString().Contains("."))
                {
                    var split = value.ToString().Split('.');
                    if (split.Any())
                    {
                        var dec = int.Parse(split[1]);

                        if (dec >= 12)
                        {
                            if (dec != 99)
                            {
                                _height = decimal.Truncate(_height.Value + 1);
                            }
                            else
                            {
                                _height = ((int)_height - 1) + 0.11M;
                            }
                        }
                        else
                        {
                            _height = value;
                        }
                    }

                }
                else
                {
                    _height = value;
                }
            }
        }

        public int? Weight { get; set; } = 169;
        public decimal Percent { get; set; }
        public string Address { get; set; }

        private DateTime? _date;

        private DateTime? Date
        {
            get => _date;
            set
            {
                _date = value;

                if (_date is not null)
                    Athlete.Birth = _date.Value.Year.ToString();
            }
        }

        private async Task UpdateProfileAsync()
        {
            // var response = await _accountManager.UpdateProfileAsync(profileModel);
            // if (response.Succeeded)
            // {
            //     await _authenticationManager.Logout();
            //     _snackBar.Add(localizer["Your Profile has been updated. Please Login to Continue."], Severity.Success);
            //     _navigationManager.NavigateTo("/");
            // }
            // else
            // {
            //     foreach (var message in response.Messages)
            //     {
            //         _snackBar.Add(localizer[message], Severity.Error);
            //     }
            // }
        }

        public IBrowserFile file { get; set; }

        [Parameter] public string ImageDataUrl { get; set; }

        public bool _uploading;

        private Task ImageFile_ConvertedToBase64(CustomFile file)
        {
            return Task.Run(async () =>
            {
                _uploading = true;
                StateHasChanged();

                await _httpClient.PostAsJsonAsync("api/v1/athlete/score",
                    new FileModel
                    {
                        File_Name = file.FileName,
                        RelateId = Athlete.Id,
                        Base64 = file.Base64,
                        Description = file.Description
                    });

                _ = Task.Run(async () =>
                {

                    var extension = file.FileName.Split('.')[1];
                    var fileName = $"{file.Description}.{extension}";

                    Athlete.ImageProfile = $"https://www.isportsrecruiting.com/api/v1/file/uploads/180/180/{fileName}";

                    var account = await LocalStorage.GetItemAsync<AccountDto>("account");
                    account.Athlete = Athlete;
                    await LocalStorage.SetItemAsync("account", account);

                    try
                    {
                        if (fileName.Contains("profile"))
                        {
                            await _httpClient.GetAsync($"api/v1/athlete/image/{Athlete.UserId}/{fileName}");

                            await runtime.InvokeVoidAsync("changeImageSrc", "mud-avatar-img", Athlete.ImageProfile);
                        }
                    }
                    catch { }
                    finally
                    {
                        _uploading = false;
                        StateHasChanged();
                    }

                   // _navigationManager.NavigateTo(_navigationManager.Uri, true);
                });
            });
        }

        private async Task<IEnumerable<string>> SearchStates(string value)
        {
            var names = StatesOfAmerica.Names;
            // if text is null or empty, show complete list
            if (string.IsNullOrEmpty(value))
                return names;
            return StatesOfAmerica.Names.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        private async Task ComponentFocusOut()
        {
            Athlete.Weight = Weight ?? 0;
            Athlete.Height = Height ?? 0;
            await AthleteChanged.InvokeAsync(Athlete);
        }
    }

    public class CustomFile
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public string Base64 { get; set; }
        public bool Uploaded { get; set; }
        private readonly IBrowserFile _file;

        public event Func<CustomFile, Task> ConvertedToBase64;

        public CustomFile()
        {
        }

        public CustomFile(IBrowserFile file)
        {
            _file = file;
        }

        public void StartConversion()
        {
            Task.Run(async () =>
            {
                Base64 = await ConvertToBase64();
                await ConvertedToBase64?.Invoke(this);
            });
        }

        private async Task<string> ConvertToBase64()
        {
            await using var memoryStream = new MemoryStream();
            await _file.OpenReadStream(maxAllowedSize: 512000 * 2).CopyToAsync(memoryStream);

            byte[] bytes = memoryStream.ToArray();
            return Convert.ToBase64String(bytes);
        }
    }
}