using System.Threading.Tasks;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Email;
using iSportsRecruiting.Shared.Models;

namespace iSportsRecruiting.Shared.Services
{
    public interface IDatabaseContext
    {
        MySqlConnection Connection { get; set; }

        Task<int> UpdateAthleteIsEnabled(int athleteId, bool enabled);
        Task EnableAthleteTut(int id);
        Task DisableAthleteTut(int id);
        Task<int> AddAthleteService(int athleteId, int serviceId);
        Task<int> CheckIfServiceExists(int athleteId, int serviceId);
        Task<int> MarkServiceAsUsed(int athleteId, int serviceId);
        
        Task<IEnumerable<UniversityContactModel>> GetSportUniversitiesCount();

        Task<int> AddCoachAsync(SportContactDTO coach);
        Task<SportContactDTO> UpdateCoachAsync(SportContactDTO coach);
        Task<int> DeleteCoachAsync(int id);

        Task<string> GetUniversityLogo(FileModel file);
        Task<int> SetUniversityLogo(FileModel file);
        Task<int> DeleteNotificationAsync(int senderId, int receiverId);
        Task<int> AddNotificationAsync(Notification notification);
        Task<IEnumerable<Notification>> GetNotificationByUserIdAsync(int userId);

        Task<string> AddConfirmationLink(ConfirmEmail confirm);
        Task<ConfirmEmail> GetConfirmationLink(string token);
        Task<ConfirmEmail> DeleteConfirmationLink(int id);

        Task<SportContactModel> GetSportsContactByUniversityAndSportAsync(int universityId, string sport);
        Task<int> RemoveAthleteClubAsync(int clubId);
        Task<int> RemoveAthleteAwardAsync(int awardId);
        Task<int> RemoveAthleteHonorAsync(int honorId);
        Task<int> RemoveAthleteVideoAsync(int videoId);
        Task<int> RemoveAthleteCoachAsync(int coachInfoId);
        Task<int> RemoveAthleteHighSchoolAsync(int higshSchoolId);

        Task<IEnumerable<AthleteHighSchoolModel>> GetAthleteHighSchoolByUserId(int id);
        Task<IEnumerable<FileModel>> GetAthleteFile(int athleteId);
        Task<int> UpdateAthleteFile(FileModel file);
        Task<int> SetAthleteFile(FileModel file);
        Task<int> SetAthleteVideoAsync(int userId, AthleteVideosModel video);
        Task<int> SetAthleteCoachAsync(int userId, AthleteCoachInfoModel coach);
        Task<int> SetAthleteClubAsync(int userId, AthleteClubsModel club);
        Task<int> SetAthleteHighSchoolAsync(int userId, AthleteHighSchoolModel honors);

        Task<int> SetAthleteAwardAsync(int userId, AthleteAwardsModel award);
        Task<int> SetAthleteHonorAsync(int userId, AthleteHonorsModel honors);

        Task<int> SetAthleteStatsAsync(int userId, AthleteStatsModel stats);

        Task<int> GetAthletesCountAsync();
        Task<int> GetAthletesCountByReferralAsync(int referralId);
        Task<AthleteModel> AddAthleteAsync(AthleteRegisterDto register);
        Task<CoachDTO> RegisterCoachAsync(CoachDTO register);
        Task<int> AddBillingPlanAsync(int entityId, EntityBilling model);

        Task<UniversityMajorModel> GetMajorsByUniversityId(int id);

        Task RemoveUniversityAsync(int id);
        Task RemoveSportAsync(int id);
        Task RemoveAthleteAsync(int id);

        Task<UserSecurity> GetUserSecurityByUserIdAsync(int userId, int userType);
        Task<UserSecurity> GetUserSecurityByPasswordAsync(int userId, int userType, string cipher);
        Task<int> AddUserSecurityAsync(UserSecurity security);

        Task<EntityBilling> GetBillingByEntityIdAsync(int id);
        Task<IEnumerable<EntityPayments>> GetPaymentsByEntityIdAsync(int id);
        Task<int> PostPaymentByEntityId(EntityPayments entityPayment);
        Task<int> UpdateBillingPlanAsync(int entityId, string plan);
        Task<int> UpdateEntityBillingAsync(int entityId, string plan, EntityBilling model);

