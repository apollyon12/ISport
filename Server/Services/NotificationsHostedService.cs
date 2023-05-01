using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Email;
using iSportsRecruiting.Shared.Services;
using SendGrid.Helpers.Mail;
using System.Text.RegularExpressions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Messaging;

namespace iSportsRecruiting.Server.Services
{
    public class NotificationsHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer _timer = null!;

        private IServiceProvider Services { get; }

        public NotificationsHostedService(IServiceProvider services)
        {
            Services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var scope = Services.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<IDatabaseContext>();

            //Aca enviamos los correos y los mensajes
            _ = Task.Run(async () =>
            {
                var scheduledNotificationsResponse = await context.GetScheduledNotificationAsync();
                string authToken = "fc1749e586c24f68c2edff52295cadca";
                string accountSid = "AC154f42e2858df6f18ed640a563c41c82";

                TwilioClient.Init(accountSid, authToken);
                foreach (var notification in scheduledNotificationsResponse)
                {
                    string[] ids = notification.Entities.Split(',');
                    foreach (var id in ids)
                    {
                        var athleteResponse = await context.GetAthleteByIdAsync(int.Parse(id));
                        if (athleteResponse is null)
                        {
                            var leadResponse = await context.GetLeadByIdAsync(int.Parse(id));
                            if (leadResponse is not null)
                            {
                                if (notification.Daily)
                                {
                                    if (notification.Send_Sms)
                                    {
                                        await SendSms(notification.Content, leadResponse.Phone);
                                    }
                                    if (notification.Send_Email)
                                    {
                                        await SendEmail(leadResponse.Email, leadResponse.Full_Name, notification.Content);
                                    }
                                }
                                if (!string.IsNullOrEmpty(notification.DayOfTheWeek))
                                {
                                    if (DateTime.UtcNow.DayOfWeek.ToString().ToLower() == notification.DayOfTheWeek.ToLower())
                                    {
                                        if (notification.Send_Sms)
                                        {
                                            await SendSms(notification.Content, leadResponse.Phone);
                                        }
                                        if (notification.Send_Email)
                                        {
                                            await SendEmail(leadResponse.Email, leadResponse.Full_Name, notification.Content);
                                        }
                                    }
                                }
                                if (notification.DayOfTheMonth > 0)
                                {
                                    if (notification.Send_Sms)
                                    {
                                        await SendSms(notification.Content, leadResponse.Phone);
                                    }
                                    if (notification.Send_Email)
                                    {
                                        await SendEmail(leadResponse.Email, leadResponse.Full_Name, notification.Content);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (athleteResponse is not null)
                            {
                                if (notification.Daily)
                                {
                                    if (notification.Send_Sms)
                                    {
                                        await SendSms(notification.Content, athleteResponse.Cel_Phone);
                                    }
                                    if (notification.Send_Email)
                                    {
                                        await SendEmail(athleteResponse.Email, athleteResponse.First_Name, notification.Content);
                                    }
                                }
                                if (!string.IsNullOrEmpty(notification.DayOfTheWeek))
                                {
                                    if (DateTime.UtcNow.DayOfWeek.ToString().ToLower() == notification.DayOfTheWeek.ToLower())
                                    {
                                        if (notification.Send_Sms)
                                        {
                                            await SendSms(notification.Content, athleteResponse.Cel_Phone);
                                        }
                                        if (notification.Send_Email)
                                        {
                                            await SendEmail(athleteResponse.Email, athleteResponse.First_Name, notification.Content);
                                        }
                                    }
                                }
                                if (notification.DayOfTheMonth > 0)
                                {
                                    if (notification.Send_Sms)
                                    {
                                        await SendSms(notification.Content, athleteResponse.Cel_Phone);
                                    }
                                    if (notification.Send_Email)
                                    {
                                        await SendEmail(athleteResponse.Email, athleteResponse.First_Name, notification.Content);
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }
        private async Task SendSms(string content, string phone)
        {
            await MessageResource.CreateAsync(
                                                body: content,
                                                from: new Twilio.Types.PhoneNumber("+12184003219"),
                                                to: new Twilio.Types.PhoneNumber("+1" + Regex.Replace(phone, @"[^\d]", "")));
        }
        private async Task SendEmail(string email, string name, string content)
        {
            await EmailClient.SendEmailAsync(
                new EmailAddress { Email = "contact@isportsrecruiting.com", Name = "iSportsRecruiting" },
                new EmailContact { Email = email, Name = name }, "Notification", content);
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
