using iSportsRecruiting.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace iSportsRecruiting.Server.Controllers
{
    public class SmsController : BaseApiController<SmsController>
    {
        //[HttpPost("leads")]
        //public async Task<ActionResult> Leads(Sample sample)
        //{
        //    try
        //    {
        //        AuthenticateTwilio();

        //        var leads = await _context.GetLeadsAsync();

        //        var phones = leads.Select(l => "+1" + Regex.Replace(l.Phone, @"[^\d]", "")).Where(phone => !string.IsNullOrWhiteSpace(phone) && phone.Length > 5);

        //        SendMessages(sample.Text, phones);

        //        return Ok(new Response());
        //    }
        //    catch (Exception e)
        //    {
        //        return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
        //    }
        //}

        //[HttpPost("athletes")]
        //public async Task<ActionResult> Athletes(Sample sample)
        //{
        //    try
        //    {
        //        AuthenticateTwilio();

        //        var athletes = await _context.GetAthletesAsync();

        //        var phones = athletes.Select(a => "+1" + Regex.Replace(a.Cel_Phone, @"[^\d]", "")).Where(phone => !string.IsNullOrWhiteSpace(phone) && phone.Length > 5);

        //        SendMessages(sample.Text, phones);

        //        return Ok(new Response());
        //    }
        //    catch (Exception e)
        //    {
        //        return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
        //    }
        //}

        [HttpPost("sendspecific")]
        public ActionResult SendSpecific(SmsParentDTO smsParent)
        {
            try
            {
                SendMessages(smsParent.Message, smsParent.Contacts);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult> Get(string direction = "Incoming")
        {
            try
            {
                List<MessageSmsDTO> messages = new();

                AuthenticateTwilio();
                var messageResources = await MessageResource.ReadAsync(limit: int.MaxValue);

                var messagesInbound = messageResources.Where(m => m.Direction ==
                    (direction == "Incoming" ? MessageResource.DirectionEnum.Inbound : MessageResource.DirectionEnum.OutboundApi));

                if (direction != "Incoming")
                    foreach (var item in messagesInbound)
                        messages.Add(new MessageSmsDTO { Error = item.ErrorMessage, Status = item.Status.ToString(), From = item.From.ToString(), FullName = "Me", Phone = item.To.ToString(), Role = "Admin", Sms = item.Body, Date = item.DateSent });
                else
                {
                    var leads = await _context.GetLeadsAsync();
                    var athletes = await _context.GetAthletesAsync();

                    foreach (var item in messagesInbound)
                    {
                        var lead = leads.Where(l => !string.IsNullOrWhiteSpace(l.Phone)).FirstOrDefault(l => ("+1" + Regex.Replace(l.Phone, @"[^\d]", "")).Contains(item.From.ToString()));
                        if (lead is not null)
                            messages.Add(new MessageSmsDTO { Error = item.ErrorMessage, Status = item.Status.ToString(), FullName = lead.Full_Name, Phone = lead.Phone, Role = "Lead", Sms = item.Body, Date = item.DateSent });

                        var athlete = athletes.Where(a => !string.IsNullOrWhiteSpace(a.Cel_Phone)).FirstOrDefault(a => ("+1" + Regex.Replace(a.Cel_Phone, @"[^\d]", "")).Contains(item.From.ToString()));
                        if (athlete is not null)
                            messages.Add(new MessageSmsDTO { Error = item.ErrorMessage, Status = item.Status.ToString(), FullName = $"{athlete.First_Name} {athlete.Last_Name}", Phone = lead.Phone, Role = "Athlete", Sms = item.Body, Date = item.DateSent });
                    }
                }

                return Ok(new Response<MessageSmsDTO[]>(payload: messages.ToArray()));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        private static void SendMessages(string body, ContactDTO[] contacts)
        {
            List<string> sent = new List<string>();
            string toBack = string.Empty;

            AuthenticateTwilio();

            var phones = contacts.Where(c => !string.IsNullOrWhiteSpace(c.Phone) && c.Phone.Length > 5);

            foreach (var to in phones)
            {
                _ = Task.Run(async () =>
                {
                    body = body.Replace("(name)", to.FullName);
                    await MessageResource.CreateAsync(
                        body: body,
                        from: new Twilio.Types.PhoneNumber("+12184003219"),
                        to: new Twilio.Types.PhoneNumber("+1" + Regex.Replace(to.Phone, @"[^\d]", "")));
                });
            }
        }

        private static void AuthenticateTwilio()
        {
            string authToken = "fc1749e586c24f68c2edff52295cadca";
            string accountSid = "AC154f42e2858df6f18ed640a563c41c82";

            TwilioClient.Init(accountSid, authToken);
        }
    }
}
