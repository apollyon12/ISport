using System.IO;
using Microsoft.AspNetCore.Mvc;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Email;
using iSportsRecruiting.Shared.Models;

namespace iSportsRecruiting.Server.Controllers.v1
{
    public class AthleteController : BaseApiController<AthleteController>
    {
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var athletes = await _context.GetAthletesAsync();
                var athletesDto = athletes.Select(a => a.ToSimpleDTO());

                return Ok(new Response<IEnumerable<AthleteDTO>>(athletesDto));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("{id:int}/short")]
        public async Task<ActionResult> Get(int id)
        {
            try
            {
                var athlete = await _context.GetAthleteByIdAsync(id);
                if (athlete is not null)
                {
                    var athleteDto = athlete.ToSimpleDTO();

                    return Ok(new Response<AthleteDTO>(athleteDto));
                }
                else
                {
                    return Ok(new Response<AthleteDTO> { Status = ResponseStatus.InternalError}); ;
                }
            }
            catch (Exception e)
            {
                return Ok(new Response<AthleteDTO> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("sport/{sportId:int}")]
        public async Task<ActionResult> GetBySportId(int sportId)
        {
            try
            {
                var athletes = await _context.GetAthletesAsync();
                var stats = await _context.GetAthleteStats();

                var filteredStats = stats.Where(s => s.Sports_Id == sportId).Select(s => s.User_Id);
                var filteredAthletes =
                    athletes.Where(a => filteredStats.Contains(a.User_Id)).Select(a => a.ToSimpleDTO());

                return Ok(new Response<IEnumerable<AthleteDTO>>(filteredAthletes));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateAthlete(AthleteDTO athlete)
        {
            try
            {
                var result = await _context.UpdateAthlete(athlete);

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("image/{athleteId:int}/{fileName}")]
        public async Task<ActionResult> UpdateImageProfileAthlete(int athleteId, string fileName)
        {
            try
            {
                var result = await _context.UpdateAthleteImageProfile(athleteId, fileName);

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }
        [HttpGet("toggle/{athleteId}")]
        public async Task<ActionResult> ToggleEnable(int athleteId, [FromQuery]bool enabled)
        {
            try
            {
                var result = await _context.UpdateAthleteIsEnabled(athleteId, enabled);

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult> GetCount()
        {
            try
            {
                var athletes = await _context.GetAthletesCountAsync();

                return Ok(new Response<int>(athletes));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("referral/{id:int}")]
        public async Task<ActionResult> GetByReferralId(int id)
        {
            try
            {
                var athletes = await _context.GetAthletesByReferralAsync(id);
                var athletesDto = athletes?.Select(a
                    => a.ToSimpleDTO()) ?? Enumerable.Empty<AthleteDTO>();

                return Ok(new Response<IEnumerable<AthleteDTO>>(athletesDto));
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<AthleteDTO>>
                    { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("referral/{id:int}/count")]
        public async Task<ActionResult> GetCountByReferralId(int id)
        {
            try
            {
                var athletes = await _context.GetAthletesCountByReferralAsync(id);
                return Ok(new Response<int>(athletes));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetCompleteProfile(int id)
        {
            try
            {
                var athleteModel = await _context.GetAthleteByIdAsync(id);
                var athlete = await athleteModel.ToDtoAsync(_context);

                var values = await _context.GetAthleteStatsByUserId(athlete.UserId);
                var structure = await _context.GetAthleteStatsStructureBySportId(values.Sports_Id, values.Position_Id);
                athlete.StatsStructValues = values?.Stats;
                athlete.StatsStruct = structure?.Stats;

                var sportId = await _context.GetAthleteSportIdAsync(athlete.UserId);
                var sportName = await _context.GetSportNameAsync(sportId);
                athlete.SportName = sportName;

                var files = await _context.GetAthleteFile(athlete.Id);
                var gpa = files.FirstOrDefault(s => s.Description.Contains("gpa"));
                var act = files.FirstOrDefault(s => s.Description.Contains("act"));
                var sat = files.FirstOrDefault(s => s.Description.Contains("sat"));

                var imageProfile = files.FirstOrDefault(s => s.Description.Contains("profile-image"));

                if (imageProfile is not null)
                    athlete.ImageProfile = $"https://isportsrecruiting.com/api/v1/file/uploads/150/150/{athlete.ImageProfile}";

                if (gpa is not null)
                {
                    athlete.GpaFileName = gpa.File_Name;
                    athlete.GpaFileOrigin = gpa.Description;
                }

                if (act is not null)
                {
                    athlete.ActFileName = act.File_Name;
                    athlete.ActFileOrigin = act.Description;
                }

                if (sat is not null)
                {
                    athlete.SatFileName = sat.File_Name;
                    athlete.SatFileOrigin = sat.Description;
                }

                var highSchools = await _context.GetAthleteHighSchoolByUserId(athlete.UserId);
                athlete.HighSchools = highSchools.Select(h => h.ToDto());

                var honors = await _context.GetAthleteHonorsByUserId(athlete.UserId);
                athlete.Honors = honors.Select(h => h.ToDTO());

                var awards = await _context.GetAthleteAwardsByUserId(athlete.UserId);
                athlete.Awards = awards.Select(a => a.ToDTO());

                var coaches = await _context.GetAthleteCoachesByUserId(athlete.UserId);
                athlete.Coaches = coaches.Select(c => c.ToDTO());

                var videos = await _context.GetAthleteVideosByUserId(athlete.UserId);
                athlete.Videos = videos.Select(v => v.ToDTO());

                var stories = await _context.GetAthleteStoriesByUserId(athlete.UserId);
                athlete.Stories = stories.Select(s => s.ToDTO());

                var clubs = await _context.GetAthleteClubsByUserId(athlete.UserId);
                athlete.Clubs = clubs.Select(c => c.ToDTO());

                return Ok(new Response<AthleteDTO>
                {
                    Payload = athlete
                });
            }
            catch (Exception e)
            {
                return Ok(new Response<AthleteDTO>
                {
                    Message = e.Message,
                    Status = ResponseStatus.InternalError
                });
            }
        }

        [HttpGet]
        [Route("percent")]
        public async Task<ActionResult> Percent(int id)
        {
            try
            {
                decimal percent = 0;

                var athleteModel = await _context.GetAthleteByIdAsync(id);
                var athlete = await athleteModel.ToDtoAsync(_context);

                var athleteStatsModel = await _context.GetAthleteStatsByUserId(athlete.UserId);

                var athleticHistory = await _context.GetAthleticHistorysByUserId(athlete.UserId);

                var athleteVideos = await _context.GetAthleteVideosByUserId(athlete.UserId);

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

                return Ok(new Response<decimal> { Payload = percent });
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("freeconsultation/{fullName}")]
        public async Task<ActionResult> SendFreeConsultationEmailAsync(Request<string> request, string fullName)
        {
            try
            {
                await EmailClient.SendEmailFreeConsultationAsync(
                    new EmailContact { Email = "contact@isportsrecruiting.com", Name = fullName },
                    "Free Consultation", request.Payload);

                return Ok();
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpGet("favorites/{coachId:int}")]
        public async Task<ActionResult> GetFavorites(int coachId)
        {
            try
            {
                var ids = await _context.GetFavoriteAthletesByCoachId(coachId);
                return Ok(new Response<IEnumerable<int>> { Payload = ids });
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<int>>
                    { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpGet("favorites/{coachId:int}/count")]
        public async Task<ActionResult> GetFavoritesCount(int coachId)
        {
            try
            {
                var count = await _context.GetCountFavoriteAthletesByCoachId(coachId);
                return Ok(new Response<int> { Payload = count });
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<int>>
                    { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpPost("favorites/{coachId:int}/{athleteId:int}")]
        public async Task<ActionResult> SetFavorite(int coachId, int athleteId)
        {
            try
            {
                await _context.AddFavoriteAthlete(coachId, athleteId);

                return Ok();
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<int>>
                    { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpDelete("favorites/{coachId:int}/{athleteId:int}")]
        public async Task<ActionResult> RemoveFavorite(int coachId, int athleteId)
        {
            try
            {
                await _context.RemoveFavoriteAthlete(coachId, athleteId);

                return Ok(new Response<IEnumerable<int>>());
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<int>>
                    { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }

        [HttpPost("stats/{userId}")]
        public async Task<ActionResult> SetAthleteStats(int userId, AthleteStatsModel stats)
        {
            try
            {
                await _context.SetAthleteStatsAsync(userId, stats);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpPost("honor/{userId}")]
        public async Task<ActionResult> SetAthleteHonor(int userId, AthleteHonorsModel honor)
        {
            try
            {
                var id = await _context.SetAthleteHonorAsync(userId, honor);

                return Ok(new Response<int>(id));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpDelete("honor/{id}")]
        public async Task<ActionResult> DeleteAthleteHonor(int id)
        {
            try
            {
                await _context.RemoveAthleteHonorAsync(id);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpPost("award/{userId}")]
        public async Task<ActionResult> SetAthleteAward(int userId, AthleteAwardsModel award)
        {
            try
            {
                var id = await _context.SetAthleteAwardAsync(userId, award);

                return Ok(new Response<int>(id));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpDelete("award/{id}")]
        public async Task<ActionResult> DeleteAthleteAward(int id)
        {
            try
            {
                await _context.RemoveAthleteAwardAsync(id);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpPost("highschool/{userId}")]
        public async Task<ActionResult> SetAthleteHighSchool(int userId, AthleteHighSchoolModel highSchool)
        {
            try
            {
                var id = await _context.SetAthleteHighSchoolAsync(userId, highSchool);

                return Ok(new Response<int>(id));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpDelete("highschool/{id}")]
        public async Task<ActionResult> DeleteAthleteHighSchool(int id)
        {
            try
            {
                await _context.RemoveAthleteHighSchoolAsync(id);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpPost("club/{userId}")]
        public async Task<ActionResult> SetAthleteClub(int userId, AthleteClubsModel club)
        {
            try
            {
                var id = await _context.SetAthleteClubAsync(userId, club);

                return Ok(new Response<int>(id));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpDelete("club/{id}")]
        public async Task<ActionResult> DeleteAthleteClub(int id)
        {
            try
            {
                await _context.RemoveAthleteClubAsync(id);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpPost("coach/{userId}")]
        public async Task<ActionResult> SetAthleteCoach(int userId, AthleteCoachInfoModel coach)
        {
            try
            {
                var id = await _context.SetAthleteCoachAsync(userId, coach);

                return Ok(new Response<int>(id));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpDelete("coach/{id}")]
        public async Task<ActionResult> DeleteAthleteCoach(int id)
        {
            try
            {
                await _context.RemoveAthleteCoachAsync(id);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpPost("video/{userId}")]
        public async Task<ActionResult> SetAthleteVideo(int userId, AthleteVideosModel video)
        {
            try
            {
                var id = await _context.SetAthleteVideoAsync(userId, video);

                return Ok(new Response<int>(id));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpDelete("video/{id}")]
        public async Task<ActionResult> DeleteAthleteVideo(int id)
        {
            try
            {
                await _context.RemoveAthleteVideoAsync(id);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpPost("score")]
        public async Task<ActionResult> UploadScore(FileModel fileModel)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(fileModel.Base64))
                {
                    var files = await _context.GetAthleteFile(fileModel.RelateId);
                    var fileExists = files.FirstOrDefault(f =>
                        f.Description.Contains(fileModel.Description, StringComparison.InvariantCultureIgnoreCase));

                    var extension = fileModel.File_Name.Split('.').Last();
                    fileModel.Description += $".{extension}";

                    if (fileExists is not null)
                        await _context.UpdateAthleteFile(fileModel);
                    else
                        await _context.SetAthleteFile(fileModel);

                    var bytes = Convert.FromBase64String(fileModel.Base64);

                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var path = $"{baseDirectory}\\wwwroot\\uploads\\{fileModel.Description}";

                    await using var file = new FileStream(path, FileMode.Create);
                    file.Write(bytes, 0, bytes.Length);
                    file.Flush();

                    if (fileExists is not null)
                        System.IO.File.Delete($"{baseDirectory}\\wwwroot\\uploads\\{fileExists.Description}");
                }

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpGet("file/download/{description}")]
        public async Task<ActionResult> DownloadFile(string description)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var path = $"{baseDirectory}\\wwwroot\\uploads\\{description}";

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(path);

            return File(fileBytes, "application/force-download", description);
        }

        [HttpGet("tut/{id}/disable")]
        public async Task<ActionResult> DisableTut(int id)
        {
            try
            {
                await _context.DisableAthleteTut(id);
                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }
        
        [HttpGet("tut/{id}/enable")]
        public async Task<ActionResult> EnableTut(int id)
        {
            try
            {
                await _context.EnableAthleteTut(id);
                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        private async Task<decimal> GetPercentProfile(int id)
        {
            decimal percent = 0;

            var athleteModel = await _context.GetAthleteByIdAsync(id);
            var athlete = await athleteModel.ToDtoAsync(_context);

            var athleteStatsModel = await _context.GetAthleteStatsByUserId(athlete.UserId);

            var athleticHistory = await _context.GetAthleticHistorysByUserId(athlete.UserId);

            var athleteVideos = await _context.GetAthleteVideosByUserId(athlete.UserId);

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