        Task<AssistantModel> GetAssistantByEmailAsync(string email);
        Task<IEnumerable<AssistantModel>> GetAssistantsAsync(int? id = null);
        Task<int> GetAssistantsCountAsync();
        Task<int> AddAssistantAsync(AssistantModel model);
        Task<int> UpdateAssistantAsync(AssistantModel model);
        Task<int> DeleteAssistantAsync(int id, string email);

        Task<int> GetPromotionsCountAsync();
        Task<PromotionModel> GetPromotionByCodeAsync(string code);
        Task<IEnumerable<PromotionModel>> GetPromotionsAsync(int? id = null);
        Task<int> AddPromotionAsync(PromotionModel model);
        Task<int> UpdatePromotionAsync(PromotionModel model);
        Task<int> DeletePromotionAsync(int id);

        Task<int> GetTasksCountAsync(int userId);
        Task<IEnumerable<TaskModel>> GetTasksAsync(int userId);
        Task<int> AddTaskAsync(TaskModel model);
        Task<int> UpdateTaskAsync(TaskModel model);
        Task<int> DeleteTaskAsync(int id);

        Task<int> GetLeadsCountAsync(int? id = null);
        Task<bool> CheckIfLeadExists(string email);
        Task<IEnumerable<LeadModel>> GetLeadsAsync(int? id = null);
        Task<int> AddLeadAsync(LeadModel model);
        Task<int> UpdateLeadAsync(LeadModel model);
        Task<int> DeleteLeadAsync(int id);

        //Task<IEnumerable<GoalModel>> GetGoalsAsync(int? id = null);
        //Task<int> AddGoalAsync(GoalModel model);
        //Task<int> UpdateGoalAsync(GoalModel model);
        //Task<int> DeleteGoalAsync(int id);

        Task<int> UpdateAthlete(AthleteDTO athlete);
        Task<int> UpdateAthleteImageProfile(int athleteId, string fileName);
        Task<IEnumerable<AthleteModel>> GetAthletesAsync();
        Task<IEnumerable<AthleteModel>> GetAthletesByReferralAsync(int referralId);
        Task<int> GetCountFavoriteUniversitiesByAthleteId(int athleteId);
        Task RemoveFavoriteUniversity(int athleteId, int universityId);
        Task AddFavoriteUniversity(int athleteId, int universityId);

        Task<IEnumerable<UniversityModel>> GetFavoriteUniversitiesByAthleteId(int athleteId);

        Task GetCache();
        Task<IEnumerable<MajorModel>> GetMajorsAsync(params int[] ids);
        Task<IEnumerable<MajorModel>> GetMajorsByCategoryIdAsync(string ids);
        Task<int> UpdateUniversityAsync(UniversityModel model);
        Task<int> GetUniversitiesCountAsync(string search = null, int? major = null);

        Task<IEnumerable<UniversityModel>> GetUniversitiesAsync(
            int? id = null, string name = null,
            int? major = null, string state = null,
            string sportName = null,
            string[] divisions = null, string gpa = null, string act = null, string sat = null,
            (string Search, int Skip, int Take)? pagination = null,
            bool notCommunity = false);
        
        Task<int> GetUniversitiesCountAsync(string name = null, int? major = null, string state = null, string sportName = null, 
            string[] divisions = null, string gpa = null, string act = null, string sat = null, bool notCommunity = false);
        Task<UniversityModel[]> GetUniversitiesQualify(string sportName, string gpa, string? search = null, int? page = null, int? take = null);

        Task<int> GetSportsCountAsync();
        Task<int> DeleteSportContactAsync(int id);
        Task<int> PostSportContactAsync(SportContactModel model);
        Task<int> UpdateSportContactAsync(SportContactModel model);
        Task<IEnumerable<SportModel>> GetSportsAsync(int? id = null, (string Search, int Skip, int Take)? pagination = null);
        Task<IEnumerable<SportContactModel>> GetSportsContactByUniversityAsync(int? universityId = null, int? sportContactId = null);
        Task<IEnumerable<PositionModel>> GetPositionsBySport(int id);

