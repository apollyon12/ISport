using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Email;
using iSportsRecruiting.Shared.Models;
using iSportsRecruiting.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using System.Text.RegularExpressions;

namespace iSportsRecruiting.Server.Controllers.v1
{
    public class EmailController : BaseApiController<EmailController>
    {
        [HttpPost("sendspecific")]
        public ActionResult SendSpecific(EmailParentDTO emailParent)
        {
            try
            {
                var emails = emailParent.Contacts
                    .Where(email => !string.IsNullOrWhiteSpace(email.Email) && email.Email.Length > 5);

                _ = Task.Run(async () => 
                {
                    foreach (var email in emails)
                    {
                        await EmailClient.SendEmailAsHtmlAsync(
                             new EmailAddress { Email = "contact@isportsrecruiting.com", Name = "iSportsRecruiting" },
                             new EmailContact { Email = email.Email, Name = email.FullName }, emailParent.Subject, emailParent.Message);
                    }
                });               

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            try
            {
                var email = await _context.GetEmailById(id);

                return Ok(new Response<EmailModel>(email));
            }
            catch (Exception e)
            {
                return Ok(new Response<EmailModel> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("sent/{senderId:int}")]
        public async Task<ActionResult> GetSents(int senderId)
        {
            try
            {
                var emails = await _context.GetEmailsBySenderId(senderId);

                return Ok(new Response<IEnumerable<EmailModel>>(emails));
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<EmailModel>>
                { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("receive/{receiverId:int}")]
        public async Task<ActionResult> GetReceives(int receiverId, int page = 1, int take = 10)
        {
            try
            {
                var emails = await _context.GetEmailsByReceiverId(receiverId);

                return Ok(new Response<IEnumerable<EmailModel>>(emails) { Total = emails.Count() });
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<EmailModel>>
                { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("sent/{senderId:int}/count")]
        public async Task<ActionResult> GetSentsCount(int senderId)
        {
            try
            {
                var count = await _context.GetCountEmailsBySenderId(senderId);
                return Ok(new Response<int> { Payload = count });
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpGet("receive/{receiveId:int}/count")]
        public async Task<ActionResult> GetReceivesCount(int receiveId)
        {
            try
            {
                var count = await _context.GetCountEmailsByReceiverId(receiveId);
                return Ok(new Response<int> { Payload = count });
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpGet("history/{sender:int}/{receiver:int}")]
        public async Task<ActionResult> GetEmailHistory(int sender, int receiver)
        {
            try
            {
                List<EmailModel> history = new();

                var emails = await _context.GetConversationAsync(sender, receiver);

                history.AddRange(emails);

                return Ok(new Response<IEnumerable<EmailModel>> { Payload = history.OrderBy(e => e.Id) });
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<EmailModel>>
                { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(EmailModel email)
        {
            try
            {
                var id = await _context.AddEmail(email);

                email.Id = id;

                // var connectionId = SignalRHub.GetConnectionIdByUserId(email.Receiver_Id);
                //
                // if (!string.IsNullOrWhiteSpace(connectionId))
                //     _ = _hubContext.Clients.Client(connectionId).SendAsync("EmailReceived", email);

                return Ok(new Response<int>(id));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpGet("readed/{whoReaded:int}/{whoIsNotified:int}")]
        public async Task<ActionResult> PostReadedMessagesAsync(int whoReaded, int whoIsNotified)
        {
            try
            {
                // var connectionId = SignalRHub.GetConnectionIdByUserId(whoIsNotified);
                //
                // if (!string.IsNullOrWhiteSpace(connectionId))
                //     _ = _hubContext.Clients.Client(connectionId).SendAsync("Ping", whoReaded);

                await _context.UpdateEmailsAsRead(whoReaded, whoIsNotified);

                return Ok(new Response<int>(whoReaded));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _context.RemoveEmail(id);

                return Ok();
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpPost("sendemail/{email}/{name}/{subject}/{content}")]
        public async Task<ActionResult> SendEmail(string email, string name, string subject, string content,
            EmailContact to)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(to.Quote))
                    content += $"<br /> {to.Quote}";

                await EmailClient.SendEmailAsync(
                    new EmailAddress { Email = email, Name = name }, to, subject, content);

                return Ok();
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("sendemail/athlete/{name}")]
        public async Task<ActionResult> SendEmailAsAnAthlete(string name, EmailRequest email)
        {
            try
            {
                var to = email.Contact;

                var baseTemplate = await GetBaseTemplateAsync(to.Athlete, email.Body);
                var from = new EmailAddress("studentathlete@isportsrecruiting.com",
                    $"{to.Athlete.FirstName} {to.Athlete.LastName}");
                var subject =
                    $"{to.Athlete.FirstName} {to.Athlete.LastName}, {to.Athlete.Position}, {to.Athlete.State}, GPA: {to.Athlete.GPA}";

                string replacementName = "{to_name}";

                var split = to.Name.Split(' ');
                string toName = split.Any() ? split[1] : to.Name;

                baseTemplate = baseTemplate.Replace(replacementName, toName);
                replacementName = to.Name;

                var toEmail = new EmailAddress(to.Email, to.Name);
                var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, "", baseTemplate);

                await EmailClient.Client.SendEmailAsync(msg);

                return Ok();
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpGet("blastemail/{athleteId:int}")]
        public async Task<ActionResult> SendBlastEmail(int athleteId, bool onlyFavorites = false)
        {
            int coachesCount = 0;
            var percent = await GetAthletePercent(athleteId, _context);

            if (percent >= 90)
            {
                var athleteModel = await _context.GetAthleteByIdAsync(athleteId);
                var athlete = await athleteModel.ToDtoAsync(_context);

                try
                {
                    UniversityModel[] universities;

                    var sportId = await _context.GetAthleteSportIdAsync(athlete.UserId);
                    var sportName = await _context.GetSportNameAsync(sportId);
                    athlete.SportName = sportName;

                    var settings = await _context.GetAthleteSettingsAsync(athleteId);
                    if (settings is not null)
                    {
                        var average = settings.Average.Split(',');
                        var gpa = average[0] == string.Empty || average[0] == "0" ? null : average[0];
                        var act = average[1] == string.Empty || average[1] == "0" ? null : average[1];
                        var sat = average[2] == string.Empty || average[2] == "0" ? null : average[2];

                        universities = await _context.GetUniversitiesForBlastEmail(sportName,
                            settings.Divisions != null ? settings.Divisions.Split(",") : new List<string>().ToArray(),
                            gpa, act, sat, null);
                    }
                    else
                    {
                        universities =
                            await _context.GetUniversitiesForBlastEmail(sportName, null, null, null, null, null);
                    }

                    if (onlyFavorites)
                    {
                        var favorites = await _context.GetFavoriteUniversitiesByAthleteId(athleteId);
                        universities = favorites.ToArray();
                    }

                    var emailsContact = _context.GetEmailsByUniversities(sportName, universities.ToArray());

                    await SendEmailsAsync(athlete, emailsContact.ToArray());

                    coachesCount = emailsContact.Length;
                    await _context.MarkServiceAsUsed(athlete.Id, 2);

                    await SendBlastEmailTeamNotification(athlete, "ronnie@isportsrecruiting.com", "Ronnie Romero");
                    await SendBlastEmailAthleteNotification(athlete, coachesCount);
                }
                catch (Exception e)
                {
                }

                return Ok(new Response<int> { Payload = coachesCount });
            }

            return Ok(new Response<int> { Message = "Profile incomplete", Status = ResponseStatus.InternalError });
        }

        [HttpGet("blastemail/{athleteId:int}/count")]
        public async Task<ActionResult> GetBlastEmailCount(int athleteId, bool onlyFavorites = false)
        {
            int universitiesCount;

            var athleteModel = await _context.GetAthleteByIdAsync(athleteId);
            var athlete = await athleteModel.ToDtoAsync(_context);

            try
            {
                UniversityModel[] universities;

                var sportId = await _context.GetAthleteSportIdAsync(athlete.UserId);
                var sportName = await _context.GetSportNameAsync(sportId);
                athlete.SportName = sportName;

                var settings = await _context.GetAthleteSettingsAsync(athleteId);
                if (settings is not null)
                {
                    var average = settings.Average.Split(',');
                    var gpa = average[0] == string.Empty || average[0] == "0" ? null : average[0];
                    var act = average[1] == string.Empty || average[1] == "0" ? null : average[1];
                    var sat = average[2] == string.Empty || average[2] == "0" ? null : average[2];

                    universities = await _context.GetUniversitiesForBlastEmail(sportName,
                        settings.Divisions != null ? settings.Divisions.Split(",") : new List<string>().ToArray(),
                        gpa, act, sat, null);
                }
                else
                {
                    universities =
                        await _context.GetUniversitiesForBlastEmail(sportName, null, null, null, null, null);
                }

                if (onlyFavorites)
                {
                    var favorites = await _context.GetFavoriteUniversitiesByAthleteId(athleteId);
                    universities = favorites.ToArray();
                }

                universitiesCount = universities.Count();
            }
            catch (Exception e)
            {
                return Problem();
            }

            return Ok(new Response<int> { Payload = universitiesCount });
        }

        private static async Task SendBlastEmailTeamNotification(AthleteDTO athlete, string receiverEmail,
            string receiverName)
        {
            var message = EmailTemplate.BlastEmailTeamNotification
                .Replace("{athlete_name}", $"{athlete.FirstName} {athlete.LastName}")
                .Replace("{receiver_name}", receiverName);

            await EmailClient.SendEmailAsync(new EmailAddress("blastemail@isportsrecruiting.com", "ISR Blast Email"),
                new EmailContact { Email = receiverEmail, Name = receiverName },
                $"ISR Team Blast Email Request from {athlete.FirstName} {athlete.LastName}", message);
        }

        private static async Task SendBlastEmailAthleteNotification(AthleteDTO athlete, int coachesCount)
        {
            var message = EmailTemplate.BlastEmailAthleteNotification
                .Replace("{athlete_name}", $"{athlete.FirstName} {athlete.LastName}")
                .Replace("{count}", coachesCount.ToString());

            await EmailClient.SendEmailAsync(new EmailAddress("blastemail@isportsrecruiting.com", "ISR Blast Email"),
                new EmailContact { Email = athlete.Email, Name = $"{athlete.FirstName} {athlete.LastName}" },
                $"ISR Team Blast Email Requested", message);
        }

        private static async Task SendEmailsAsync(AthleteDTO athlete, EmailContact[] contacts)
        {
            int count = 0;
            var baseTemplate = await GetBaseTemplateAsync(athlete);
            var from = new EmailAddress("studentathlete@isportsrecruiting.com",
                $"{athlete.FirstName} {athlete.LastName}");
            var subject =
                $"{athlete.FirstName} {athlete.LastName}, {athlete.Position}, {athlete.State}, GPA: {athlete.GPA}";

            string replacementName = "{to_name}";
            foreach (var contact in contacts)
            {
                count++;

                baseTemplate = baseTemplate.Replace(replacementName, contact.Name);
                replacementName = contact.Name;

                var to = new EmailAddress(contact.Email, contact.Name);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", baseTemplate);

                await EmailClient.Client.SendEmailAsync(msg);
            }
        }

        private static async Task<string> GetBaseTemplateAsync(AthleteDTO athlete, string customStatement = null)
        {
            var template = await System.IO.File.ReadAllTextAsync("Emails/athlete_profile.html");
            template = template.Replace("{id}", athlete.Id.ToString());
            template = template.Replace("{url_name}", $"{athlete.FirstName} {athlete.LastName}"
                .Replace(" ", "-").Replace(",", "").ToLower());
            template = template.Replace("{first_name}", athlete.FirstName);
            template = template.Replace("{first_name_upper}", athlete.FirstName.ToUpper());
            template = template.Replace("{middle_name}", athlete.MiddleName);
            template = template.Replace("{last_name}", athlete.LastName);
            template = template.Replace("{state}", athlete.State);
            template = template.Replace("{GPA}", athlete.GPA);
            template = template.Replace("{SAT}", athlete.SAT);
            template = template.Replace("{ACT}", athlete.ACT);
            template = template.Replace("{sport}", athlete.SportName);
            template = template.Replace("{weight}", athlete.Weight.ToString());
            template = template.Replace("{height}", athlete.Height.ToString());
            template = template.Replace("{email}", athlete.Email);
            template = template.Replace("{cel_phone}", athlete.CellPhone);
            template = template.Replace("{hs_grad_year}", athlete.GraduationYear);
            template = template.Replace("{position}", athlete.Position);
            template = template.Replace("{age}", athlete.Birth);
            template = template.Replace("{NCAA}", athlete.NCAA.ToString());
            template = template.Replace("{NAIA}", athlete.NAIA.ToString());
            template = template.Replace("{image_profile}", athlete.ImageProfile);
            template = template.Replace("{personal_statement}",
                (customStatement is null ? athlete.PersonalStatement : customStatement).Replace("\n", "<br>"));

            return template;
        }

        private static async Task<decimal> GetAthletePercent(int athleteId, IDatabaseContext context)
        {
            decimal percent = 0;

            var athleteModel = await context.GetAthleteByIdAsync(athleteId);
            var athlete = await athleteModel.ToDtoAsync(context);

            var athleteStatsModel = await context.GetAthleteStatsByUserId(athlete.UserId);

            var athleticHistory = await context.GetAthleticHistorysByUserId(athlete.UserId);

            var athleteVideos = await context.GetAthleteVideosByUserId(athlete.UserId);

            if (athleteVideos is not null)
                percent += 25M;

            if (athleteStatsModel is not null)
                percent += 10M;

            if (athleticHistory is not null)
                percent += 5M;

            if (!string.IsNullOrWhiteSpace(athlete.FirstName))
                percent += 4.5M;

            if (!string.IsNullOrWhiteSpace(athlete.LastName))
                percent += 3.5M;

            if (athlete.Weight != 0)
                percent += 2.5M;

            if (athlete.Height != 0)
                percent += 2.5M;

            if (athlete.NCAA != 0)
                percent += 2.5M;

            if (athlete.NAIA != 0)
                percent += 2.5M;

            if (!string.IsNullOrWhiteSpace(athlete.CellPhone))
                percent += 5M;

            if (!string.IsNullOrWhiteSpace(athlete.GraduationYear))
                percent += 5M;

            if (!string.IsNullOrWhiteSpace(athlete.GPA))
                percent += 10M;

            if (!string.IsNullOrWhiteSpace(athlete.SAT))
                percent += 3.5M;

            if (!string.IsNullOrWhiteSpace(athlete.ACT))
                percent += 3.5M;

            if (!string.IsNullOrWhiteSpace(athlete.PersonalStatement))
                percent += 10M;

            if (!string.IsNullOrWhiteSpace(athlete.ImageProfile))
                percent += 5M;

            return percent;
        }
    }
}