using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using AuthorizeNet.Api.Contracts.V1;
using Dapper;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Email;
using iSportsRecruiting.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;

namespace iSportsRecruiting.Server.Controllers.v1
{
    public class AccountController : BaseApiController<AccountController>
    {
        public ConcurrentDictionary<string, Action<Response<AccountDto>>> loginAttempts = new();

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> LogIn(LoginInfoDTO login)
        {
            try
            {
                var account = await GetAccountType(login.Email, login.Cipher);

                if (account is null)
                    return Ok(new Response<AccountDto> { Status = ResponseStatus.IncorrectEmailOrPassword, Message = "Incorrect Email/Password Combination" });

                if (account.Type == UserType.Athlete)
                {

                    if(!account.Athlete.Enabled)
                        return Ok(new Response<AccountDto> { Status = ResponseStatus.AccountDisabled, Message = "Account disabled" });

                    account.Athlete.PercentCompletionProfile = await GetAthletePercent(account.Athlete.Id);
                    var values = await _context.GetAthleteStatsByUserId(account.Athlete.UserId);
                    var structure = await _context.GetAthleteStatsStructureBySportId(values.Sports_Id, values.Position_Id);
                    account.Athlete.StatsStructValues = values?.Stats;
                    account.Athlete.StatsStruct = structure?.Stats;

                    var sportId = await _context.GetAthleteSportIdAsync(account.Athlete.UserId);
                    var sportName = await _context.GetSportNameAsync(sportId);
                    account.Athlete.SportId = sportId;
                    account.Athlete.SportName = sportName;

                    var files = await _context.GetAthleteFile(account.Athlete.Id);
                    var gpa = files.FirstOrDefault(s => s.Description.Contains("gpa"));
                    var act = files.FirstOrDefault(s => s.Description.Contains("act"));
                    var sat = files.FirstOrDefault(s => s.Description.Contains("sat"));

                    var imageProfile = files.FirstOrDefault(s => s.Description.Contains("profile-image"));

                    if (imageProfile is not null)
                    {
                        account.Athlete.ImageProfile = $"https://www.isportsrecruiting.com/api/v1/file/uploads/180/180/{account.Athlete.ImageProfile}";
                        account.Image = account.Athlete.ImageProfile;
                    }

                    if (gpa is not null)
                    {
                        account.Athlete.GpaFileName = gpa.File_Name;
                        account.Athlete.GpaFileOrigin = gpa.Description;
                    }

                    if (act is not null)
                    {
                        account.Athlete.ActFileName = act.File_Name;
                        account.Athlete.ActFileOrigin = act.Description;
                    }

                    if (sat is not null)
                    {
                        account.Athlete.SatFileName = sat.File_Name;
                        account.Athlete.SatFileOrigin = sat.Description;
                    }

                    var highSchools = await _context.GetAthleteHighSchoolByUserId(account.Athlete.UserId);
                    account.Athlete.HighSchools = highSchools.Select(h => h.ToDto());

                    var honors = await _context.GetAthleteHonorsByUserId(account.Athlete.UserId);
                    account.Athlete.Honors = honors.Select(h => h.ToDTO());

                    var awards = await _context.GetAthleteAwardsByUserId(account.Athlete.UserId);
                    account.Athlete.Awards = awards.Select(a => a.ToDTO());

                    var coaches = await _context.GetAthleteCoachesByUserId(account.Athlete.UserId);
                    account.Athlete.Coaches = coaches.Select(c => c.ToDTO());

                    var videos = await _context.GetAthleteVideosByUserId(account.Athlete.UserId);
                    account.Athlete.Videos = videos.Select(v => v.ToDTO());

                    var stories = await _context.GetAthleteStoriesByUserId(account.Athlete.UserId);
                    account.Athlete.Stories = stories.Select(s => s.ToDTO());

                    var clubs = await _context.GetAthleteClubsByUserId(account.Athlete.UserId);
                    account.Athlete.Clubs = clubs.Select(c => c.ToDTO());

                    var plan = await _context.GetBillingByEntityIdAsync(account.Athlete.Id);

                    if (plan is not null)
                        account.Athlete.Plan = plan.Plan;
                }

                return Ok(new Response<AccountDto> { Payload = account });
            }
            catch (Exception e)
            {
                return Ok(new Response<AccountDto> { Status = ResponseStatus.InternalError, Message = "Wrong email/password combination" });
            }
        }

