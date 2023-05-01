using System.Net.Http.Json;
using Blazored.LocalStorage;
using iSportsRecruiting.Shared.DTO;
using Microsoft.JSInterop;

namespace iSportsRecruiting.Client.Shared;

public interface IPushMessageService
{
    Task<bool> SendAsync(string message, string redirectTo, string icon = "icon-512.png");
}

public class PushMessageService : IPushMessageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public PushMessageService(IJSRuntime jsRuntime, HttpClient httpClient, ILocalStorageService localStorage)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<bool> SendAsync(string message, string redirectTo, string icon = "icon-512.png")
    {
        try
        {
            var notificationsSupported = await _localStorage.GetItemAsync<bool?>("notificationsSupported");
            if (notificationsSupported.HasValue && !notificationsSupported.Value)
                return false;

            var subscription = await _jsRuntime.InvokeAsync<NotificationDto?>("blazorPushNotifications.requestSubscription");
            if (subscription is null)
                return false;

            subscription.Icon = icon;
            subscription.Message = message;
            subscription.RedirectTo = redirectTo;

            var result = await _httpClient.PostAsJsonAsync("api/v1/notification/send", subscription);
            var response = await result.Content.ReadFromJsonAsync<Response>();
            if (response is null || response.Status != ResponseStatus.Ok)
                throw new Exception();

            await _localStorage.SetItemAsync<bool?>("notificationsSupported", true);
            return true;
        }
        catch
        {
            await _localStorage.SetItemAsync<bool?>("notificationsSupported", false);
            return false;
        }
    }
}