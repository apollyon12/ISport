using System.ComponentModel.Design;
using AuthorizeNet.Api.Contracts.V1;
using iSportsRecruiting.Server.Controllers.v1;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Email;
using iSportsRecruiting.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;

namespace iSportsRecruiting.Server.Controllers;

public class ServiceController : BaseApiController<ServiceController>
{
    [HttpGet]
    [Route("pay/{athleteId:int}/{plan}")]
    public async Task<ActionResult> PayService(int athleteId, string plan, [FromQuery]string promoCode = null!)
    {
        try
        {
            var athlete = await _context.GetAthleteByIdAsync(athleteId);
            var billing = await _context.GetBillingByEntityIdAsync(athleteId);
            var creditCardTransaction = new CreditCardTransactionDTO
            {
                FirstName = athlete.First_Name,
                LastName = athlete.Last_Name,
                Address = athlete.Address,
                CardCode = billing.Card_Code,
                CardNumber = billing.Card_Number,
                City = athlete.City,
                ExpirationDate = billing.Exp_Date,
                Zip = billing.Zip
            };

            var paymentResponse =
                await CreditCardController.PayAsync(_context, creditCardTransaction, plan, promoCode, false);

            if (paymentResponse is null || paymentResponse.resultCode != messageTypeEnum.Ok)
                return Ok(new Response<bool>
                {
                    Payload = false,
                    Message = "Your credit card data is not right, please correct",
                    Status = ResponseStatus.InternalError
                });

            var unitPrice = paymentResponse.message.First(m => m.code == "UnitPrice").text;

            await _context.PostPaymentByEntityId(new EntityPayments
            {
                Entity_Id = athleteId, Plan = plan, Amount = unitPrice, Date = DateTime.Now,
                Promotion_Code = promoCode
            });

            int serviceId = plan switch
            {
                "PC" => 1,
                "BE" => 2,
                "VE" => 3,
                _ => 0
            };

            if (serviceId != 0)
            {
                await _context.AddAthleteService(athleteId, serviceId);

                if (serviceId == 1) //ADVISOR
                {
                    await SendAdvisorTeamNotification(athlete.ToSimpleDTO(), "ronnie@isportsrecruiting.com", "Ronnie Romero");
                    await SendAdvisorAthleteNotification(athlete.ToSimpleDTO());
                    //Advisor
                    //await SendAdvisorTeamNotification(athlete.ToSimpleDTO(), "advisor", "advisor");
                }
                else if (serviceId == 3) //VIDEO
                {
                    await SendVideoTeamNotification(athlete.ToSimpleDTO(), "ronnie@isportsrecruiting.com", "Ronnie Romero");
                    await SendVideoAthleteNotification(athlete.ToSimpleDTO());
                    //Advisor
                    //await SendAdvisorTeamNotification(athlete.ToSimpleDTO(), "advisor", "advisor");
                }

                return Ok(new Response<bool>
                {
                    Payload = true, Message = "Payment successful", Status = ResponseStatus.Ok
                });
            }

            throw new Exception("This service doesn't exist");
        }
        catch (Exception e)
        {
            return Ok(new Response<bool> {Payload = false, Message = e.Message, Status = ResponseStatus.InternalError});
        }
    }

    [HttpGet("check/{athleteId:int}/{serviceId:int}")]
    public async Task<ActionResult> CheckIfServiceExists(int athleteId, int serviceId)
    {
        try
        {
            var count = await _context.CheckIfServiceExists(athleteId, serviceId);

            return Ok(new Response<int> { Payload = count, Message = null!, Status = ResponseStatus.Ok });
        }
        catch (Exception e)
        {
            return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.InternalError });
        }
    }
    
    private static async Task SendAdvisorTeamNotification(AthleteDTO athlete, string receiverEmail, string receiverName)
    {
        var message = EmailTemplate.AdvisorTeamNotification
            .Replace("{athlete_name}", $"{athlete.FirstName} {athlete.LastName}")
            .Replace("{receiver_name}", receiverName);

        await EmailClient.SendEmailAsync(new EmailAddress("advisor@isportsrecruiting.com", "ISR Advisor"),
            new EmailContact { Email = receiverEmail, Name = receiverName }, $"ISR Team Advisor Request from {athlete.FirstName} {athlete.LastName}", message);
    }
    
    private static async Task SendVideoTeamNotification(AthleteDTO athlete, string receiverEmail, string receiverName)
    {
        var message = EmailTemplate.VideoTeamNotification
            .Replace("{athlete_name}", $"{athlete.FirstName} {athlete.LastName}")
            .Replace("{receiver_name}", receiverName);

        await EmailClient.SendEmailAsync(new EmailAddress("video@isportsrecruiting.com", "ISR Video Editing"),
            new EmailContact { Email = receiverEmail, Name = receiverName }, $"ISR Team Video Editing Request from {athlete.FirstName} {athlete.LastName}", message);
    }
    
    private static async Task SendAdvisorAthleteNotification(AthleteDTO athlete)
    {
        var message = EmailTemplate.AdvisorAthleteNotification
            .Replace("{athlete_name}", $"{athlete.FirstName} {athlete.LastName}");

        await EmailClient.SendEmailAsync(new EmailAddress("advisor@isportsrecruiting.com", "ISR Advisor"),
            new EmailContact { Email = athlete.Email, Name = $"{athlete.FirstName} {athlete.LastName}" }, $"Recruiting Advisor Requested", message);
    }
    
    private static async Task SendVideoAthleteNotification(AthleteDTO athlete)
    {
        var message = EmailTemplate.VideoAthleteNotification
            .Replace("{athlete_name}", $"{athlete.FirstName} {athlete.LastName}");

        await EmailClient.SendEmailAsync(new EmailAddress("video@isportsrecruiting.com", "ISR Video Editing"),
            new EmailContact { Email = athlete.Email, Name = $"{athlete.FirstName} {athlete.LastName}" }, $"Video Editing Requested", message);
    }
}