        Task<AthleteModel> GetAthleteByEmailOrDefaultAsync(string email);
        Task<AthleteModel> GetAthleteByIdAsync(int id);
        Task<IEnumerable<AthleteStatsModel>> GetAthleteStats();
        Task<AthleteStatsModel> GetAthleteStatsByUserId(int id);
        Task<AthleticHistoryModel> GetAthleticHistorysByUserId(int id);
        Task<IEnumerable<AthleteVideosModel>> GetAthleteVideosByUserId(int id);
        Task<AthleteStatsStructModel> GetAthleteStatsStructureBySportId(int id, int position);
        Task<IEnumerable<AthleteCoachInfoModel>> GetAthleteCoachesByUserId(int id);
        Task<IEnumerable<AthleteHonorsModel>> GetAthleteHonorsByUserId(int id);
        Task<IEnumerable<AthleteAwardsModel>> GetAthleteAwardsByUserId(int id);
        Task<IEnumerable<AthleteStoriesModel>> GetAthleteStoriesByUserId(int id);
        Task<IEnumerable<AthleteClubsModel>> GetAthleteClubsByUserId(int id);


        Task<int> GetUsersCountAsync();
        Task<bool> CheckUserIsAdminAsync(int userId);
        Task<UserModel> GetUserByUserIdOrDefaultAsync(int userId);
        Task<UserModel> GetUserByEmailOrDefaultAsync(string email);
        Task<IEnumerable<UserModel>> GetUsersAsync(int? id = null, (string Search, int Skip, int Take)? pagination = null);

        Task<CoachModel> GetCoachByIdAsync(int id);
        Task<CoachModel> GetCoachByEmailOrDefaultAsync(string email);
        Task<CoachModel> GetCoachByUniversityContactAsync(int universityContactId);

        Task<IEnumerable<StateModel>> GetStatesAsync(int? countryId = null, string search = null);

        Task<IEnumerable<DivisionModel>> GetDivisionsAsync();

        Task<PositionModel> GetAthletePositionAsync(int positionId);
        Task<AthleteSettingsModel> GetAthleteSettingsAsync(int athleteId);
        Task<UniversityModel[]> GetUniversitiesForBlastEmail(string sportName, string[] divisions, string gpa, string act, string sat, string state);
        Task<int> GetAthleteSportIdAsync(int userId);
        Task<string> GetSportNameAsync(int sportId);
        Task InsertOrUpdateAthleteSettings(int athleteId, string divisions, string average);

        Task<IEnumerable<int>> GetFavoriteAthletesByCoachId(int coachId);
        Task<int> GetCountFavoriteAthletesByCoachId(int coachId);
        Task AddFavoriteAthlete(int coachId, int athleteId);
        Task RemoveFavoriteAthlete(int coachId, int athleteId);

        Task UpdateEmailsAsRead(int whoReaded, int whoIsNotified);
        Task<EmailModel> GetEmailById(int id);
        Task<IEnumerable<EmailModel>> GetAllEmails();
        Task<IEnumerable<EmailModel>> GetConversationAsync(int sender, int receiver);
        Task<IEnumerable<EmailModel>> GetEmailsBySenderId(int senderId);
        Task<IEnumerable<EmailModel>> GetEmailsByReceiverId(int receiverId);
        Task<int> GetCountEmailsBySenderId(int senderId);
        Task<int> GetCountEmailsByReceiverId(int receiverId);
        Task<int> AddEmail(EmailModel model);
        Task RemoveEmail(int id);

        Task<IEnumerable<CoachModel>> GetCoachsBySportIdAsync(int sportId);
        Task<CoachModel> GetCoachsByUniversityIdAsync(int sportId, int universityId);
        Task<IEnumerable<CoachViewModel>> GetCoachViewsAsync(int athleteId);
        Task<int> GetCoachViewsCountAsync(int athleteId);
        Task<int> AddCoachViewsAsync(CoachViewModel model);

        EmailContact[] GetEmailsByUniversities(string sportName, UniversityModel[] universities);
        Task<SocialMediaModel> GetSocialMediaByUserIdAsync(int userId);
        Task<int> AddSocialMediaAsync(SocialMediaModel model);
        Task<int> UpdateSocialMediaAsync(SocialMediaModel model);
        Task<int> AddScheduledNotificationAsync(ScheduledNotificationDTO notification);
        Task<IEnumerable<ScheduledNotification>> GetScheduledNotificationAsync();
        Task<LeadModel> GetLeadByIdAsync(int id);
        Task<int> DeleteScheduledNotificationAsync(int id);
    }
}
