using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Models;
using iSportsRecruiting.Server.UniversityData;
using Dapper;
using Child = iSportsRecruiting.Server.MajorsCategoryData.Child;

namespace iSportsRecruiting.Server.Controllers
{
    [AllowAnonymous]
    public class UniversityController : BaseApiController<UniversityController>
    {
        [HttpGet]
        public async Task<ActionResult> Get(int? id = null, string? search = null, int? major = null,
            string? state = null, int? page = null,
            int? take = null, string? sportName = null, string? divisions = null, string? gpa = null,
            string? act = null,
            string? sat = null, int? takeOnly = -1)
        {
            var response = new Response<IEnumerable<UniversityDTO>>();

            try
            {
                if (!(page.HasValue && take.HasValue))
                {
                    var universities = await _context.GetUniversitiesAsync(id, search, major, state, sportName,
                        divisions != null ? divisions.Split(",") : new List<string>().ToArray(), gpa, act, sat);

                    response.Payload = universities.Select(u => u.ToDTO());

                    if (takeOnly == -1)
                        return Ok(response);

                    response.Payload = response.Payload.Take(takeOnly.Value);
                    return Ok(response);
                }
                else
                {
                    var universities = await _context.GetUniversitiesAsync(major: major, sportName: sportName,
                        pagination: (search, page.Value * take.Value, take.Value), state: state,
                        divisions: divisions != null ? divisions.Split(",") : new List<string>().ToArray(),
                        gpa: gpa, act: act, sat: sat);

                    var universitiesCount = await _context.GetUniversitiesCountAsync(search, major,
                        sportName: sportName, state: state,
                        divisions: divisions != null ? divisions.Split(",") : new List<string>().ToArray(),
                        gpa: gpa, act: act, sat: sat);

                    response.Payload = universities.Select(u => u.ToDTO());
                    response.Total = universitiesCount;

                    if (takeOnly == -1)
                        return Ok(response);

                    response.Payload = response.Payload.Take(takeOnly.Value);
                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet("without-community")]
        public async Task<ActionResult> GetWithoutCommunity(int? id = null, string? search = null, int? major = null,
            string? state = null, int? page = null,
            int? take = null, string? sportName = null, string? divisions = null, string? gpa = null,
            string? act = null,
            string? sat = null, int? takeOnly = -1, bool? randomizer = false)
        {
            var response = new Response<IEnumerable<UniversityDTO>>();

            try
            {
                if (!(page.HasValue && take.HasValue))
                {
                    var universities = await _context.GetUniversitiesAsync(id, search, major, state, sportName,
                        divisions != null ? divisions.Split(",") : new List<string>().ToArray(),
                        gpa, act, sat, (search, page.Value * take.Value, take.Value), true);

                    var total = await _context.GetUniversitiesCountAsync(search, major, state, sportName,
                        divisions != null ? divisions.Split(",") : new List<string>().ToArray(),
                        gpa, act, sat);

                    response.Total = total;
                    response.Payload = universities.Select(u => u.ToDTO());

                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet("divisions")]
        public async Task<ActionResult> GetDivisionsQuantity()
        {
            var response = new Response<List<DivisionCount>>
            {
                Payload = new List<DivisionCount>()
            };

            try
            {
                var universities = (await _context.GetUniversitiesAsync()).ToArray();
                var ncaa = universities.Count(u =>
                    !string.IsNullOrWhiteSpace(u.Division) &&
                    u.Division.Contains("NCAA", StringComparison.InvariantCultureIgnoreCase));
                var naia = universities.Count(u =>
                    !string.IsNullOrWhiteSpace(u.Division) &&
                    u.Division.Contains("NAIA", StringComparison.InvariantCultureIgnoreCase));
                var njca = universities.Count(u =>
                    !string.IsNullOrWhiteSpace(u.Division) &&
                    u.Division.Contains("NJCAA", StringComparison.InvariantCultureIgnoreCase));

                response.Payload.Add(new DivisionCount("NCAA", ncaa));
                response.Payload.Add(new DivisionCount("NAIA", naia));
                response.Payload.Add(new DivisionCount("NJCAA", njca));

                return Ok(response);
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet("universities-count")]
        public async Task<ActionResult> Get()
        {
            var response = new Response<IEnumerable<UniversityDTO>>();

            try
            {
                var universities = await _context.GetSportUniversitiesCount();

                var grouped = universities.Where(u =>
                    u.Sports is not null).GroupBy(u => u.Sports,
                    (s, u) =>
                        new SportUniversityCountDto
                        {
                            SportName = s.ToUpper().Replace("MEN'S", "").Replace("MENS", "").Replace("MEN", ""),
                            UniversitiesCount = u.Count()
                        }).Where(u => u.UniversitiesCount > 1).OrderByDescending(u => u.UniversitiesCount);

                return Ok(new Response<IEnumerable<SportUniversityCountDto>>(grouped));
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpPost("upload-image")]
        public async Task<ActionResult> UploadUniversityLogo(FileModel fileModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileModel.Base64))
                    return Ok(new Response());

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                var extension = fileModel.File_Name.Split('.').Last();

                fileModel.Description = fileModel.Description.Replace(extension, string.Empty);

                string logo = await _context.GetUniversityLogo(fileModel);

                System.IO.File.Delete($"{baseDirectory}\\wwwroot\\images\\logos\\{logo}.{extension}");

                await _context.SetUniversityLogo(fileModel);

                var bytes = Convert.FromBase64String(fileModel.Base64);

                var path = $"{baseDirectory}\\wwwroot\\images\\logos\\{fileModel.Description}.{extension}";

                await using var file = new FileStream(path, FileMode.Create);
                file.Write(bytes, 0, bytes.Length);
                file.Flush();

                return Ok(new Response());
            }
            catch (Exception e)
            {
                return Ok(new Response { Message = e.Message, Status = ResponseStatus.Error });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _context.RemoveUniversityAsync(id);

            return Ok();
        }

        [HttpGet("blastemail")]
        public ActionResult GetBlastEmailUniversities(string? sportName = null, string? divisions = null,
            string? gpa = null, string? act = null, string? sat = null, string? state = null)
        {
            var result = _context.GetUniversitiesForBlastEmail(sportName,
                divisions != null ? divisions.Split(",") : new List<string>().ToArray(), gpa, act, sat, state);
            return Ok(result);
        }

        [HttpGet("qualify")]
        public async Task<ActionResult> GetQualifyUniversities(string? sportName, string? gpa, string? search = null, int? page = null, int? take = null)
        {
            try
            {
                var result = await _context.GetUniversitiesQualify(sportName, gpa, search,
                    take is null ? 0 : page * take.Value, int.MaxValue);

                return Ok(new Response<UniversityModel[]>
                {
                    Payload = take is not null ? result.Take(take.Value).ToArray() : result,
                    Total = result.Length
                });
            }
            catch (Exception e)
            {
                return Ok(new Response<UniversityModel[]>
                {
                    Status = ResponseStatus.InternalError,
                    Message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("blastemailcount/{userId:int}")]
        public async Task<ActionResult> GetBlastEmailUniversitiesCount(int userId)
        {
            try
            {
                var settings = await _context.GetAthleteSettingsAsync(userId);
                var sportId = await _context.GetAthleteSportIdAsync(userId);
                var sportName = await _context.GetSportNameAsync(sportId);

                var average = settings.Average.Split(",");
                var gpa = average[0];
                var act = average[1];
                var sat = average[2];

                var result =
                    await _context.GetUniversitiesForBlastEmail(sportName, settings.Divisions.Split(","), gpa, act, sat,
                        null);
                return Ok(new Response<int> { Payload = result.Length });
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet("favorites/{athleteId:int}")]
        public async Task<ActionResult> GetFavoriteUniversities(int athleteId, int page = 1, int take = 10)
        {
            try
            {
                var favorites = await _context.GetFavoriteUniversitiesByAthleteId(athleteId);
                var dtos = favorites.Select(f => f.ToDTO());

                var total = await _context.GetCountFavoriteUniversitiesByAthleteId(athleteId);
                return Ok(new Response<IEnumerable<UniversityDTO>> { Payload = dtos, Total = total });
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Ok(new Response<IEnumerable<int>> { Status = ResponseStatus.Error });
        }

        [HttpGet("favorites/{athleteId:int}/count")]
        public async Task<ActionResult> GetFavoriteUniversitiesCount(int athleteId)
        {
            try
            {
                var count = await _context.GetCountFavoriteUniversitiesByAthleteId(athleteId);
                return Ok(new Response<int> { Payload = count });
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpPost("favorites/{athleteId:int}/{universityId:int}/{plan}")]
        public async Task<ActionResult> SetFavoriteUniversity(int athleteId, int universityId, string? plan)
        {
            try
            {
                await _context.AddFavoriteUniversity(athleteId, universityId);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
                return Problem();
            }
        }

        [HttpDelete("favorites/{athleteId:int}/{universityId:int}/{plan}")]
        public async Task<ActionResult> RemoveFavoriteUniversity(int athleteId, int universityId, string? plan)
        {
            try
            {
                if (plan == "AA")
                {
                    var count = await _context.GetCountFavoriteUniversitiesByAthleteId(athleteId);

                    if (count >= 25)
                    {
                        return Ok(new Response
                        {
                            Status = ResponseStatus.InternalError,
                            Message =
                                "You can't change favorite universities, upgrade your account if you want access to unlimited universities!"
                        });
                    }
                }

                await _context.RemoveFavoriteUniversity(athleteId, universityId);

                return Ok(new Response());
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
                return Problem();
            }
        }

        [HttpGet]
        [Route("settings/{id:int}")]
        public async Task<ActionResult> GetAthleteSettings(int id)
        {
            try
            {
                var athleteSetting = await _context.GetAthleteSettingsAsync(id);

                if (athleteSetting is null)
                    return Ok(new Response());

                return Ok(new Response<AthleteSettingsDTO>
                {
                    Payload = new AthleteSettingsDTO
                    {
                        Average = athleteSetting.Average,
                        Divisions = athleteSetting.Divisions
                    }
                });
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet("contact/{id}")]
        public async Task<ActionResult> GetSportContactByUniversity(int? id = null)
        {
            var model = await _context.GetSportsContactByUniversityAsync(id);
            var responseDTO = model.Select(m => m.ToDTO());

            return Ok(new Response<IEnumerable<SportContactDTO>>
            {
                Payload = responseDTO
            });
        }

        [HttpPut]
        [Route("update-settings/{id:int}")]
        public async Task<ActionResult> InsertOrUpdateAthleteSettings(int id, string? divisions, string? average)
        {
            try
            {
                await _context.InsertOrUpdateAthleteSettings(id, divisions, average);

                return Ok();
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpPut]
        public async Task<ActionResult> Put(UniversityDTO university)
        {
            try
            {
                var result = await _context.UpdateUniversityAsync(university.ToModel());

                if (result == 1)
                    return Ok(new Response());
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpPost]
        public async Task<ActionResult> Post(UniversityDTO university)
        {
            try
            {
                var result = await _context.UpdateUniversityAsync(university.ToModel());

                if (result == 1)
                    return Ok(new Response());
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet]
        [Route("count")]
        public async Task<ActionResult> Count(string? search = null, int? major = null)
        {
            try
            {
                var count = await _context.GetUniversitiesCountAsync(search, major);

                return Ok(new Response<int>
                {
                    Payload = count
                });
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet]
        [Route("ping")]
        public async Task<ActionResult> Ping()
        {
            int count = 0;
            HttpClient client = new();
            List<UniversityDTO> universitiesToUpdate = new();

            try
            {
                var universities = await _context.GetUniversitiesAsync();

                foreach (var university in universities)
                {
                    count++;
                    try
                    {
                        var response = await client.GetAsync(university.Website);
                        if (!response.IsSuccessStatusCode)
                        {
                            universitiesToUpdate.Add(university.ToDTO());
                        }
                    }
                    catch (Exception e)
                    {
                        universitiesToUpdate.Add(university.ToDTO());
                    }
                }

                return Ok(new Response<IEnumerable<UniversityDTO>>
                {
                    Total = universitiesToUpdate.Count,
                    Payload = universitiesToUpdate
                });
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet]
        [Route("nonrecognized")]
        public async Task<ActionResult> NonRecognized()
        {
            try
            {
                var collegeDataText =
                    await System.IO.File.ReadAllTextAsync(@"C:\Users\Jeremi\Desktop\collegedata.json");
                var collegeData =
                    JsonSerializer.Deserialize<ResponseTemporal<IEnumerable<University>>>(collegeDataText);

                var universities = await _context.GetUniversitiesAsync();

                var nonRecognizeUniversities =
                    universities.Where(u => !collegeData.Results.Any(c => c.Name == u.University)).ToList();

                await System.IO.File.WriteAllTextAsync(@"C:\Users\Jeremi\Desktop\notfounduniversities.txt",
                    string.Join("\n------\n",
                        nonRecognizeUniversities.Select(u => { return $"ID: {u.Id}\nNAME: {u.University}"; })));
            }
            catch (Exception e)
            {
            }

            return Ok();
        }

        [HttpGet]
        [Route("recognized")]
        public async Task<ActionResult> Recognized()
        {
            int count = 0;
            HttpClient http = new();
            Queue<University> slugList = new();

            var universities = await _context.GetUniversitiesAsync();

            foreach (var university in universities)
            {
                count++;

            TRY:
                try
                {
                    var stream = await http.GetStreamAsync(
                        $"https://www.collegedata.com/cdapi/chances/search?q={WebUtility.UrlEncode(university.University)}&pageNum=0&pageSize=10");
                    var search = await JsonSerializer.DeserializeAsync<Search>(stream);

                    if (search.Total > 0)
                    {
                        search.Results[0].Name = university.University;
                        slugList.Enqueue(new University { Name = search.Results[0].Name, Slug = search.Results[0].Slug });
                    }
                    else
                    {
                        if (university.University.Contains(" at"))
                        {
                            university.University = university.University.Replace(" at", ",");
                            goto TRY;
                        }
                        else if (university.University.Contains(","))
                        {
                            university.University = university.University.Replace(",", " in");
                            goto TRY;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (university.University.Contains(" at"))
                    {
                        university.University = university.University.Replace(" at", ",");
                        goto TRY;
                    }
                    else if (university.University.Contains(","))
                    {
                        university.University = university.University.Replace(",", " in");
                        goto TRY;
                    }
                }
            }

            return Ok(new Response<IEnumerable<University>>
            {
                Total = slugList.Count,
                Payload = slugList
            });
        }

        [HttpGet]
        [Route("relation")]
        public async Task<ActionResult> Relation()
        {
            HttpClient http = new();

            try
            {
                var universities = (await _context.GetUniversitiesAsync())
                    .Select(u =>
                    {
                        u.University = CleanName(u.University);
                        return u;
                    }).ToArray();

                var allMajors = (await _context.Connection.QueryAsync<MajorModel>("SELECT * FROM majors")).ToArray();

                var collegeDataText =
                    await System.IO.File.ReadAllTextAsync(@"C:\Users\USUARIO\Desktop\collegedata.json");
                var collegeData = JsonSerializer.Deserialize<ResponseTemporal<IEnumerable<University>>>(collegeDataText)
                    ?.Results.Select(
                        c =>
                        {
                            c.Name = CleanName(c.Name);
                            c.Slug = CleanName(c.Slug);
                            return c;
                        }).Reverse();

                var values = string.Empty;

                var universitiesMajor =
                    (await _context.Connection.QueryAsync<UniversityMajorModel>("SELECT * FROM university_major"))
                    .Select(um => um.University_Id).ToArray();

                foreach (var university in collegeData?.Where(u => !string.IsNullOrWhiteSpace(u.Slug)) ??
                                           Array.Empty<University>())
                {
                    Stream academicStream = null;

                TRY:
                    try
                    {
                        academicStream = await http.GetStreamAsync(
                            $"https://www.collegedata.com/_next/data/YKqQo-TdZUWSgNmUhPpPY/college-search/{university.Slug}/academics.json");
                    }
                    catch (Exception e)
                    {
                        if (e.HResult == -2146233088)
                        {
                            await Task.Delay(10000);
                            goto TRY;
                        }
                    }

                    var academic = await JsonSerializer.DeserializeAsync<Academic>(academicStream);

                    if (academic?.PageProps.Profile != null)
                    {
                        var universityMajors = academic.PageProps.Profile.BodyContent[0].Data.Children[0].Data.Data?
                            .EnumerateArray().Select(u => u.GetString()).ToArray();

                        if (universityMajors != null)
                        {
                            var majorsToRelate = allMajors.Where(m => universityMajors.Contains(m.Name)).ToArray();

                            var dbUniversity = universities.FirstOrDefault(u =>
                                university.Name == u.University ||
                                university.Slug == MakeSlug(u.University));

                            if (dbUniversity != null && !universitiesMajor.Contains(dbUniversity.Id))
                            {
                                values +=
                                    $"({dbUniversity.Id}, '{string.Join(",", majorsToRelate.Select(m => m.Id))}'), ";
                            }
                        }
                    }
                }

                values = values[..^2];
                await _context.Connection.ExecuteAsync($"INSERT INTO university_major (university_id, majors) " +
                                                       $"VALUES {values}");
            }
            catch (Exception e)
            {
            }

            return Ok();
        }

        [HttpGet]
        [Route("majors/{universityId:int}")]
        public async Task<ActionResult> Majors(int universityId)
        {
            try
            {
                var universityMajors = await _context.GetMajorsByUniversityId(universityId);
                if (universityMajors is null)
                    return Ok("This university doesnt have any majors");

                int[] idsArray = null;
                if (!string.IsNullOrWhiteSpace(universityMajors.Majors))
                {
                    idsArray = universityMajors.Majors.Split(",").Select(n =>
                        int.TryParse(n, out var id) ? id : 0).Where(n => n != 0).ToArray();
                }

                var majors = (await _context.GetMajorsAsync(idsArray)).ToArray();
                var response = new Response<IEnumerable<MajorModel>>
                {
                    Payload = majors.OrderBy(m => m.Name),
                    Total = majors.Count()
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet]
        [Route("majors")]
        public async Task<ActionResult> GetMajors()
        {
            try
            {
                var majors = (await _context.GetMajorsByCategoryIdAsync(null)).ToArray();
                var response = new Response<IEnumerable<MajorModel>>
                {
                    Payload = majors.OrderBy(m => m.Name),
                    Total = majors.Length
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet]
        [Route("majors/category/{ids}")]
        public async Task<ActionResult> MajorsByCategoryId(string? ids)
        {
            try
            {
                var majors = (await _context.GetMajorsByCategoryIdAsync(ids)).ToArray();
                var response = new Response<IEnumerable<MajorModel>>
                {
                    Payload = majors.OrderBy(m => m.Name),
                    Total = majors.Length
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet("majors/category")]
        public async Task<ActionResult> MajorsCategory()
        {
            try
            {
                var categories = (await _context.Connection
                    .QueryAsync<CategoryModel>("SELECT * FROM majors_category")).ToArray();
                return Ok(new Response<IEnumerable<CategoryModel>>
                {
                    Payload = categories,
                    Total = categories.Length
                });
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpGet("majors/university")]
        public async Task<ActionResult> MajorUniversity()
        {
            try
            {
                var categories = (await _context.Connection
                    .QueryAsync<MajorUniversityModel>("SELECT * FROM university_major")).ToArray();
                return Ok(new Response<IEnumerable<MajorUniversityModel>>
                {
                    Payload = categories,
                    Total = categories.Length
                });
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        private static IEnumerable<string> GetAllChildrens(Child[] childs)
        {
            if (childs is null)
                yield break;

            foreach (var child in childs)
            {
                yield return child.TopCheckBox.Label.ToLower();

                if (child.Children is null)
                    continue;

                foreach (var child2 in child.Children)
                {
                    yield return child2.TopCheckBox.Label.ToLower();

                    if (child2.Children is null)
                        continue;

                    foreach (var child3 in child2.Children)
                        yield return child3.TopCheckBox.Label.ToLower();
                }
            }
        }


        private static string MakeSlug(string name)
        {
            return name.Replace(" ", "-");
        }

        private static string CleanName(string name)
        {
            return name.Replace("'", "").Replace("_", "")
                .Replace(",", "").Replace(".", "").Replace("´", "").Replace("`", "")
                .Replace("+", "").Replace("(", "").Replace(")", "").Replace("/", "")
                .Replace("=", "").Replace("$", "").Replace("@", "").Replace("|", "")
                .Replace("&", "").Replace("~", "").ToLower();
        }
    }
}

namespace iSportsRecruiting.Server.MajorsCategoryData
{
    public partial class Temperatures
    {
        [JsonPropertyName("searchBarPrompt")] public string SearchBarPrompt { get; set; }

        [JsonPropertyName("children")] public Child[] Children { get; set; }
    }

    public partial class Child
    {
        [JsonPropertyName("topCheckBox")] public TopCheckBox TopCheckBox { get; set; }

        [JsonPropertyName("children")] public Child[] Children { get; set; }
    }

    public partial class TopCheckBox
    {
        [JsonPropertyName("value")] public string Value { get; set; }

        [JsonPropertyName("label")] public string Label { get; set; }
    }
}

namespace iSportsRecruiting.Server.UniversityData
{
    public partial class Academic
    {
        [JsonPropertyName("pageProps")] public PageProps PageProps { get; set; }

        [JsonPropertyName("__N_SSG")] public bool NSsg { get; set; }
    }

    public partial class PageProps
    {
        [JsonPropertyName("profile")] public Profile Profile { get; set; }
    }

    public partial class Profile
    {
        [JsonPropertyName("id")] public long Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("slug")] public string Slug { get; set; }

        [JsonPropertyName("website")] public Uri Website { get; set; }

        [JsonPropertyName("campusMapUrl")] public Uri CampusMapUrl { get; set; }

        [JsonPropertyName("chance")] public object Chance { get; set; }

        [JsonPropertyName("headerCardContent")]
        public HeaderCardContent[] HeaderCardContent { get; set; }

        [JsonPropertyName("bodyContent")] public BodyContent[] BodyContent { get; set; }
    }

    public partial class BodyContent
    {
        [JsonPropertyName("type")] public string Type { get; set; }

        [JsonPropertyName("data")] public BodyContentData Data { get; set; }
    }

    public partial class BodyContentData
    {
        [JsonPropertyName("title")] public string Title { get; set; }

        [JsonPropertyName("iconType")] public object IconType { get; set; }

        [JsonPropertyName("link")] public object Link { get; set; }

        [JsonPropertyName("children")] public Child[] Children { get; set; }
    }

    public partial class Child
    {
        [JsonPropertyName("type")] public object Type { get; set; }

        [JsonPropertyName("data")] public ChildData Data { get; set; }
    }

    public partial class ChildData
    {
        [JsonPropertyName("title")] public string Title { get; set; }

        [JsonPropertyName("buttonText")] public string ButtonText { get; set; }

        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public JsonElement? Data { get; set; }
    }

    public struct DatumElement
    {
        public DatumClass DatumClass;
        public string String;
    }

    public partial class DatumClass
    {
        [JsonPropertyName("label")] public string Label { get; set; }

        [JsonPropertyName("value")] public long Value { get; set; }
    }

    public partial class HeaderCardContent
    {
        [JsonPropertyName("type")] public string Type { get; set; }

        [JsonPropertyName("data")] public HeaderCardContentData Data { get; set; }
    }

    public partial class HeaderCardContentData
    {
        [JsonPropertyName("value")] public object Value { get; set; }

        [JsonPropertyName("title")] public string Title { get; set; }

        [JsonPropertyName("text")] public string Text { get; set; }

        [JsonPropertyName("link")] public string Link { get; set; }
    }

    public partial class Search
    {
        [JsonPropertyName("results")] public Result[] Results { get; set; }

        [JsonPropertyName("total")] public long Total { get; set; }
    }

    public partial class Result
    {
        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("id")] public long Id { get; set; }

        [JsonPropertyName("slug")] public string Slug { get; set; }

        [JsonPropertyName("address")] public Address Address { get; set; }
    }

    public partial class Address
    {
        [JsonPropertyName("street1")] public string Street1 { get; set; }

        [JsonPropertyName("street2")] public object Street2 { get; set; }

        [JsonPropertyName("city")] public string City { get; set; }

        [JsonPropertyName("state")] public string State { get; set; }
    }

    public class CollegeData
    {
        [JsonPropertyName("total")] public int Total { get; set; }
        [JsonPropertyName("results")] public University[] Results { get; set; }
    }

    public class University
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("slug")] public string Slug { get; set; }
    }

    public class ResponseTemporal<T>
    {
        [JsonPropertyName("results")] public T Results { get; set; }
        [JsonPropertyName("total")] public int? Total { get; set; }
    }
}