        [HttpGet]
        [Route("validate/{email}")]
        public async Task<ActionResult> LogIn(string email)
        {
            try
            {
                return Ok(new Response<bool> { Payload = await EmailAlreadyExists(email) });
            }
            catch (Exception e)
            {
                return Ok(new Response<bool> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet]
        [Route("details/{userId}")]
        public async Task<ActionResult> GetAccountDetails(int userId)
        {
            try
            {
                var user = await GetSimpleAccount(userId);

                if (user is not null)
                {
                    var account = await GetAccountType(user.Email);

                    return Ok(new Response<AccountDto> { Payload = account });
                }

                return Ok(new Response<AccountDto>());

            }
            catch (Exception e)
            {
                return Ok(new Response<AccountDto> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPost]
        [Route("password/create")]
        public async Task<ActionResult> CreatePassword(LoginInfoDTO login)
        {
            try
            {
                await _context.AddUserSecurityAsync(new UserSecurity { User_Id = login.UserId, Cipher = login.Cipher, User_Type = login.UserType });

                return await LogIn(login);
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPost]
        [Route("register/athlete/{plan}")]
        public async Task<ActionResult> CreateAthleteAccount(AthleteRegisterDto register, string plan, [FromQuery] string promoCode = null!, [FromQuery] bool firstPayment = true, [FromQuery] bool isTempOffer = false)
        {
            try
            {
                var paymentResponse = await CreditCardController.PayAsync(_context, register.CreditCardTransaction, plan, promoCode,
                    firstPayment, isTempOffer);

                if (paymentResponse is not null && paymentResponse.resultCode == messageTypeEnum.Ok)
                {
                    var athlete = await _context.AddAthleteAsync(register);

                    if (!firstPayment)
                    {
                        await _context.UpdateBillingPlanAsync(athlete.Id, plan);
                    }
                    else
                    {
                        await _context.AddBillingPlanAsync(athlete.Id, new EntityBilling
                        {
                            Card_Code = register.CreditCardTransaction.CardCode,
                            Card_Number = register.CreditCardTransaction.CardNumber,
                            Entity_Id = athlete.Id,
                            Exp_Date = register.CreditCardTransaction.ExpirationDate,
                            Plan = plan,
                            PromotionCode = promoCode,
                            Zip = register.CreditCardTransaction.Zip
                        });
                    }

                    var unitPrice = paymentResponse.message.First(m => m.code == "UnitPrice").text;

                    await _context.PostPaymentByEntityId(new EntityPayments
                    { Entity_Id = athlete.Id, Plan = plan, Amount = unitPrice, Date = DateTime.Now, Promotion_Code = promoCode });

                    //var schools = await _context.GetUniversitiesAsync(sportName: register.Info.SportName);

                    var message = EmailTemplate.Welcome
                        .Replace("{name}", $"{athlete.First_Name} {athlete.Last_Name}");
                        //.Replace("{schools}", schools.Count().ToString());

                    await EmailClient.SendEmailAsync(new EmailAddress("contact@isportsrecruiting.com", "iSportsRecruiting"),
                        new EmailContact { Email = athlete.Email, Name = athlete.First_Name }, "Welcome to ISR!", message);

                    return Ok(new Response<AthleteDTO>(athlete.ToSimpleDTO()));
                }

                return Ok(new Response<AthleteDTO> { Message = "Your credit card data is not right, please correct", Status = ResponseStatus.InternalError });
            }
            catch (Exception e)
            {
                return Ok(new Response<AthleteDTO> { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpPost("register/coach")]
        public async Task<ActionResult> CreateCoachAsync(CoachDTO dto)
        {
            try
            {
                var coach = await _context.RegisterCoachAsync(dto);

                var html = EmailTemplate.WelcomeCoach
                    .Replace("{name}", dto.FirstName + " " + dto.LastName);

                await EmailClient.SendEmailAsync(new EmailAddress("contact@isportsrecruiting.com", "iSportsRecruiting"),
                    new EmailContact { Email = dto.Email, Name = dto.FirstName }, "Welcome to ISR!", html);

                return Ok(new Response<CoachDTO>(coach));
            }
            catch (Exception e)
            {
                return Ok(new Response<CoachDTO> { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpPost("register/confirmation/create")]
        public async Task<ActionResult> CreateConfirmationLink(ConfirmEmail confirm)
        {
            try
            {
                var token = await _context.AddConfirmationLink(confirm);

                var html = EmailTemplate.ConfirmationEmail
                    .Replace("{?name?}", confirm.Full_Name)
                    .Replace("{?url?}", $"https://www.isportsrecruiting.com/register/coach/{token}");

                await EmailClient.SendEmailAsync(new EmailAddress("contact@isportsrecruiting.com", "iSportsRecruiting"),
                   new EmailContact { Email = confirm.Email, Name = confirm.Full_Name }, "Please confirm your email", html);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpGet("register/{token}/validate")]
        public async Task<ActionResult> ValidateConfirmationLink(string token)
        {
            try
            {
                var confirmationLink = await _context.GetConfirmationLink(token);

                if (confirmationLink is null)
                {
                    throw new Exception("This url is wrong or has already expired...");
                }

                return Ok(new Response<ConfirmEmail>(confirmationLink));
            }
            catch (Exception e)
            {
                return Ok(new Response<ConfirmEmail> { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpPost("register/confirmation")]
        public async Task<ActionResult> RegisterFromConfirmationLink(ConfirmEmail confirm)
        {
            try
            {
                CoachDTO coach = new();
                coach.FirstName = confirm.Full_Name.Split(' ')[0];
                coach.LastName = confirm.Full_Name.Split(' ')[1];
                coach.Email = confirm.Email;
                coach.IsCoach = confirm.Is_Coach;
                coach.UniversityId = confirm.University_Id;
                coach.SportId = confirm.Sport_Id;
                coach.UniversityContactId = confirm.Contact_Id;
                coach.Cipher = confirm.Cipher;

                await _context.DeleteConfirmationLink(confirm.Id);

                return await CreateCoachAsync(coach);
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        private async Task<decimal> GetAthletePercent(int id)
        {
            try
            {
                decimal percent = 0;

                var athleteModel = await _context.GetAthleteByIdAsync(id);
                var athlete = await athleteModel.ToDtoAsync(_context);

                var athleteStatsModel = await _context.GetAthleteStatsByUserId(athlete.UserId);

                var athleticHistory = await _context.GetAthleteHighSchoolByUserId(athlete.UserId);

                var athleteVideos = await _context.GetAthleteVideosByUserId(athlete.UserId);

                var coaches = await _context.GetAthleteCoachesByUserId(athlete.UserId);

                if (coaches is not null)
                    percent += 2;

                if (athleteVideos is not null)
                    percent += 25M;

                if (athleteStatsModel is not null)
                    percent += 10M; //

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
                    percent += 5M;

                if (string.IsNullOrWhiteSpace(athlete.SAT)
                        && !string.IsNullOrWhiteSpace(athlete.ACT))
                    percent += 5M;

                if (!string.IsNullOrWhiteSpace(athlete.PersonalStatement))
                    percent += 10M;

                if (!string.IsNullOrWhiteSpace(athlete.ImageProfile))
                    percent += 5M;

                return percent;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        private async Task<bool> EmailAlreadyExists(string email)
        {
            var athlete = await _context.GetAthleteByEmailOrDefaultAsync(email);
            if (athlete is not null)
            {
                return true;
            }

            var coach = await _context.GetCoachByEmailOrDefaultAsync(email);
            if (coach is not null)
            {
                return true;
            }

            var admin = await _context.GetUserByEmailOrDefaultAsync(email);
            if (admin is not null)
            {
                return true;
            }

            var assistant = await _context.GetAssistantByEmailAsync(email);
            if (assistant is not null)
            {
                return true;
            }

            return false;
        }

        private Task<UserModel> GetSimpleAccount(int userId)
         => _context.GetUserByUserIdOrDefaultAsync(userId);

        private async Task<AccountDto> GetAccountType(string email, string cipher = null!)
        {
            AccountDto account = new();

            var athlete = await _context.GetAthleteByEmailOrDefaultAsync(email);
            if (athlete is not null)
            {
                account.Image = athlete.Image_Profile;
                account.Name = athlete.First_Name;
                account.Type = UserType.Athlete;
                account.Athlete = await athlete.ToDtoAsync(_context);
                account.Id = account.Athlete.Id;

                if (cipher is not null)
                {
                    var userSecurity = await _context.GetUserSecurityByUserIdAsync(account.Id, (int)account.Type);

                    if (userSecurity is null)
                        account.ShouldResetPassword = true;
                    else if (userSecurity.Cipher != cipher)
                        return null;
                }

                return account;
            }

            var coach = await _context.GetCoachByEmailOrDefaultAsync(email);
            if (coach is not null)
            {
                account.Name = coach.First_Name;
                account.Type = UserType.Coach;
                account.Coach = coach.ToDTO();
                account.Id = account.Coach.UserId;

                var university = await _context.GetUniversitiesAsync(account.Coach.UniversityId);
                account.Coach.UniversityImage = university.FirstOrDefault()?.Image;
                account.Coach.University = university.FirstOrDefault()?.University;
                account.Image = account.Coach.UniversityImage;

                if (cipher is not null)
                {
                    var userSecurity = await _context.GetUserSecurityByUserIdAsync(account.Id, (int)account.Type);

                    if (userSecurity is null)
                        account.ShouldResetPassword = true;
                    else if (userSecurity.Cipher != cipher)
                        return null;
                }

                return account;
            }

            // TODO: This needs to be fix. Needs ADDITIONAL CHECK FOR ADMIN USERS NOT JUST USERS TABLE

            var admin = await _context.GetUserByEmailOrDefaultAsync(email);
            if (admin is not null)
            {
                account.Name = admin.Username;
                account.Type = UserType.Admin;
                account.Admin = admin.ToDTO();
                account.Id = account.Admin.Id;

                if (cipher is not null)
                {
                    var userSecurity = await _context.GetUserSecurityByUserIdAsync(account.Id, (int)account.Type);

                    if (userSecurity is null)
                        account.ShouldResetPassword = true;
                    else if (userSecurity.Cipher != cipher)
                        return null;
                }

                return account;
            }

            var assistant = await _context.GetAssistantByEmailAsync(email);
            if (assistant is not null)
            {
                account.Name = assistant.Full_Name;
                account.Type = assistant.Type == 1 ? UserType.AssisAdmin : UserType.AssisCoord;
                account.Assistant = assistant.ToDTO();
                account.Id = account.Assistant.Id;

                var university = await _context.GetUniversitiesAsync(account.Assistant.UniversityId);
                if (university is not null)
                {
                    account.Assistant.UniversityImage = university.FirstOrDefault()?.Image;
                    account.Image = university.FirstOrDefault()?.Image;
                }

                if (cipher is not null)
                {
                    var userSecurity = await _context.GetUserSecurityByUserIdAsync(account.Id, (int)account.Type);

                    if (userSecurity is null)
                        account.ShouldResetPassword = true;
                    else if (userSecurity.Cipher != cipher)
                        return null;
                }

                return account;
            }

            return account;
        }

        [HttpGet("forgot/{email}")]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            string body = EmailTemplate.ResetPasswordNotification;
            body = body.Replace("{name}", email);

            var user = await _context.GetUserByEmailOrDefaultAsync(email);
            body = body.Replace("{email}", user.User_Id.ToString());

            await EmailClient.SendEmailFreeConsultationAsync(new EmailContact { Email = email, Name = "Client" }, "ISR Reset Password", body);

            return Ok();
        }

        [HttpGet("referral/{assistantId:int}/{athleteId:int}")]
        public async Task<ActionResult> SetReferral(int assistantId, int athleteId)
        {
            try
            {
                string sql = $"INSERT INTO referrals (athlete_id, assistant_id) VALUES ({athleteId}, {assistantId})";
                await _context.Connection.ExecuteAsync(sql);

                return Ok();
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }
    }
}
