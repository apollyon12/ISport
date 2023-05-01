using Dapper;
using iSportsRecruiting.Shared.Models;
using MySql.Data.MySqlClient;

using System.Linq;
using System.Globalization;

using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Email;
using Google.Protobuf;
using Microsoft.Win32;
using System.Data;

namespace iSportsRecruiting.Shared.Services
{
    public class DatabaseContext : IDatabaseContext, IAsyncDisposable
    {
        public MySqlConnection Connection { get; set; }

        public static UniversityModel[] CachedUniversities { get; set; }

        public static UniversityContactModel[] CachedUniversitiesContact { get; set; }

        public DatabaseContext()
        {
            Connection =
                new MySqlConnection("server=my01.winhost.com;uid=isradmin;pwd=Vemex1233;database=mysql_146323_isr");
            //Connection = new MySqlConnection("server=localhost;port=3306;uid=root;pwd=Password19;database=mysql_146323_isr");

            if (CachedUniversities is null)
                GetCache();
        }

        public ValueTask DisposeAsync()
        {
            return Connection.DisposeAsync();
        }

        public async Task GetCache()
        {
            var universities = await Connection.QueryAsync<UniversityModel>("SELECT * FROM university");
            CachedUniversities = universities.ToArray();

            var universitiesContact = await Connection.QueryAsync<UniversityContactModel>(
                "SELECT university_id, coach, email, sports, assisantcoach, asscoachemail from university_contact");
            CachedUniversitiesContact = universitiesContact.Select(uc =>
            {
                if (uc.Sports is not null)
                    uc.Sports = uc.Sports.Replace("'", "").ToUpper();

                return uc;
            }).ToArray();
        }

