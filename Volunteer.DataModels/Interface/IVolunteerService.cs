using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Volunteer.VolunteerMessage;

namespace Jtext103.Volunteer.DataModels.Interface
{
    public interface IVolunteerService
    {
        #region register
        void RegisterMap<T>(System.Collections.Generic.IEnumerable<string> propertyList);
        void RegisterMap<T>();
        #endregion register

        #region insert&save&update
        void InsertOne(Entity entity);
        void SaveOne(Entity entity);
        void UpdateOne(Guid id, Dictionary<string, object> updateObject);
        #endregion insert&save&update

        #region find
        Entity FindOneById(Guid id);
        User FindUser(string email);
        User FindUser(string email, string password);
        User FindUser(Guid userId);
        Activity FindActivity(Guid activityId);
        IEnumerable<Activity> FindAllActivities(string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindAllNotDraftActivities(User user, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        List<Activity> FindDraftActivitiesByOrganizerOrOrganization(User user, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        List<Activity> FindActivitesVolunteerSignedIn(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        List<Activity> FindActivitesByOrganizationId(Guid organizationId, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        List<Activity> FindActivatedActivitesByOrganizerId(Guid organizerId, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindAllAboutToStartActivities(User user, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindAllRunningActivities(User user, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindAllFinishedActivities(User user, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllUsers(string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllOrganizations(string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllOrganizers(string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllVolunteers(string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllAppliedOrganizerByOrganization(User organization, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllOrganizerByOrganization(User organization, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllAppliedOrganizationByOrganizer(User organizer, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllJoinedOrganizationByOrganizer(User organizer, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllToJoinOrganizationByOrganizer(User organizer, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindAllVolunteerSignedInActivities(User volunteer, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        //IEnumerable<Activity> FindAllVolunteerNotSignOutActivites(User volunteer, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindAllVolunteerCompletedActivities(User volunteer, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllVolunteerInActivity(Activity activity, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllToCheckInVolunteersInActivity(Activity activity, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllToCheckOutVolunteersInActivity(Activity activity, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllCompletedVolunteersInActivity(Activity activity, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllNotCompletedVolunteersInActivity(Activity activity, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<User> FindAllVolunteerWhoFavoriteTheActivity(Guid activityId, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindAllActivitiesWhichVolunteerFavorite(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindAboutToStartActivitiesWhichVolunteerFavorite(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindRunningActivitiesWhichVolunteerFavorite(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindFinishedActivitiesWhichVolunteerFavorite(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        IEnumerable<Activity> FindAllActivitiesWhichVolunteerViewed(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        object FindMessageSender(Message message);
        #endregion find

        #region other
        bool AddOrganizerToOrganization(User organizer, User organization);
        bool OrganizationKickOrganizerOut(User organizer, User organization, string comment);
        bool OrganizerQuitOrganization(User organizer, User organization);
        bool OrganizerApplyToJoinOrganization(User organizer, User organization, string comment);
        bool OrganizationAcceptOrganizerApplication(User organizer, User organization, string comment);
        bool OrganizationRefuseOrganizerApplication(User organizer, User organization, string comment);
        //bool VolunteerSignInActivity(User volunteer, Activity activity);
        //bool VolunteerSignOutActivity(User volunteer, Activity activity);
        void VolunteerViewActivity(User volunteer, Activity activity);
        void ClearVolunteerViewActivityRecord(User volunteer);
        bool CheckIfVolunteerViewActivity(User volunteer, Guid activityId);
        bool CheckIfVolunteerSignInActivity(User volunteer, Guid activityId);
        bool CheckIfVolunteerCompleteActivity(Guid volunteerId, Activity activity);
        bool CheckIfVolunteerFavoriteActivity(User volunteer, Guid activityId);
        bool CheckIfOrganizerCanManageActivity(Activity activity, User organizer);
        void VolunteerFavoriteActivity(User volunteer, Activity activity);
        void VolunteerUnFavoriteActivity(User volunteer, Activity activity);
        bool ActivityValidateBadgeLimit(Activity activity, Guid userId);
        void TimeToCheckStatus(Activity activity);
        void AddProfoile(User user, Role role);
        bool CheckUserPassword(string email, string password);
        object MyPointRankOfAllVolunteer(Guid id);
        bool DeleteActivity(Guid id);
        /// <summary>
        /// 排序以及分页(sortByKey为""或者null不排序；pageIndex和pageSize有一个为0，不分页)
        /// </summary>
        /// <param name="source">总信息</param>
        /// <param name="sortByKey">排序的依据</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">每页元素个数</param>
        /// <returns></returns>
        List<Activity> SortAndPaging(List<Activity> source, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        List<User> SortAndPaging(List<User> source, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        List<object> SortAndPaging(List<object> source, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        List<T> SortAndPaging<T>(List<T> source, string sortByKey, bool isAscending, int pageIndex, int pageSize);
        #endregion other
    }
}
