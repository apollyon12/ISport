using AuthorizeNet.Api.Contracts.V1;
using iSportsRecruiting.Server.Controllers.v1;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Models;
using iSportsRecruiting.Shared.Services;

namespace iSportsRecruiting.Server.Services;

public class PaymentsHostedService : IHostedService, IDisposable
{
    private int executionCount = 0;
    private Timer _timer = null!;

    private IServiceProvider Services { get; }

    public PaymentsHostedService(IServiceProvider services)
    {
        Services = services;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromDays(1));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        Task.Run(async () =>
        {

            var scope = Services.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<IDatabaseContext>();

            var athletes = await context.GetAthletesAsync();

            athletes = athletes.Where(x => x.Enabled == true);

            foreach (var athlete in athletes)
            {
                try
                {
                    var payments = await context.GetPaymentsByEntityIdAsync(athlete.Id);
                    var lastPayment = payments.OrderBy(p => p.Date).Last(p => !string.IsNullOrWhiteSpace(p.Plan) && p.Plan == "WS" || p.Plan == "MS");

                    var creditCardInfo = await context.GetBillingByEntityIdAsync(athlete.Id);

                    if(creditCardInfo is null) 
                        continue;

                    
                    if (lastPayment.Plan == "MS")
                    {
                        var result = DateTime.Now - lastPayment.Date;

                        if (result.Days >= 30)
                        {
                            var paymentResult = await PerformPayment(context, lastPayment, creditCardInfo);

                            if (paymentResult is not null && paymentResult.resultCode == messageTypeEnum.Ok)
                            {
                                var unitPrice = paymentResult.message.First(m => m.code == "UnitPrice").text;

                                await context.PostPaymentByEntityId(new EntityPayments
                                {
                                    Entity_Id = athlete.Id,
                                    Plan = lastPayment.Plan,
                                    Amount = unitPrice,
                                    Date = DateTime.Now,
                                    Promotion_Code = null!
                                });
                            }
                            else
                            {
                                await context.UpdateAthleteIsEnabled(athlete.Id, false);
                            }
                        }
                    }
                }
                catch { }
            }
        });
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private Task<messagesType?> PerformPayment(IDatabaseContext context, EntityPayments payments, EntityBilling creditCardInfo)
    {
        var creditCardTransaction = new CreditCardTransactionDTO
        {
            CardCode = creditCardInfo.Card_Code,
            CardNumber = creditCardInfo.Card_Number,
            ExpirationDate = creditCardInfo.Exp_Date,
            Zip = creditCardInfo.Zip
        };

        return CreditCardController.PayAsync(context, creditCardTransaction, payments.Plan);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}