        public async Task<int> AddAthleteService(int athleteId, int serviceId)
        {
            try
            {
                return await Connection.QuerySingleAsync<int>("INSERT INTO athlete_services " +
                                                              "(athlete_id, service_id) " +
                                                              $"VALUES " +
                                                              $"({athleteId}, {serviceId}); " +
                                                              $"SELECT LAST_INSERT_ID();");
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> CheckIfServiceExists(int athleteId, int serviceId)
        {
            try
            {
                return await Connection.QuerySingleAsync<int>(
                    $"SELECT COUNT(*) FROM athlete_services WHERE athlete_id = {athleteId} AND service_id = {serviceId}");
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> MarkServiceAsUsed(int athleteId, int serviceId)
        {
            try
            {
                return await Connection.QuerySingleAsync<int>(
                    $"DELETE FROM athlete_services WHERE athlete_id = {athleteId} AND service_id = {serviceId}");
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> AddNotificationAsync(Notification notification)
        {
            try
            {
                return await Connection.QuerySingleAsync<int>("INSERT INTO notification " +
                                                              "(entity_id, icon, title, body, link, type, image, external_relate_id) " +
                                                              $"VALUES " +
                                                              $"({notification.Entity_Id}, \"{notification.Icon}\", \"{notification.Title}\", " +
                                                              $"\"{notification.Body}\", \"{notification.Link}\", \"{notification.Type}\", " +
                                                              $"\"{notification.Image}\", \"{notification.External_Relate_Id}\"); " +
                                                              $"SELECT LAST_INSERT_ID();");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> AddScheduledNotificationAsync(ScheduledNotificationDTO notification)
        {
            //CultureInfo culture = new CultureInfo("en-US");
            //var date = Convert.ToDateTime(notification.Date, culture);
            try
            {
                var query = notification.DayOfTheMonth == null ? "INSERT INTO schedulle_notifcation " +
                                                              "(entities, send_sms, send_email, daily, content, dayoftheweek, is_deleted) " +
                                                              $"VALUES " +
                                                              $"(\"{notification.To}\", {notification.Send_Sms}, {notification.Send_Email}, {notification.Daily}," +
                                                              $"\"{notification.Message}\", \"{notification.DayOfTheWeek}\", {false})"
                                                                : "INSERT INTO schedulle_notifcation " +
                                                              "(entities, send_sms, send_email, daily, content, dayoftheweek, dayofthemonth, is_deleted) " +
                                                              $"VALUES " +
                                                              $"(\"{notification.To}\", {notification.Send_Sms}, {notification.Send_Email}, {notification.Daily}," +
                                                              $"\"{notification.Message}\", \"{notification.DayOfTheWeek}\", {notification.DayOfTheMonth}, {false})";
                return await Connection.ExecuteAsync(query);

            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                throw new Exception(e.Message);
                return default;
            }
        }
        public async Task<IEnumerable<ScheduledNotification>> GetScheduledNotificationAsync()
        {
            try
            {
                var response = await Connection.QueryAsync<ScheduledNotification>($"SELECT * FROM schedulle_notifcation " +
                                                                                    $"WHERE is_deleted = {false}");
                return response;
            }
            catch
            {
                return default;
            }
        }
        public Task<int> DeleteScheduledNotificationAsync(int id)
        {
            try
            {
                return Connection.ExecuteAsync($"UPDATE schedulle_notifcation SET " +
                                                $"is_deleted = {true} " +                                                
                                                $"WHERE id = {id}");
            }
            catch
            {
                return default;
            }
        }

        public async Task<IEnumerable<Notification>> GetNotificationByUserIdAsync(int userId)
        {
            try
            {
                return await Connection.QueryAsync<Notification>(
                    $"SELECT * FROM notification WHERE entity_id = {userId}");
            }
            catch
            {
                return default;
            }
        }

        public Task<int> DeleteNotificationAsync(int senderId, int receiverId)
        {
            try
            {
                return Connection.ExecuteAsync(
                    $"DELETE FROM notification WHERE entity_id = {receiverId} AND external_relate_id = {senderId}");
            }
            catch
            {
                return default;
            }
        }

        public async Task<UserSecurity> GetUserSecurityByUserIdAsync(int userId, int userType)
        {
            try
            {
                return await Connection.QueryFirstAsync<UserSecurity>(
                    $"SELECT * FROM users_password WHERE user_id = {userId} AND user_type = {userType}");
            }
            catch
            {
                return default;
            }
        }

        public async Task<UserSecurity> GetUserSecurityByPasswordAsync(int userId, int userType, string cipher)
        {
            try
            {
                return await Connection.QueryFirstAsync<UserSecurity>(
                    $"SELECT * FROM users_password WHERE user_id = {userId} AND user_type = {userType} AND cipher = \"{cipher}\"");
            }
            catch
            {
                return default;
            }
        }

        public async Task<int> AddUserSecurityAsync(UserSecurity security)
        {
            try
            {
                await Connection.ExecuteAsync($"DELETE FROM users_password WHERE user_id = {security.User_Id}");

                return await Connection.ExecuteAsync("INSERT INTO users_password " +
                                                     "(user_id, user_type, cipher) " +
                                                     $"VALUES ({security.User_Id}, {security.User_Type}, '{security.Cipher}')");
            }
            catch
            {
                return default;
            }
        }

        public async Task<string> AddConfirmationLink(ConfirmEmail confirm)
        {
            try
            {
                var currentTicks = DateTime.Now.Ticks;
                var expiredConfirmationLinks =
                    await Connection.QueryAsync<int>($"SELECT id FROM confirm_email WHERE date_ticks < {currentTicks}");
                if (expiredConfirmationLinks is not null && expiredConfirmationLinks.Any())
                {
                    await Connection.ExecuteAsync(
                        $"DELETE FROM confirm_email WHERE id IN ({string.Join(",", expiredConfirmationLinks)})");
                }

                await Connection.ExecuteAsync("INSERT INTO confirm_email " +
                                              "(token, university_id, contact_id, is_coach, date_ticks, sport_id, full_name, email) " +
                                              $"VALUES (\"{confirm.Token}\", {confirm.University_Id}, {confirm.Contact_Id}, {confirm.Is_Coach}, {confirm.Date_Ticks}, " +
                                              $"{confirm.Sport_Id}, \"{confirm.Full_Name}\", \"{confirm.Email}\")");

                return confirm.Token;
            }
            catch
            {
                return default;
            }
        }

        public async Task<ConfirmEmail> GetConfirmationLink(string token)
        {
            try
            {
                //Esperamos que elimine los vencidos
                var currentTicks = DateTime.Now.Ticks;
                var expiredConfirmationLinks =
                    await Connection.QueryAsync<int>($"SELECT id FROM confirm_email WHERE date_ticks < {currentTicks}");
                if (expiredConfirmationLinks is not null && expiredConfirmationLinks.Any())
                {
                    await Connection.ExecuteAsync(
                        $"DELETE FROM confirm_email WHERE id IN ({string.Join(",", expiredConfirmationLinks)})");
                }

                return await Connection.QueryFirstAsync<ConfirmEmail>(
                    $"SELECT * FROM confirm_email WHERE token = \"{token}\"");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<ConfirmEmail> DeleteConfirmationLink(int id)
        {
            try
            {
                return await Connection.QueryFirstAsync<ConfirmEmail>($"DELETE FROM confirm_email WHERE id = {id}");
            }
            catch
            {
                return default;
            }
        }

        public async Task<CoachDTO> RegisterCoachAsync(CoachDTO register)
        {
            int universityContactId;

            await Connection.ExecuteAsync("INSERT INTO users (username, email) " +
                                          $"VALUES ('{(register.FirstName + register.LastName).ToLower()}', \"{register.Email}\")");

            var user = await GetUserByEmailOrDefaultAsync(register.Email);

            if (register.UniversityContactId == 0)
            {
                try
                {
                    universityContactId =
                        await Connection.QueryFirstAsync<int>(
                            $"SELECT id FROM university_contact WHERE email = '{register.Email}'");
                }
                catch
                {
                    universityContactId = 0;
                }
            }
            else
            {
                universityContactId = register.UniversityContactId;
            }


            if (universityContactId < 1)
            {
                if (register.IsCoach)
                {
                    var sportName = await GetSportNameAsync(register.SportId);
                    await Connection.ExecuteAsync(
                        $"INSERT INTO university_contact (university_id, sports, coach, email) VALUES ({register.UniversityId},\"{sportName}\",\"{register.FirstName} {register.LastName}\", \"{register.Email}\")");
                    universityContactId =
                        await Connection.QueryFirstAsync<int>(
                            $"SELECT id FROM university_contact WHERE email = '{register.Email}'");
                }
                else
                {
                    var sportName = await GetSportNameAsync(register.SportId);
                    await Connection.ExecuteAsync(
                        $"INSERT INTO university_contact (university_id, sports, ASSISANTCOACH, ASSCOACHEMAIL) VALUES ({register.UniversityId},\"{sportName}\",\"{register.FirstName} {register.LastName}\", \"{register.Email}\")");
                    universityContactId =
                        await Connection.QueryFirstAsync<int>(
                            $"SELECT id FROM university_contact WHERE email = '{register.Email}'");
                }
            }
            else
            {
                if (register.IsCoach)
                {
                    await Connection.ExecuteAsync($"UPDATE university_contact SET " +
                                                  $"coach = \"{register.FirstName} {register.LastName}\" " +
                                                  $",email = \"{register.Email}\" " +
                                                  $"WHERE id = {register.UniversityContactId}");
                }
                else
                {
                    await Connection.ExecuteAsync($"UPDATE university_contact SET " +
                                                  $"ASSISANTCOACH = \"{register.FirstName} {register.LastName}\" " +
                                                  $",ASSCOACHEMAIL = \"{register.Email}\" " +
                                                  $"WHERE id = {register.UniversityContactId}");
                }
            }

            await Connection.ExecuteAsync("INSERT INTO coach " +
                                          "(user_id, first_name, last_name, email, university_id, sports_id, universitycontact_id) " +
                                          $"VALUES ({user.User_Id}, '{register.FirstName}', '{register.LastName}', '{register.Email}', " +
                                          $"{register.UniversityId}, {register.SportId}, {universityContactId})");

            var coach = await GetCoachByEmailOrDefaultAsync(register.Email);

            await Connection.ExecuteAsync("INSERT INTO users_password " +
                                          "(user_id, user_type, cipher) " +
                                          $"VALUES ({coach.User_Id}, 5, '{register.Cipher}')");

            var coachDto = coach.ToDTO();
            return coachDto;
        }

        public async Task<int> AddCoachAsync(SportContactDTO coach)
        {
            return await Connection.QuerySingleAsync<int>(
                $"INSERT INTO university_contact (university_id, sports, coach, email, ASSISANTCOACH, ASSCOACHEMAIL) VALUES " +
                $"({coach.UniversityId},\"{coach.Sports}\",\"{coach.Coach}\", \"{coach.Email}\", \"{coach.AssisantCoach}\", \"{coach.AssCoachEmail}\"); SELECT LAST_INSERT_ID();");
        }

        public async Task<SportContactDTO> UpdateCoachAsync(SportContactDTO register)
        {
            await Connection.ExecuteAsync($"UPDATE university_contact SET " +
                                          $"coach = \"{register.Coach}\" " +
                                          $",email = \"{register.Email}\" " +
                                          $",ASSISANTCOACH = \"{register.AssisantCoach}\" " +
                                          $",ASSCOACHEMAIL = \"{register.AssCoachEmail}\" " +
                                          $",SPORTS = \"{register.Sports}\" " +
                                          $"WHERE id = {register.Id}");
            return register;
        }

        public Task<int> DeleteCoachAsync(int id)
        {
            return Connection.ExecuteAsync($"DELETE FROM university_contact WHERE id = {id}");
        }

        public async Task<AthleteModel> AddAthleteAsync(AthleteRegisterDto register)
        {
            await Connection.ExecuteAsync("INSERT INTO users (username, email) " +
                                          $"VALUES ('{(register.Info.FirstName + register.Info.LastName).ToLower()}', \"{register.Info.Email}\")");

            var user = await GetUserByEmailOrDefaultAsync(register.Info.Email);

            await Connection.ExecuteAsync("INSERT INTO athletes_profile " +
                                          "(user_id, first_name, last_name, email, position_id, position2_id) " +
                                          $"VALUES ({user.User_Id}, '{register.Info.FirstName}', '{register.Info.LastName}', '{register.Info.Email}', {register.Info.PositionId}, {register.Info.Position2Id})");

            var athlete = await GetAthleteByEmailOrDefaultAsync(register.Info.Email);

            await Connection.ExecuteAsync("INSERT INTO users_password " +
                                          "(user_id, user_type, cipher) " +
                                          $"VALUES ({athlete.Id}, 4, '{register.Cipher}')");

            await Connection.ExecuteAsync("INSERT INTO athlete_stats_value " +
                                          "(user_id, sports_id, position_id, position2_id) " +
                                          $"VALUES ({user.User_Id}, {register.Info.SportId}, {register.Info.PositionId}, {register.Info.Position2Id})");

            return athlete;
        }

        public Task<IEnumerable<AssistantModel>> GetAssistantsAsync(int? id = null)
        {
            try
            {
                string filter = id is null ? string.Empty : $"WHERE id = {id}";
                return Connection.QueryAsync<AssistantModel>($"SELECT * from assistant {filter}");
            }
            catch
            {
                return default;
            }
        }

        public Task<int> GetAssistantsCountAsync()
        {
            try
            {
                return Connection.QueryFirstAsync<int>("SELECT COUNT(*) from assistant");
            }
            catch
            {
                return default;
            }
        }

        public Task<AssistantModel> GetAssistantByEmailAsync(string email)
        {
            try
            {
                return Connection.QueryFirstAsync<AssistantModel>($"SELECT * from assistant WHERE email = \"{email}\"");
            }
            catch
            {
                return default;
            }
        }

        public Task<int> AddAssistantAsync(AssistantModel model)
        {
            return Connection.ExecuteAsync("INSERT INTO assistant " +
                                           $"(type, full_name, email, phone, university_id, date_created) " +
                                           $"VALUES ({model.Type}, '{model.Full_Name}', '{model.Email}', " +
                                           $"'{model.Phone}', '{model.University_Id}', \"{DateTime.Now:yyyy-MM-dd}\")");
        }

        public Task<int> UpdateAssistantAsync(AssistantModel model)
        {
            return Connection.ExecuteAsync("UPDATE assistant SET " +
                                           $"type = {model.Type} " +
                                           $",full_name = \"{model.Full_Name}\" " +
                                           $",email = \"{model.Email}\" " +
                                           $",phone = \"{model.Phone}\" " +
                                           $",university_id = {model.University_Id} " +
                                           $"WHERE id = {model.Id}");
        }

        public async Task<int> DeleteAssistantAsync(int id, string email)
        {
            var resultAssistant = await Connection.ExecuteAsync($"DELETE FROM assistant WHERE id = {id}");
            var resultUser = await Connection.ExecuteAsync($"DELETE FROM users WHERE email = {email}");

            return int.Parse((resultUser == resultAssistant).ToString());

        }

        public Task<IEnumerable<PromotionModel>> GetPromotionsAsync(int? id = null)
        {
            try
            {
                string filter = id is null ? string.Empty : $"WHERE id = {id}";
                return Connection.QueryAsync<PromotionModel>($"SELECT * from promotions {filter}");
            }
            catch
            {
                return default;
            }
        }

        public Task<int> GetPromotionsCountAsync()
        {
            try
            {
                return Connection.QueryFirstAsync<int>($"SELECT COUNT(*) from promotions");
            }
            catch
            {
                return default;
            }
        }

        public Task<PromotionModel> GetPromotionByCodeAsync(string code)
        {
            try
            {
                return Connection.QueryFirstAsync<PromotionModel>($"SELECT * from promotions WHERE CODE = \"{code}\"");
            }
            catch
            {
                return default;
            }
        }

        public Task<int> AddPromotionAsync(PromotionModel model)
        {
            return Connection.ExecuteAsync("INSERT INTO promotions " +
                                           "(code, discount, description, expiration, plan_id) " +
                                           $"VALUES ('{model.Code}', {model.Discount}, \"{model.Description}\", " +
                                           $"'{model.Expiration:yyyy-MM-dd}', '{model.Plan_Id}')");
        }

        public Task<int> UpdatePromotionAsync(PromotionModel model)
        {
            return Connection.ExecuteAsync("UPDATE promotions SET " +
                                           $"code = '{model.Code}' " +
                                           $",discount = {model.Discount} " +
                                           $",description = \"{model.Description}\" " +
                                           $",expiration = \"{model.Expiration:yyyy-MM-dd}\" " +
                                           $",plan_id = '{model.Plan_Id}'" +
                                           $"WHERE id = {model.Id}");
        }

        public Task<int> DeletePromotionAsync(int id)
        {
            return Connection.ExecuteAsync($"DELETE FROM promotions WHERE id = {id}");
        }

        public Task<IEnumerable<TaskModel>> GetTasksAsync(int userId)
        {
            return Connection.QueryAsync<TaskModel>($"SELECT * from tasks WHERE user_id = {userId}");
        }

        public Task<int> GetTasksCountAsync(int userId)
        {
            return Connection.QueryFirstAsync<int>($"SELECT COUNT(*) from tasks WHERE user_id = {userId}");
        }

        public Task<int> AddTaskAsync(TaskModel model)
        {
            return Connection.ExecuteAsync("INSERT INTO tasks " +
                                           "(user_id, description, date_start, date_end, time_start, status, location, phone, email, notes, type) " +
                                           $"VALUES ({model.User_Id}, \"{model.Description}\", \"{model.Date_Start:yyyy-MM-dd}\", " +
                                           $"\"{model.Date_End:yyyy-MM-dd}\", {model.Time_Start}, {model.Status}, {model.Location}, \"{model.Phone}\", " +
                                           $"\"{model.Email}\", \"{model.Notes}\", {model.Type} )");
        }

        public Task<int> UpdateTaskAsync(TaskModel model)
        {
            return Connection.ExecuteAsync("UPDATE tasks SET " +
                                           $"user_id = {model.User_Id} " +
                                           $",description = \"{model.Description}\" " +
                                           $",date_start = \"{model.Date_Start:yyyy-MM-dd}\" " +
                                           $",date_end = \"{model.Date_End:yyyy-MM-dd}\" " +
                                           $",time_start = {model.Time_Start} " +
                                           $",status = {model.Status} " +
                                           $",location = {model.Location} " +
                                           $",phone = \"{model.Phone}\" " +
                                           $",email = \"{model.Email}\" " +
                                           $",notes = \"{model.Notes}\" " +
                                           $",type = {model.Type} " +
                                           $"WHERE id = {model.Id}");
        }

        public Task<int> DeleteTaskAsync(int id)
        {
            return Connection.ExecuteAsync($"DELETE FROM tasks WHERE id = {id}");
        }

        public Task<IEnumerable<LeadModel>> GetLeadsAsync(int? id = null)
        {
            string filter = id is null ? string.Empty : $"WHERE user_id = {id}";
            return Connection.QueryAsync<LeadModel>($"SELECT * from leads {filter}");
        }
        public Task<LeadModel> GetLeadByIdAsync(int id)
        {
            return Connection.QueryFirstAsync<LeadModel>($"SELECT * from leads WHERE id = {id}");
        }
        public async Task<bool> CheckIfLeadExists(string email)
        {
            string filter = $"WHERE email = \"{email}\"";
            var result = await Connection.QuerySingleAsync<int>($"SELECT COUNT(*) from leads {filter}");
            return result > 0;
        }

        public Task<int> GetLeadsCountAsync(int? id = null)
        {
            string filter = id is null ? string.Empty : $"WHERE user_id = {id}";
            return Connection.QueryFirstAsync<int>($"SELECT COUNT(*) from leads {filter}");
        }

        public Task<int> AddLeadAsync(LeadModel model)
        {
            return Connection.ExecuteAsync("INSERT INTO leads " +
                                           "(user_id, athlete_id, full_name, email, phone, sport, graduation_year, added_on, status, social_media, notes, gpa) " +
                                           $"VALUES ({model.User_Id}, {model.Athlete_Id}, \"{model.Full_Name}\", \"{model.Email}\", " +
                                           $"\"{model.Phone}\", {model.Sport}, {model.Graduation_Year}, \"{model.Added_On:yyyy-MM-dd}\", {model.Status}, \"{model.Social_Media}\", \"{model.Notes}\", \"{model.GPA}\")");
        }

        public Task<int> UpdateLeadAsync(LeadModel model)
        {
            return Connection.ExecuteAsync("UPDATE leads SET " +
                                           $"user_id = {model.User_Id} " +
                                           $",athlete_id = {model.Athlete_Id} " +
                                           $",full_name = \"{model.Full_Name}\" " +
                                           $",email = \"{model.Email}\" " +
                                           $",phone = \"{model.Phone}\" " +
                                           $",sport = {model.Sport} " +
                                           $",graduation_year = {model.Graduation_Year} " +
                                           $",added_on = \"{model.Added_On:yyyy-MM-dd}\" " +
                                           $",status = {model.Status} " +
                                           $",social_media = \"{model.Social_Media}\" " +
                                           $",notes = \"{model.Notes}\" " +
                                           $",gpa = \"{model.GPA}\" " +
                                           $"WHERE id = {model.Id}");
        }

        public Task<int> DeleteLeadAsync(int id)
        {
            return Connection.ExecuteAsync($"DELETE FROM leads WHERE id = {id}");
        }

        public Task<IEnumerable<UserModel>> GetUsersAsync(int? id = null,
            (string Search, int Skip, int Take)? pagination = null)
        {
            if (id.HasValue)
                return Connection.QueryAsync<UserModel>(
                    $"SELECT user_id, username, email, games FROM users WHERE ID = '{id.Value}'");

            if (pagination.HasValue)
                return Connection.QueryAsync<UserModel>(
                    $"SELECT user_id, username, email, games FROM users LIMIT {pagination.Value.Skip},{pagination.Value.Take}");

            return Connection.QueryAsync<UserModel>("SELECT user_id, username, email, games FROM users");
        }

        public Task<int> GetUsersCountAsync()
        {
            return Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM users");
        }

        public Task<IEnumerable<SportModel>> GetSportsAsync(int? id = null,
            (string Search, int Skip, int Take)? pagination = null)
        {
            if (id.HasValue)
                return Connection.QueryAsync<SportModel>(
                    $"SELECT id, sport_name, gender FROM sports_sport WHERE ID = '{id.Value}'");

            if (pagination.HasValue)
                return Connection.QueryAsync<SportModel>(
                    $"SELECT id, sport_name, gender FROM sports_sport WHERE sport_name LIKE \"%{pagination.Value.Search}%\" LIMIT {pagination.Value.Skip},{pagination.Value.Take}");

            return Connection.QueryAsync<SportModel>("SELECT id, sport_name, gender FROM sports_sport");
        }

        public Task<IEnumerable<PositionModel>> GetPositionsBySport(int id)
        {
            return Connection.QueryAsync<PositionModel>(
                $"SELECT id, positions FROM athlete_position WHERE sports_id = {id}");
        }

        public Task<IEnumerable<SportContactModel>> GetSportsContactByUniversityAsync(int? universityId = null,
            int? sportContactId = null)
        {
            if (universityId.HasValue)
                return Connection.QueryAsync<SportContactModel>(
                    $"SELECT * FROM university_contact WHERE university_id = {universityId}");
            else
                return Connection.QueryAsync<SportContactModel>(
                    $"SELECT * FROM university_contact WHERE id = {sportContactId}");
        }

        public Task<SportContactModel> GetSportsContactByUniversityAndSportAsync(int universityId, string sport)
        {
            return Connection.QueryFirstAsync<SportContactModel>(
                $"SELECT * FROM university_contact WHERE university_id = {universityId} AND sports LIKE \"{sport}\"");
        }

        public Task<int> UpdateSportContactAsync(SportContactModel model)
        {
            return Connection.ExecuteAsync("UPDATE university_contact SET " +
                                           $"sports = \"{model.Sports}\" " +
                                           $",coach = \"{model.Coach}\" " +
                                           $",email = \"{model.Email}\" " +
                                           $",assisantcoach = \"{model.AssisantCoach}\" " +
                                           $",asscoachemail = \"{model.AssCoachEmail}\" " +
                                           $"WHERE id = {model.Id}");
        }

        public Task<int> PostSportContactAsync(SportContactModel model)
        {
            return Connection.ExecuteAsync("INSERT INTO university_contact " +
                                           $"(university_id, sports, coach, email, assisantcoach, asscoachemail) " +
                                           $"VALUES ({model.University_Id}, '{model.Sports}', '{model.Coach}', '{model.Email}', '{model.AssisantCoach}', '{model.AssCoachEmail}')");
        }

        public Task<int> DeleteSportContactAsync(int id)
        {
            return Connection.ExecuteAsync("DELETE FROM university_contact " +
                                           $"WHERE id = {id}");
        }

        public Task<int> GetSportsCountAsync()
        {
            return Connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM sports_sport");
        }


        public UniversityModel[] GetUniversities(string sportName, string[] divisions, string gpa, string act,
            string sat)
        {
            float gpaPlus = 0, gpaLess = 0, actPlus = 0, actLess = 0, satPlus = 0, satLess = 0;
            if (float.TryParse(gpa, out float gpaFloat))
            {
                gpaPlus = gpaFloat + ((gpaFloat / 100) * 3);
                gpaLess = gpaFloat - ((gpaFloat / 100) * 10);
            }

            if (float.TryParse(act, out float actFloat))
            {
                actPlus = actFloat + ((actFloat / 100) * 3);
                actLess = actFloat - ((actFloat / 100) * 10);
            }

            if (float.TryParse(sat, out float satFloat))
            {
                satPlus = satFloat + ((satFloat / 100) * 3);
                satLess = satFloat - ((satFloat / 100) * 10);
            }

            sportName = sportName.Replace("'", "").ToUpper();
            var universitiesContact = CachedUniversitiesContact.Where(cu => sportName.Contains(cu.Sports));

            return CachedUniversities.Where(u =>
                universitiesContact.Any(cu => cu.University_Id == u.Id) &&
                (!divisions.Any() || divisions.Contains(u.Division)) &&
                (string.IsNullOrEmpty(gpa) || !float.TryParse(u.GPA, out float parsedGPA) ||
                 parsedGPA >= gpaLess
                 && parsedGPA <= gpaPlus)
                && (string.IsNullOrEmpty(act) || !float.TryParse(u.ACT, out float parsedACT) ||
                    parsedACT >= actLess
                    && parsedACT <= actPlus)
                && (string.IsNullOrEmpty(sat) || !float.TryParse(u.SAT, out float parsedSAT) ||
                    parsedSAT >= satLess
                    && parsedSAT <= satPlus)).ToArray();
        }

        public async Task<IEnumerable<MajorModel>> GetMajorsAsync(params int[] ids)
        {
            try
            {
                if (ids?.Any() ?? false)
                {
                    string filters = "WHERE ";

                    foreach (var id in ids)
                        filters += $"id = {id} OR ";

                    filters = filters[..^4];

                    return await Connection.QueryAsync<MajorModel>($"SELECT * FROM majors {filters}");
                }

                return await Connection.QueryAsync<MajorModel>($"SELECT * FROM majors");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<IEnumerable<MajorModel>> GetMajorsByCategoryIdAsync(string ids)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ids))
                    return await Connection.QueryAsync<MajorModel>("SELECT * FROM majors");

                string filter = "WHERE ";
                foreach (var id in ids.Split(','))
                    filter += $"category_id = {id} OR ";
                return await Connection.QueryAsync<MajorModel>($"SELECT * FROM majors {filter[..^4]}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<UniversityMajorModel> GetMajorsByUniversityId(int id)
        {
            try
            {
                return await Connection.QueryFirstAsync<UniversityMajorModel>(
                    $"SELECT * FROM university_major WHERE university_id = {id}");
            }
            catch
            {
                return default;
            }
        }

        public async Task<IEnumerable<UniversityModel>> GetUniversitiesAsync(
            int? id = null, string name = null, int? major = null, string state = null,
            string sportName = null, string[] divisions = null, string gpa = null, string act = null,
            string sat = null, (string Search, int Skip, int Take)? pagination = null, bool notCommunity = false)
        {
            if (id.HasValue)
                return await Connection.QueryAsync<UniversityModel>(
                    $"SELECT * FROM university WHERE ID = '{id.Value}'");

            string query = "SELECT * FROM university ";

            decimal gpaPlus = 0, gpaLess = 0, actPlus = 0, actLess = 0, satPlus = 0, satLess = 0;

            if (pagination.HasValue && !string.IsNullOrWhiteSpace(pagination.Value.Search))
            {
                query += $"WHERE UPPER(UNIVERSITY) LIKE UPPER(\"%{pagination.Value.Search.ToUpper()}%\") " +
                         $"OR UPPER(WEBSITE) LIKE UPPER(\"%{pagination.Value.Search.ToUpper()}%\") ";
            }

            if (notCommunity)
            {
                if (query.Contains("WHERE"))
                    query += "AND UPPER(UNIVERSITY) NOT LIKE UPPER(\"%community%\")";
                else
                    query += "WHERE UPPER(UNIVERSITY) NOT LIKE UPPER(\"%community%\")";
            }

            if (major.HasValue)
            {
                var universities =
                    await Connection.QueryAsync<int>(
                        $"SELECT university_id FROM university_major WHERE majors like '%,{major},%'");
                var universities2 = await Connection.QueryAsync<int>(
                        $"SELECT university_id FROM university_major WHERE majors like '%{major},%'");
                var universities3 = await Connection.QueryAsync<int>(
                        $"SELECT university_id FROM university_major WHERE majors like '%,{major}%'");

                universities = universities.Concat(universities2).Concat(universities3);

                if (query.Contains("WHERE"))
                {
                    if (universities is not null && universities.Any())
                        query += $"AND ID IN ({string.Join(", ", universities)}) ";
                    else
                        query += $"AND ID IN (-1) ";
                }
                else
                {
                    if (universities is not null && universities.Any())
                        query += $"WHERE ID IN ({string.Join(", ", universities)}) ";
                    else
                        query += $"WHERE ID IN (-1) ";
                }
            }

            if (!string.IsNullOrWhiteSpace(sportName))
            {
                var universities = await Connection.QueryAsync<int>(
                    $"SELECT university_id FROM university_contact WHERE SPORTS like '%{sportName?.Replace("'", "")?.ToUpper()}%'");

                if (!query.Contains("WHERE"))
                {
                    query += $"WHERE ID IN ({string.Join(", ", universities)}) ";
                }
                else
                {
                    query += $"AND ID IN ({string.Join(", ", universities)}) ";
                }
            }

            if (!string.IsNullOrWhiteSpace(state))
            {
                if (!query.Contains("WHERE"))
                {
                    query += $"WHERE UPPER(STATE) = '{state.ToUpper()}' ";
                }
                else
                {
                    query += $"AND UPPER(STATE) = '{state.ToUpper()}' ";
                }
            }

            if (divisions != null && divisions.Any())
            {
                if (!query.Contains("WHERE"))
                {
                    query += $"WHERE division IN ({string.Join(", ", divisions.Select(d => $"'{d}'"))}) ";
                }
                else
                {
                    query += $"AND division IN ({string.Join(", ", divisions.Select(d => $"'{d}'"))}) ";
                }
            }

            if (decimal.TryParse(gpa, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out decimal gpaResult))
            {
                gpaPlus = gpaResult + ((gpaResult / 100) * 4);
                gpaLess = gpaResult - ((gpaResult / 100) * 10);
            }

            if (decimal.TryParse(act, out decimal actResult))
            {
                actPlus = actResult + ((actResult / 100) * 10);
                actLess = actResult - ((actResult / 100) * 20);
            }

            if (decimal.TryParse(sat, out decimal satResult))
            {
                satPlus = satResult + ((satResult / 100) * 10);
                satLess = satResult - ((satResult / 100) * 20);
            }

            if (!string.IsNullOrWhiteSpace(gpa))
            {
                if (!query.Contains("WHERE"))
                {
                    query +=
                        $"WHERE CAST(GPA AS DECIMAL(10, 2)) >= {gpaLess.ToString("0.00", CultureInfo.GetCultureInfo("en-US"))} AND CAST(GPA AS DECIMAL(10, 2)) <= {gpaPlus.ToString("0.00", CultureInfo.GetCultureInfo("en-US"))} ";
                }
                else
                {
                    query +=
                        $"AND CAST(GPA AS DECIMAL(10, 2)) >= {gpaLess.ToString("0.00", CultureInfo.GetCultureInfo("en-US"))} AND CAST(GPA AS DECIMAL(10, 2)) <= {gpaPlus.ToString("0.00", CultureInfo.GetCultureInfo("en-US"))} ";
                }
            }

            if (!string.IsNullOrWhiteSpace(act))
            {
                if (!query.Contains("WHERE"))
                {
                    query +=
                        $"WHERE CAST(ACT AS UNSIGNED) >= {Math.Round(actLess, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} AND CAST(ACT AS UNSIGNED) <= {Math.Round(actPlus, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} ";
                }
                else
                {
                    query +=
                        $"AND CAST(ACT AS UNSIGNED) >= {Math.Round(actLess, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} AND CAST(ACT AS UNSIGNED) <= {Math.Round(actPlus, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} ";
                }
            }

            if (!string.IsNullOrWhiteSpace(sat))
            {
                if (!query.Contains("WHERE"))
                {
                    query +=
                        $"WHERE CAST(SAT AS UNSIGNED) >= {Math.Round(satLess, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} AND CAST(SAT AS UNSIGNED) <= {Math.Round(satPlus, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} ";
                }
                else
                {
                    query +=
                        $"AND CAST(SAT AS UNSIGNED) >= {Math.Round(satLess, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} AND CAST(SAT AS UNSIGNED) <= {Math.Round(satPlus, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} ";
                }
            }

            if (pagination.HasValue)
                return await Connection.QueryAsync<UniversityModel>(query +
                                                                    $"LIMIT {pagination.Value.Skip},{pagination.Value.Take}");

            return await Connection.QueryAsync<UniversityModel>(query);
        }

        public async Task<IEnumerable<UniversityContactModel>> GetSportUniversitiesCount()
        {
            var universities =
                await Connection.QueryAsync<UniversityContactModel>(
                    $"SELECT * FROM university_contact where sports not like \"%WOMEN%\"");
            return universities;
        }

        public async Task<int> GetUniversitiesCountAsync(string name = null, int? major = null, string state = null,
            string sportName = null,
            string[] divisions = null, string gpa = null, string act = null, string sat = null,
            bool notCommunity = false)
        {
            string query = "SELECT COUNT(*) FROM university ";

            decimal gpaPlus = 0, gpaLess = 0, actPlus = 0, actLess = 0, satPlus = 0, satLess = 0;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query += $"WHERE UPPER(UNIVERSITY) LIKE UPPER(\"%{name.ToUpper()}%\") " +
                         $"OR UPPER(WEBSITE) LIKE UPPER(\"%{name.ToUpper()}%\") ";
            }

            if (notCommunity)
            {
                if (query.Contains("WHERE"))
                    query += "AND UPPER(UNIVERSITY) NOT LIKE UPPER(\"%community%\")";
                else
                    query += "WHERE UPPER(UNIVERSITY) NOT LIKE UPPER(\"%community%\")";
            }

            if (major.HasValue)
            {
                var universities =
                    await Connection.QueryAsync<int>(
                        $"SELECT university_id FROM university_major WHERE majors like '%{major}%'");

                if (universities is not null && universities.Any())
                    query += $"WHERE ID IN ({string.Join(", ", universities)}) ";
                else
                    query += $"WHERE ID IN (-1) ";
            }

            if (!string.IsNullOrWhiteSpace(sportName))
            {
                var universities = await Connection.QueryAsync<int>(
                    $"SELECT university_id FROM university_contact WHERE SPORTS like '%{sportName?.Replace("'", "")?.ToUpper()}%'");

                if (!query.Contains("WHERE"))
                {
                    query += $"WHERE ID IN ({string.Join(", ", universities)}) ";
                }
                else
                {
                    query += $"AND ID IN ({string.Join(", ", universities)}) ";
                }
            }

            if (!string.IsNullOrWhiteSpace(state))
            {
                if (!query.Contains("WHERE"))
                {
                    query += $"WHERE UPPER(STATE) LIKE UPPER(\"%{state}%\") ";
                }
                else
                {
                    query += $"AND UPPER(STATE) LIKE UPPER(\"%{state}%\") ";
                }
            }

            if (divisions != null && divisions.Any())
            {
                if (!query.Contains("WHERE"))
                {
                    query += $"WHERE division IN ({string.Join(", ", divisions.Select(d => $"'{d}'"))}) ";
                }
                else
                {
                    query += $"AND division IN ({string.Join(", ", divisions.Select(d => $"'{d}'"))}) ";
                }
            }

            if (decimal.TryParse(gpa, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out decimal gpaResult))
            {
                gpaPlus = gpaResult + ((gpaResult / 100) * 4);
                gpaLess = gpaResult - ((gpaResult / 100) * 10);
            }

            if (decimal.TryParse(act, out decimal actResult))
            {
                actPlus = actResult + ((actResult / 100) * 10);
                actLess = actResult - ((actResult / 100) * 20);
            }

            if (decimal.TryParse(sat, out decimal satResult))
            {
                satPlus = satResult + ((satResult / 100) * 10);
                satLess = satResult - ((satResult / 100) * 20);
            }

            if (!string.IsNullOrWhiteSpace(gpa))
            {
                if (!query.Contains("WHERE"))
                {
                    query +=
                        $"WHERE CAST(GPA AS DECIMAL(10, 2)) >= {gpaLess.ToString("0.00", CultureInfo.GetCultureInfo("en-US"))} AND CAST(GPA AS DECIMAL(10, 2)) <= {gpaPlus.ToString("0.00", CultureInfo.GetCultureInfo("en-US"))} ";
                }
                else
                {
                    query +=
                        $"AND CAST(GPA AS DECIMAL(10, 2)) >= {gpaLess.ToString("0.00", CultureInfo.GetCultureInfo("en-US"))} AND CAST(GPA AS DECIMAL(10, 2)) <= {gpaPlus.ToString("0.00", CultureInfo.GetCultureInfo("en-US"))} ";
                }
            }

            if (!string.IsNullOrWhiteSpace(act))
            {
                if (!query.Contains("WHERE"))
                {
                    query +=
                        $"WHERE CAST(ACT AS UNSIGNED) >= {Math.Round(actLess, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} AND CAST(ACT AS UNSIGNED) <= {Math.Round(actPlus, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} ";
                }
                else
                {
                    query +=
                        $"AND CAST(ACT AS UNSIGNED) >= {Math.Round(actLess, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} AND CAST(ACT AS UNSIGNED) <= {Math.Round(actPlus, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} ";
                }
            }

            if (!string.IsNullOrWhiteSpace(sat))
            {
                if (!query.Contains("WHERE"))
                {
                    query +=
                        $"WHERE CAST(SAT AS UNSIGNED) >= {Math.Round(satLess, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} AND CAST(SAT AS UNSIGNED) <= {Math.Round(satPlus, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} ";
                }
                else
                {
                    query +=
                        $"AND CAST(SAT AS UNSIGNED) >= {Math.Round(satLess, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} AND CAST(SAT AS UNSIGNED) <= {Math.Round(satPlus, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.GetCultureInfo("en-US"))} ";
                }
            }

            return await Connection.QuerySingleAsync<int>(query);
        }

        public async Task<IEnumerable<UniversityModel>> GetFavoriteUniversitiesByAthleteId(int athleteId)
        {
            var ids = await Connection.QueryAsync<int>(
                $"SELECT university_id FROM athlete_favorite_universities WHERE athlete_id = {athleteId}");

            return await Connection.QueryAsync<UniversityModel>(
                $"SELECT * FROM university WHERE ID IN ({string.Join(",", ids)})");//LIMIT {page},{take}
        }

        public async Task<int> GetCountFavoriteUniversitiesByAthleteId(int athleteId)
        {
            return await Connection.QueryFirstAsync<int>(
                $"SELECT COUNT(*) FROM athlete_favorite_universities WHERE athlete_id = {athleteId}");
        }

        public async Task AddFavoriteUniversity(int athleteId, int universityId)
        {
            await Connection.ExecuteAsync(
                $"INSERT INTO athlete_favorite_universities (athlete_id,university_id) VALUES ({athleteId},{universityId})");
        }

        public async Task RemoveFavoriteUniversity(int athleteId, int universityId)
        {
            await Connection.ExecuteAsync(
                $"DELETE FROM athlete_favorite_universities WHERE athlete_id = {athleteId} AND university_id = {universityId}");
        }

        public async Task<int> GetUniversitiesCountAsync(string search = null, int? major = null)
        {
            string query = "SELECT COUNT(*) FROM university ";
            if (search is null)
            {
                if (major.HasValue)
                {
                    var universities =
                        await Connection.QueryAsync<UniversityMajorModel>(
                            $"SELECT university_id FROM university_major WHERE majors like '%{major}%'");

                    query += $"WHERE ID IN ({string.Join(", ", universities.Select(u => u.University_Id))}) ";
                }

                return await Connection.QueryFirstAsync<int>(query);
            }
            else
            {
                query +=
                    $"WHERE UNIVERSITY LIKE '%{search}%' " +
                    $"OR WEBSITE LIKE '%{search}%' " +
                    $"OR STATE LIKE '%{search}%' ";

                if (major.HasValue)
                {
                    var universities =
                        await Connection.QueryAsync<UniversityMajorModel>(
                            $"SELECT university_id FROM university_major WHERE majors like '%{major}%'");

                    query += $"AND ID IN ({string.Join(", ", universities.Select(u => u.University_Id))}) ";
                }

                return await Connection.QueryFirstAsync<int>(query);
            }
        }

        public Task<int> UpdateUniversityAsync(UniversityModel model)
        {
            return Connection.ExecuteAsync("UPDATE university SET " +
                                           $"university = \"{model.University}\" " +
                                           $",address = \"{model.Address}\" " +
                                           $",website = \"{model.Website}\" " +
                                           $",ATHLETICWEBSITE = \"{model.AthleticWebSite}\" " +
                                           $",division = \"{model.Division}\" " +
                                           $",tuisionin = \"{model.Tuisionin}\" " +
                                           $",tuitionout = \"{model.Tuitionout}\" " +
                                           $",gpa = \"{model.GPA}\" " +
                                           $",sat = \"{model.SAT}\" " +
                                           $",act = \"{model.ACT}\" " +
                                           $",cost_of_attendance = \"{model.Cost_Of_Attendance}\" " +
                                           $",cost_of_attendance_out = \"{model.Cost_Of_Attendance_Out}\" " +
                                           $",type = \"{model.Type}\" " +
                                           $",state = \"{model.State}\" " +
                                           $"WHERE id = {model.Id}");
        }

        public async Task<CoachModel> GetCoachByEmailOrDefaultAsync(string email)
        {
            try
            {
                return await Connection.QueryFirstAsync<CoachModel>($"SELECT * FROM coach WHERE email = '{email}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<CoachModel> GetCoachByIdAsync(int id)
        {
            try
            {
                return await Connection.QueryFirstAsync<CoachModel>($"SELECT * FROM coach WHERE id = {id}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<CoachModel> GetCoachByUniversityContactAsync(int universityContactId)
        {
            try
            {
                return await Connection.QueryFirstAsync<CoachModel>(
                    $"SELECT * FROM coach WHERE universitycontact_id = {universityContactId}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<UserModel> GetUserByEmailOrDefaultAsync(string email)
        {
            try
            {
                return await Connection.QueryFirstAsync<UserModel>($"SELECT * FROM users WHERE email = '{email}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<UserModel> GetUserByUserIdOrDefaultAsync(int userId)
        {
            try
            {
                return await Connection.QueryFirstAsync<UserModel>($"SELECT * FROM users WHERE user_id = '{userId}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public Task<int> GetAthleteSportIdAsync(int userId)
        {
            return Connection.QueryFirstAsync<int>(
                $"SELECT sports_id FROM athlete_stats_value WHERE user_id = {userId}");
        }

        public Task<string> GetSportNameAsync(int sportId)
        {
            return Connection.QueryFirstAsync<string>($"SELECT sport_name FROM sports_sport WHERE id = {sportId}");
        }

        public async Task<UniversityModel[]> GetUniversitiesForBlastEmail(string sportName, string[] divisions,
            string gpa,
            string act, string sat, string state)
        {
            var result = await GetUniversitiesAsync(sportName: sportName, divisions: divisions,
                gpa: gpa, act: act, sat: sat, state: state);

            return result.ToArray();
        }

        public async Task<UniversityModel[]> GetUniversitiesQualify(string sportName, string gpa, string? search = null, int? page = null, int? take = null)
        {
            var result = await GetUniversitiesAsync(sportName: sportName, gpa: gpa, pagination: (search, page ?? 0, take ?? int.MaxValue));
            return result.ToArray();
        }

        public async Task<AthleteModel> GetAthleteByEmailOrDefaultAsync(string email)
        {
            try
            {
                return await Connection.QueryFirstAsync<AthleteModel>(
                    $"SELECT * FROM athletes_profile WHERE email = '{email}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<IEnumerable<AthleteModel>> GetAthletesAsync()
        {
            try
            {
                return await Connection.QueryAsync<AthleteModel>($"SELECT sports.sport_name, athletes.* FROM athletes_profile as athletes INNER JOIN athlete_stats_value as stats ON athletes.user_id = stats.user_id INNER JOIN sports_sport as sports ON stats.sports_id = sports.id group by athletes.id order by id desc");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<int> GetAthletesCountAsync()
        {
            try
            {
                return await Connection.QueryFirstAsync<int>($"SELECT COUNT(*) FROM athletes_profile");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        //INSERT INTO referrals (athlete_id, assistant_id) VALUES ({assistantId}, {athleteId})
        public async Task<IEnumerable<AthleteModel>> GetAthletesByReferralAsync(int referralId)
        {
            try
            {
                var athletes =
                    await Connection.QueryAsync<int>(
                        $"SELECT athlete_id FROM referrals where assistant_id = {referralId}");
                if (athletes is not null && athletes.Any())
                    return await Connection.QueryAsync<AthleteModel>(
                        $"SELECT * FROM athletes_profile WHERE id IN ({string.Join(",", athletes)})");

                return default;
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task DisableAthleteTut(int id)
        {
            try
            {
                await Connection.ExecuteAsync($"UPDATE athletes_profile SET show_tut = 0 WHERE id = {id}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }
        }

        public async Task EnableAthleteTut(int id)
        {
            try
            {
                await Connection.ExecuteAsync($"UPDATE athletes_profile SET show_tut = 1 WHERE id = {id}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }
        }

        public async Task<int> GetAthletesCountByReferralAsync(int referralId)
        {
            try
            {
                var athletes =
                    await Connection.QueryAsync<int>(
                        $"SELECT athlete_id FROM referrals where assistant_id = {referralId}");
                if (athletes is not null && athletes.Any())
                    return await Connection.QueryFirstAsync<int>(
                        $"SELECT COUNT(*) FROM athletes_profile WHERE id IN ({string.Join(",", athletes)})");

                return default;
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<bool> CheckUserIsAdminAsync(int userId)
        {
            try
            {
                var count = await Connection.QueryFirstAsync<int>(
                    $"SELECT COUNT(*) FROM admin_users WHERE user_id = {userId}");

                return count > 0;
            }
            catch
            {
            }

            return false;
        }

        public Task<IEnumerable<StateModel>> GetStatesAsync(int? countryId = null, string search = null)
        {
            if (search is null)
            {
                if (countryId is null)
                {
                    return Connection.QueryAsync<StateModel>("SELECT * FROM states");
                }
                else
                {
                    return Connection.QueryAsync<StateModel>("SELECT * FROM states " +
                                                             $"WHERE country_id = {countryId.Value}");
                }
            }
            else
            {
                if (countryId is null)
                {
                    return Connection.QueryAsync<StateModel>("SELECT * FROM states " +
                                                             $"WHERE name LIKE '%{search}%'");
                }
                else
                {
                    return Connection.QueryAsync<StateModel>("SELECT * FROM states " +
                                                             $"WHERE name LIKE '%{search}%' " +
                                                             $"WHERE country_id = {countryId.Value}");
                }
            }
        }

        public Task<IEnumerable<DivisionModel>> GetDivisionsAsync()
        {
            return Connection.QueryAsync<DivisionModel>("SELECT * FROM divisions");
        }

        public Task<PositionModel> GetAthletePositionAsync(int positionId)
        {
            return Connection.QueryFirstAsync<PositionModel>($"SELECT positions FROM athlete_position " +
                                                             $"WHERE id = {positionId}");
        }

        public async Task<AthleteSettingsModel> GetAthleteSettingsAsync(int athleteId)
        {
            try
            {
                return await Connection.QueryFirstAsync<AthleteSettingsModel>(
                    $"SELECT divisions, average FROM athletes_settings " +
                    $"WHERE athlete_id = {athleteId}");
            }
            catch
            {
                return default;
            }
        }

        public async Task InsertOrUpdateAthleteSettings(int athleteId, string divisions, string average)
        {
            var count = await Connection.QueryFirstAsync<int>(
                $"SELECT COUNT(*) FROM athletes_settings WHERE athlete_id = {athleteId}");
            if (count > 0)
            {
                await Connection.ExecuteAsync($"UPDATE athletes_settings SET athlete_id = {athleteId}, " +
                                              $"divisions = '{(divisions is null ? string.Empty : divisions)}', average = '{average}' WHERE athlete_id = {athleteId}");
            }
            else
            {
                await Connection.ExecuteAsync("INSERT INTO athletes_settings (athlete_id, divisions, average) VALUES " +
                                              $"({athleteId}, '{(divisions is null ? string.Empty : divisions)}', '{average}')");
            }
        }

        public async Task<int> UpdateAthlete(AthleteDTO athlete)
        {
            try
            {
                return await Connection.ExecuteAsync("UPDATE athletes_profile SET " +
                                                     $"first_name = \"{athlete.FirstName}\", " +
                                                     $"last_name = \"{athlete.LastName}\", " +
                                                     $"weight = \"{athlete.Weight}\", " +
                                                     $"height = \"{athlete.Height.ToString("0.#0", CultureInfo.InvariantCulture)}\", " +
                                                     $"state = \"{athlete.State}\", " +
                                                     $"address = \"{athlete.Address.Replace("\"", "\\\"")}\", " +
                                                     $"birth = \"{athlete.Birth}\", " +
                                                     $"hs_grad_year = \"{athlete.GraduationYear}\", " +
                                                     $"gpa = \"{athlete.GPA}\", " +
                                                     $"sat = \"{athlete.SAT}\", " +
                                                     $"act = \"{athlete.ACT}\", " +
                                                     $"ncaa = \"{athlete.NCAA}\", " +
                                                     $"naia = \"{athlete.NAIA}\", " +
                                                     $"cel_phone = \"{athlete.CellPhone}\", " +
                                                     $"g_first_name = \"{athlete.GuardianName}\", " +
                                                     $"g_phone = \"{athlete.GuardianPhone}\", " +
                                                     $"g_mail = \"{athlete.GuardianEmail}\", " +
                                                     $"personal_statement = \"{athlete.PersonalStatement.Replace("\"", "\\\"")}\", " +
                                                     $"coaches_evaluation = \"{athlete.CoachesEvaluation}\", " +
                                                     $"high_school = \"{athlete.HighSchool}\" " +
                                                     $"WHERE id = {athlete.Id} ");
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public async Task<int> UpdateAthleteImageProfile(int athleteId, string fileName)
        {
            try
            {
                return await Connection.ExecuteAsync("UPDATE athletes_profile SET " +
                                                     $"image_profile = \"{fileName}\" " +
                                                     $"WHERE user_id = {athleteId} ");
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateAthleteIsEnabled(int athleteId, bool enabled)
        {
            try
            {
                string sql = "UPDATE athletes_profile SET " +
                                                     $"enabled = {(enabled ? "True" : "False")} " +
                                                     $"WHERE id = {athleteId} ";
                return await Connection.ExecuteAsync(sql);
            }
            catch
            {
                return -1;
            }
        }

        public async Task<AthleteModel> GetAthleteByIdAsync(int id)
        {
            try
            {
                return await Connection.QueryFirstAsync<AthleteModel>(
                    $"SELECT * FROM athletes_profile WHERE id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<IEnumerable<AthleteStatsModel>> GetAthleteStats()
        {
            try
            {
                return await Connection.QueryAsync<AthleteStatsModel>($"SELECT * FROM athlete_stats_value");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<AthleteStatsModel> GetAthleteStatsByUserId(int id)
        {
            try
            {
                return await Connection.QueryFirstAsync<AthleteStatsModel>(
                    $"SELECT * FROM athlete_stats_value WHERE user_id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<int> SetAthleteStatsAsync(int userId, AthleteStatsModel stats)
        {
            try
            {
                var alreadyHas = await GetAthleteStatsByUserId(userId) is not null;
                if (alreadyHas)
                {
                    return await Connection.ExecuteAsync("UPDATE athlete_stats_value SET " +
                                                         $"stats = \"{stats.Stats}\"" +
                                                         $"WHERE user_id = '{userId}'");
                }

                return await Connection.ExecuteAsync($"INSERT INTO athlete_stats_value " +
                                                     $"(user_id, stats) VALUES " +
                                                     $"({userId}, \"{stats.Stats}\")");
            }
            catch
            {
                return default;
            }
        }

        public async Task<AthleteStatsStructModel> GetAthleteStatsStructureBySportId(int id, int position)
        {
            try
            {
                var structs = await Connection.QueryAsync<AthleteStatsStructModel>(
                    $"SELECT * FROM athlete_game_position_stats WHERE sports_id = {id}");

                return structs.FirstOrDefault(s => s.Position_Id.Split(',').Contains(position.ToString()));

            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<AthleticHistoryModel> GetAthleticHistorysByUserId(int id)
        {
            try
            {
                return await Connection.QueryFirstAsync<AthleticHistoryModel>(
                    $"SELECT * FROM atheletic_history WHERE user_id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<IEnumerable<AthleteVideosModel>> GetAthleteVideosByUserId(int id)
        {
            try
            {
                return await Connection.QueryAsync<AthleteVideosModel>(
                    $"SELECT * FROM athletes_video WHERE user_id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<IEnumerable<AthleteCoachInfoModel>> GetAthleteCoachesByUserId(int id)
        {
            try
            {
                return await Connection.QueryAsync<AthleteCoachInfoModel>(
                    $"SELECT * FROM coach_information WHERE user_id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<IEnumerable<AthleteHonorsModel>> GetAthleteHonorsByUserId(int id)
        {
            try
            {
                return await Connection.QueryAsync<AthleteHonorsModel>(
                    $"SELECT * FROM athlete_honor_roll WHERE user_id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<int> SetAthleteHonorAsync(int userId, AthleteHonorsModel honors)
        {
            try
            {
                return await Connection.QuerySingleAsync<int>("INSERT INTO athlete_honor_roll " +
                                                              "(user_id, honor_roll, year) VALUES " +
                                                              $"({userId}, \"{honors.Honor_Roll}\", \"{honors.Year}\"); " +
                                                              "SELECT LAST_INSERT_ID();");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> RemoveAthleteHonorAsync(int honorId)
        {
            try
            {
                return await Connection.ExecuteAsync($"DELETE FROM athlete_honor_roll WHERE id = {honorId}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<IEnumerable<AthleteHighSchoolModel>> GetAthleteHighSchoolByUserId(int id)
        {
            try
            {
                return await Connection.QueryAsync<AthleteHighSchoolModel>(
                    $"SELECT * FROM atheletic_history WHERE user_id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<int> SetAthleteHighSchoolAsync(int userId, AthleteHighSchoolModel honors)
        {
            try
            {
                return await Connection.QuerySingleAsync<int>("INSERT INTO atheletic_history " +
                                                              "(user_id, highschool, descriptions, year) VALUES " +
                                                              $"({userId}, \"{honors.HighSchool}\", \"{honors.Descriptions}\", {honors.Year}); " +
                                                              $"SELECT LAST_INSERT_ID();");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> RemoveAthleteHighSchoolAsync(int higshSchoolId)
        {
            try
            {
                return await Connection.ExecuteAsync($"DELETE FROM atheletic_history WHERE id = {higshSchoolId}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> SetAthleteClubAsync(int userId, AthleteClubsModel club)
        {
            try
            {
                return await Connection.QuerySingleAsync<int>("INSERT INTO club_season_history " +
                                                              "(user_id, club_name, competition_year) VALUES " +
                                                              $"({userId}, \"{club.Club_Name}\", {club.Competition_Year}); " +
                                                              "SELECT LAST_INSERT_ID();");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> RemoveAthleteClubAsync(int clubId)
        {
            try
            {
                return await Connection.ExecuteAsync($"DELETE FROM club_season_history WHERE id = {clubId}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> SetAthleteCoachAsync(int userId, AthleteCoachInfoModel coach)
        {
            try
            {
                return await Connection.QuerySingleAsync<int>("INSERT INTO coach_information " +
                                                              "(user_id, name, phone, email, club_name, type) VALUES " +
                                                              $"({userId}, \"{coach.Name}\", \"{coach.Phone}\", \"{coach.Email}\", \"{coach.Club_Name}\", \"{coach.Type}\"); " +
                                                              $"SELECT LAST_INSERT_ID();");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> RemoveAthleteCoachAsync(int coachInfoId)
        {
            try
            {
                return await Connection.ExecuteAsync($"DELETE FROM coach_information WHERE id = {coachInfoId}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> SetAthleteVideoAsync(int userId, AthleteVideosModel video)
        {
            try
            {
                return await Connection.QuerySingleAsync<int>("INSERT INTO athletes_video " +
                                                              "(user_id, video1, title) VALUES " +
                                                              $"({userId}, \"{video.Video1}\", \"{video.Title}\"); " +
                                                              "SELECT LAST_INSERT_ID();");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> RemoveAthleteVideoAsync(int videoId)
        {
            try
            {
                return await Connection.ExecuteAsync($"DELETE FROM athletes_video WHERE id = {videoId}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> SetAthleteFile(FileModel file)
        {
            try
            {
                if (file.Description.ToLower().Contains("profile"))
                {
                    await Connection.ExecuteAsync(
                        $"UPDATE athletes_profile SET image_profile = \"{file.Description}\" WHERE id = {file.RelateId}");
                }

                return await Connection.ExecuteAsync("INSERT INTO athlete_file " +
                                                     "(file_name, description, athlete_id) VALUES " +
                                                     $"(\"{file.File_Name}\", \"{file.Description}\", {file.RelateId})");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> SetUniversityLogo(FileModel file)
        {
            try
            {
                return await Connection.ExecuteAsync(
                    $"UPDATE university SET image = \"{file.Description}\" WHERE id = {file.RelateId} ");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<string> GetUniversityLogo(FileModel file)
        {
            try
            {
                return await Connection.QueryFirstAsync<string>(
                    $"SELECT image FROM university WHERE id = {file.RelateId} ");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> UpdateAthleteFile(FileModel file)
        {
            try
            {
                return await Connection.ExecuteAsync("UPDATE athlete_file SET " +
                                     $"file_name = \"{file.File_Name}\"," +
                                     $"description = \"{file.Description}\" " +
                                     $"WHERE athlete_id = {file.RelateId} AND description = \"{file.Description}\"");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> UpdateUniversityImage(FileModel file)
        {
            try
            {
                return await Connection.ExecuteAsync(
                    $"UPDATE universities SET image = \"{file.Description}\" WHERE id = {file.RelateId}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<IEnumerable<FileModel>> GetAthleteFile(int athleteId)
        {
            try
            {
                return await Connection.QueryAsync<FileModel>(
                    $"SELECT * FROM athlete_file WHERE athlete_id = {athleteId}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> SetAthleteAwardAsync(int userId, AthleteAwardsModel award)
        {
            try
            {
                return await Connection.QuerySingleAsync<int>("INSERT INTO athletes_awards " +
                                                              "(user_id, awards, year) VALUES " +
                                                              $"({userId}, \"{award.Awards}\", \"{award.Year}\"); " +
                                                              "SELECT LAST_INSERT_ID();");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> RemoveAthleteAwardAsync(int awardId)
        {
            try
            {
                return await Connection.ExecuteAsync($"DELETE FROM athletes_awards WHERE id = {awardId}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<IEnumerable<AthleteAwardsModel>> GetAthleteAwardsByUserId(int id)
        {
            try
            {
                return await Connection.QueryAsync<AthleteAwardsModel>(
                    $"SELECT * FROM athletes_awards WHERE user_id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<IEnumerable<AthleteStoriesModel>> GetAthleteStoriesByUserId(int id)
        {
            try
            {
                return await Connection.QueryAsync<AthleteStoriesModel>(
                    $"SELECT * FROM atheletic_history WHERE user_id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }


        public async Task<IEnumerable<AthleteClubsModel>> GetAthleteClubsByUserId(int id)
        {
            try
            {
                return await Connection.QueryAsync<AthleteClubsModel>(
                    $"SELECT * FROM club_season_history WHERE user_id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }

            return default;
        }

        public async Task<EntityBilling> GetBillingByEntityIdAsync(int id)
        {
            try
            {
                return await Connection.QuerySingleAsync<EntityBilling>(
                    $"SELECT * FROM entity_billing WHERE entity_id = '{id}' ORDER BY ID DESC LIMIT 1");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public Task<int> UpdateEntityBillingAsync(int entityId, string plan, EntityBilling model)
        {
            return Connection.ExecuteAsync("UPDATE entity_billing SET " +
                                           $"card_number = \"{model.Card_Number}\" " +
                                           $",exp_date = \"{model.Exp_Date}\" " +
                                           $",card_code = \"{model.Card_Code}\" " +
                                           $",zip = \"{model.Zip}\" " +
                                           $",plan = \"{plan}\" " +
                                           $"WHERE entity_id = {entityId}");
        }

        public Task<int> UpdateBillingPlanAsync(int entityId, string plan)
        {
            return Connection.ExecuteAsync("UPDATE entity_billing SET " +
                                           $"plan = \"{plan}\" " +
                                           $"WHERE entity_id = {entityId}");
        }

        public Task<int> AddBillingPlanAsync(int entityId, EntityBilling model)
        {
            return Connection.ExecuteAsync("INSERT INTO entity_billing " +
                                           "(entity_id, card_number, exp_date, card_code, zip, plan, date) VALUES " +
                                           $"({entityId}, '{model.Card_Number}', '{model.Exp_Date}', '{model.Card_Code}', " +
                                           $"'{model.Zip}', '{model.Plan}', \"{DateTime.Now:yyyy-MM-dd}\")");
        }

        public async Task<IEnumerable<EntityPayments>> GetPaymentsByEntityIdAsync(int id)
        {
            try
            {
                return await Connection.QueryAsync<EntityPayments>(
                    $"SELECT * FROM entity_payments WHERE entity_id = '{id}'");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task<int> PostPaymentByEntityId(EntityPayments entityPayment)
        {
            try
            {
                if (entityPayment.Promotion_Code is not null)
                    return await Connection.ExecuteAsync("INSERT INTO entity_payments " +
                                                         "(entity_id, plan, amount, promotion_code, date) VALUES " +
                                                         $"({entityPayment.Entity_Id}, '{entityPayment.Plan}', \"{entityPayment.Amount}\", '{entityPayment.Promotion_Code}', \"{entityPayment.Date:yyyy-MM-dd}\")");

                return await Connection.ExecuteAsync("INSERT INTO entity_payments " +
                                                     "(entity_id, plan, amount, date) VALUES " +
                                                     $"({entityPayment.Entity_Id}, '{entityPayment.Plan}', \"{entityPayment.Amount}\", \"{entityPayment.Date:yyyy-MM-dd}\")");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
                return default;
            }
        }

        public async Task RemoveUniversityAsync(int id)
        {
            try
            {
                await Connection.ExecuteAsync($"DELETE FROM university WHERE id = {id}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }
        }

        public async Task RemoveSportAsync(int id)
        {
            try
            {
                await Connection.ExecuteAsync($"DELETE FROM sports_sport WHERE id = {id}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }
        }

        public async Task RemoveAthleteAsync(int id)
        {
            try
            {
                var athlete = await GetAthleteByIdAsync(id);
                if (athlete is not null)
                    await Connection.ExecuteAsync($"DELETE FROM users WHERE id = {athlete.User_Id}");

                await Connection.ExecuteAsync($"DELETE FROM users_password WHERE user_id = {athlete.User_Id}");
                await Connection.ExecuteAsync($"DELETE FROM athlete_stats_value WHERE user_id = {athlete.User_Id}");

                await Connection.ExecuteAsync($"DELETE FROM athletes_profile WHERE id = {id}");
                await Connection.ExecuteAsync($"DELETE FROM coach_fav_athletes WHERE athlete_id = {id}");

                // Por seguridad dejar los registros de correo
                // await Connection.ExecuteAsync($"DELETE FROM emails WHERE sender_id = {id} OR receiver_id = {id}");
            }
#pragma warning disable CS0168 // La variable 'e' se ha declarado pero nunca se usa
            catch (Exception e)
#pragma warning restore CS0168 // La variable 'e' se ha declarado pero nunca se usa
            {
            }
        }

        public async Task<IEnumerable<int>> GetFavoriteAthletesByCoachId(int coachId)
        {
            return await Connection.QueryAsync<int>(
                $"SELECT athlete_id FROM coach_fav_athletes WHERE coach_id = {coachId}");
        }

        public async Task<int> GetCountFavoriteAthletesByCoachId(int coachId)
        {
            return await Connection.QueryFirstAsync<int>(
                $"SELECT COUNT(*) FROM coach_fav_athletes WHERE coach_id = {coachId}");
        }

        public async Task AddFavoriteAthlete(int coachId, int athleteId)
        {
            await Connection.ExecuteAsync(
                $"INSERT INTO coach_fav_athletes (coach_id,athlete_id) VALUES ({coachId},{athleteId})");
        }

        public async Task RemoveFavoriteAthlete(int coachId, int athleteId)
        {
            await Connection.ExecuteAsync(
                $"DELETE FROM coach_fav_athletes WHERE coach_id = {coachId} AND athlete_id = {athleteId}");
        }

        public async Task<IEnumerable<EmailModel>> GetAllEmails()
        {
            return await Connection.QueryAsync<EmailModel>("SELECT * FROM emails");
        }

        public async Task<IEnumerable<EmailModel>> GetConversationAsync(int sender, int receiver)
        {
            return await Connection.QueryAsync<EmailModel>(
                $"SELECT * FROM emails WHERE sender_id = {sender} AND receiver_id = {receiver} OR receiver_id = {sender} AND sender_id = {receiver}");
        }

        public async Task<EmailModel> GetEmailById(int id)
        {
            return await Connection.QueryFirstAsync<EmailModel>($"SELECT * FROM emails WHERE id = {id}");
        }

        public async Task<IEnumerable<EmailModel>> GetEmailsBySenderId(int senderId)
        {
            return await Connection.QueryAsync<EmailModel>($"SELECT * FROM emails WHERE sender_id = {senderId}");
        }

        public async Task<IEnumerable<EmailModel>> GetEmailsByReceiverId(int receiverId)
        {
            return await Connection.QueryAsync<EmailModel>($"SELECT * FROM emails WHERE receiver_id = {receiverId} AND is_deleted = false");
        }

        public async Task<int> GetCountEmailsBySenderId(int senderId)
        {
            return await Connection.QueryFirstAsync<int>($"SELECT COUNT(*) FROM emails WHERE sender_id = {senderId}");
        }

        public async Task<int> GetCountEmailsByReceiverId(int receiverId)
        {
            return await Connection.QueryFirstAsync<int>(
                $"SELECT COUNT(*) FROM emails WHERE receiver_id = {receiverId}");
        }

        public Task<int> AddEmail(EmailModel model)
        {
            return Connection.QuerySingleAsync<int>(
                $"INSERT INTO emails (sender_id,sender_name,sender_image,receiver_id,receiver_name,receiver_image,subject,content,date,university_id,university_name,receiver_type,replied_to,timestamp,readed) VALUES " +
                $"({model.Sender_Id},\"{model.Sender_Name}\",\"{model.Sender_Image}\",{model.Receiver_Id},\"{model.Receiver_Name}\",\"{model.Receiver_Image}\"," +
                $"\"{model.Subject}\",\"{model.Content}\",\"{model.Date:yyyy-MM-dd}\", {model.University_Id}, \"{model.University_Name}\",{model.Receiver_Type},{model.Replied_To},{model.TimeStamp},{model.Readed}); " +
                $"SELECT LAST_INSERT_ID();");
        }

        public async Task UpdateEmailsAsRead(int whoReaded, int whoIsNotified)
        {
            await Connection.ExecuteAsync(
                $"UPDATE emails SET readed = 1 WHERE sender_id = {whoReaded} AND receiver_id = {whoIsNotified}");
        }

        public async Task RemoveEmail(int id)
        {
            await Connection.ExecuteAsync($"UPDATE emails SET is_deleted = true WHERE id = {id}");
        }

        public async Task<IEnumerable<CoachModel>> GetCoachsBySportIdAsync(int sportId)
        {
            return await Connection.QueryAsync<CoachModel>(
                $"SELECT coach.*, university.image FROM coach inner join university on coach.university_id = university.ID WHERE sports_id = {sportId}");
        }

        public async Task<CoachModel> GetCoachsByUniversityIdAsync(int sportId, int universityId)
        {
            return await Connection.QuerySingleAsync<CoachModel>(
                $"SELECT coach.*, university.image FROM coach inner join university on coach.university_id = university.ID WHERE sports_id = {sportId} AND university_id = {universityId}");
        }

        public async Task<IEnumerable<CoachViewModel>> GetCoachViewsAsync(int athleteId)
        {
            return await Connection.QueryAsync<CoachViewModel>(
                $"SELECT * FROM coach_views WHERE athlete_id = {athleteId}");
        }

        public async Task<int> GetCoachViewsCountAsync(int athleteId)
        {
            return await Connection.QueryFirstAsync<int>(
                $"SELECT COUNT(*) FROM coach_views WHERE athlete_id = {athleteId}");
        }

        public async Task<int> AddCoachViewsAsync(CoachViewModel model)
        {
            return await Connection.ExecuteAsync("INSERT INTO coach_views " +
                                                 "(coach_id, athlete_id, university_name, coach_name, university_id, university_image)" +
                                                 " VALUES " +
                                                 $"({model.Coach_Id}, {model.Athlete_Id}, \"{model.University_Name}\", \"{model.Coach_Name}\", " +
                                                 $"\"{model.University_Id}\", \"{model.University_Image}\")");
        }

        public EmailContact[] GetEmailsByUniversities(string sportName, UniversityModel[] universities)
        {
            sportName = sportName.Replace("'", "").ToUpper();
            var universitiesContact = CachedUniversitiesContact.Where(cu =>
                string.IsNullOrWhiteSpace(cu.Sports) || sportName.Contains(cu.Sports));

            var ids = universities.Select(u => u.Id);
            var filteredContacts = universitiesContact.Where(cu => ids.Contains(cu.University_Id));

            List<EmailContact> emails = new();
            foreach (var contact in filteredContacts)
            {
                if (!string.IsNullOrWhiteSpace(contact.Email))
                    emails.Add(new EmailContact
                    { UniversityId = contact.University_Id, Name = contact.Coach, Email = contact.Email });

                if (!string.IsNullOrWhiteSpace(contact.AssCoachEmail))
                    emails.Add(new EmailContact
                    {
                        UniversityId = contact.University_Id,
                        Name = contact.AssisantCoach,
                        Email = contact.AssCoachEmail
                    });
            }

            return emails.ToArray();
        }
        public async Task<SocialMediaModel> GetSocialMediaByUserIdAsync(int userId)
        {
            return await Connection.QueryFirstAsync<SocialMediaModel>($"SELECT * FROM social_media WHERE user_id = {userId}");
        }
        public Task<int> AddSocialMediaAsync(SocialMediaModel model)
        {
            return Connection.QuerySingleAsync<int>("INSERT INTO social_media " +
                                                  "(user_id, instagram, facebook, twitter) " +
                                                  $"VALUES " +
                                                  $"({model.User_id}, '{model.Instagram}', '{model.Facebook}', '{model.Twitter}'); " +
                                                  $"SELECT LAST_INSERT_ID();");
        }
        public Task<int> UpdateSocialMediaAsync(SocialMediaModel model)
        {
            return Connection.ExecuteAsync("UPDATE social_media SET " +
                                           $"id = \"{model.Id}\" " +
                                           $",user_id = \"{model.User_id}\" " +
                                           $",instagram = \"{model.Instagram}\" " +
                                           $",facebook = \"{model.Facebook}\" " +
                                           $",twitter = \"{model.Twitter}\" " +
                                           $"WHERE id = {model.Id}");
        }

    }
}