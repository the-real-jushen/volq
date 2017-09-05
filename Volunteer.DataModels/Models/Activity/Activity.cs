using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.VolunteerEvent;
using Jtext103.BlogSystem.Interface;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class Activity : Entity, IRateable
    {
        //发起人Id
        public Guid OrganizerId { get; set; }
        //组织Id
        public Guid OrganizationId { get; set; }
        //组织名称
        public string OrganizationName { get; set; }
        //活动名称
        public string Name { get; set; }
        //活动摘要
        public string Abstract { get; set; }
        //活动激活时间
        public DateTime ActivateTime { get; set; }
        //开放注册时间
        public DateTime OpenSignInTime { get; set; }
        //关闭注册时间
        public DateTime CloseSignInTime { get; set; }
        //活动开始时间
        public DateTime StartTime { get; set; }
        //活动结束时间
        public DateTime FinishTime { get; set; }
        //活动完成后应得点数
        public double Point { get; set; }
        //人数上限（当为0时无上限）
        public int MostVolunteers { get; set; }
        //人数下限
        public int LeastVolunteers { get; set; }
        //活动地点描述
        public string Location { get; set; }
        //活动地点坐标
        public string Coordinate { get; set; }
        //具体过程
        public string Procedure { get; set; }
        //封面（默认为将第一个图片裁剪后的图片地址）
        public string Cover { get; set; }
        //精彩瞬间（多个图片地址）
        public List<string> Photos { get; set; }
        //视频（多个视频地址）
        public List<string> Videos { get; set; }
        //活动的标签
        public List<string> Tags { get; set; }
        //参加活动志愿者拥有badge的限制
        public ActivityBadgeLimit BadgeLimit { get; set; }
        //属性（六芒星）的总点数上限
        public double HexagramPropertyTotalPointLimit { get; set; }
        //属性（六芒星）
        public HexagramProperty HexagramProperty { get; set; }
        //人员要求
        public string Requirement { get; set; }
        //志愿者查看次数
        public int VolunteerViewedTime { get; set; }
        //志愿者收藏次数
        public int VolunteerFavoritedTime { get; set; }
        //判断该activity是否激活
        private bool isActive;
        //该活动所得平均分
        private double rating;
        //已经打过分的人数
        private int ratedNumber;
        public int RatedNumber
        {
            get
            {
                return ratedNumber;
            }
        }
        //volunteer的状态列表
        public List<VolunteerParticipateInActivityRecord> VolunteerStatus { get; set; }

        private ActivityStatus statusInDB;
        //获得该活动的状态, 并重置statusInDB的值
        public ActivityStatus Status
        {
            get
            {
                if (statusInDB == ActivityStatus.Finished || statusInDB == ActivityStatus.Abort)
                {
                    return statusInDB;
                }
                ActivityStatus result;
                if (isActive == false)
                {
                    result = ActivityStatus.Draft;
                }
                else
                {
                    int volunteerNmuber = HasSignedInVolunteerNumber;
                    //还没到sign in Time
                    if (OpenSignInTime.ToLocalTime() >= DateTime.Now)
                    {
                        result = ActivityStatus.Active;
                    }
                    //已过finish time，finished状态
                    else if (FinishTime.ToLocalTime() <= DateTime.Now)
                    {
                        if (volunteerNmuber >= LeastVolunteers && (volunteerNmuber <= MostVolunteers || MostVolunteers == 0))
                        {
                            result = ActivityStatus.Finished;
                            //自动checkout所有该活动的volunteer
                            CheckOutAll();
                        }
                        else
                        {
                            result = ActivityStatus.Abort;
                        }
                    }
                    //open sign in < now <close sign in
                    else if (OpenSignInTime.ToLocalTime() <= DateTime.Now && CloseSignInTime.ToLocalTime() >= DateTime.Now)
                    {
                        //没到start time, sign in状态
                        if (DateTime.Now < StartTime.ToLocalTime())
                        {
                            if (volunteerNmuber < MostVolunteers || MostVolunteers == 0)
                            {
                                result = ActivityStatus.SignIn;
                            }
                            else if (volunteerNmuber == MostVolunteers && MostVolunteers != 0)
                            {
                                result = ActivityStatus.MaxVolunteer;
                            }
                            else
                            {
                                result = ActivityStatus.Abort;
                            }
                        }
                        //到了start time, SignInAndCheckIn状态, 如果人数满了无法sign in则为check in状态
                        else
                        {
                            if (volunteerNmuber < MostVolunteers || MostVolunteers == 0)
                            {
                                result = ActivityStatus.RunningSignInAndCheckIn;
                            }
                            else if (volunteerNmuber == MostVolunteers && MostVolunteers != 0)
                            {
                                result = ActivityStatus.RunningCheckIn;
                            }
                            else
                            {
                                result = ActivityStatus.Abort;
                            }
                        }
                    }
                    //已过close sign in time，还没到startTime，ready状态
                    else if (CloseSignInTime.ToLocalTime() <= DateTime.Now && StartTime.ToLocalTime() >= DateTime.Now)
                    {
                        if (volunteerNmuber >= LeastVolunteers && (volunteerNmuber <= MostVolunteers || MostVolunteers == 0))
                        {
                            result = ActivityStatus.Ready;
                        }
                        else
                        {
                            result = ActivityStatus.Abort;
                        }
                    }
                    //start time <= now <= finish time
                    else if (StartTime.ToLocalTime() <= DateTime.Now && FinishTime.ToLocalTime() >= DateTime.Now)
                    {
                        //已过close sign in time，sign in状态
                        if (DateTime.Now > CloseSignInTime.ToLocalTime())
                        {
                            if (volunteerNmuber >= LeastVolunteers && (volunteerNmuber <= MostVolunteers || MostVolunteers == 0))
                            {
                                result = ActivityStatus.RunningCheckIn;
                            }
                            else
                            {
                                result = ActivityStatus.Abort;
                            }
                        }
                        //没到close sign in time，signInAndCheckIn状态, 如果人数满了无法sign in则为check in状态
                        else
                        {
                            if (volunteerNmuber < MostVolunteers || MostVolunteers == 0)
                            {
                                result = ActivityStatus.RunningSignInAndCheckIn;
                            }
                            else if (volunteerNmuber == MostVolunteers && MostVolunteers != 0)
                            {
                                result = ActivityStatus.RunningCheckIn;
                            }
                            else
                            {
                                result = ActivityStatus.Abort;
                            }
                        }
                    }
                    else
                    {
                        result = ActivityStatus.Abort;
                    }
                }
                if (statusInDB == result)
                {
                    return result;
                }
                else
                {
                    //先修改当前实例的statusInDB，防止下次get status时还在用该实例，与数据库不同步出现bug
                    statusInDB = result;
                    //更新数据库
                    Dictionary<string, object> queryObject = new Dictionary<string, object>();
                    Dictionary<string, object> subQueryObject = new Dictionary<string, object>();
                    subQueryObject.Add("statusInDB", result);
                    queryObject.Add("$set", subQueryObject);
                    _serviceContext.UpdateOne(Id, queryObject);
                    //产生状态变化事件
                    EventService.Publish("ActivityStatusChangeEvent", result.ToString(), this.Id);
                    return result;
                }
            }
        }

        //已经signed in的volunteer数
        //如果有人sign In或者sign out，需要重新计算hasSignedInVolunteerNumber，否则不需重新计算
        private int hasSignedInVolunteerNumber;
        public int HasSignedInVolunteerNumber
        {
            get
            {
                return hasSignedInVolunteerNumber;
            }
        }

        /// <summary>
        /// 该活动的完成率(该活动未finished时完成率显示为-1)
        /// </summary>
        public double CompleteRate
        {
            get
            {
                if (this.Status == ActivityStatus.Finished)
                {
                    int completedNumber = 0;
                    //int totalNumber = 0;
                    foreach (VolunteerParticipateInActivityRecord volunteerRecord in VolunteerStatus)
                    {
                        if (volunteerRecord.VolunteerStatus == VolunteerStatusInActivity.complete)
                        {
                            completedNumber++;
                        }
                        //if (volunteerRecord.VolunteerStatus != VolunteerStatusInActivity.unsignedIn)
                        //{
                        //    totalNumber++;
                        //}
                    }
                    //if (totalNumber > 0)
                    //{
                    //    return completedNumber / totalNumber;
                    //}
                    if (HasSignedInVolunteerNumber > 0)
                    {
                        return completedNumber / HasSignedInVolunteerNumber;
                    }
                    else
                    {
                        throw new Exception("参加活动总人数必须大于0");
                    }
                }
                else
                {
                    //throw new Exception("该活动还未结束，无法查询完成率。");
                    return -1;
                }
            }
        }

        public Activity()
        {
            this.EntityType = "Activity";
            statusInDB = ActivityStatus.Draft;
            VolunteerStatus = new List<VolunteerParticipateInActivityRecord>();
            HexagramPropertyTotalPointLimit = 50;
            this.HexagramProperty = new HexagramProperty(0, 0, 0, 0, 0);
            Photos = new List<string>();
            Videos = new List<string>();
            Tags = new List<string>();
            BadgeLimit = new ActivityBadgeLimit();
            hasSignedInVolunteerNumber = 0;
            VolunteerViewedTime = 0;
            VolunteerFavoritedTime = 0;
            isActive = false;
            rating = 0;
            ratedNumber = 0;
        }
        /// <summary>
        /// 激活该activity
        /// </summary>
        /// <returns>成功return true, 失败return false</returns>
        public bool ActivateActivity()
        {
            if (isActive == false)
            {
                if (this.ValidateTime() && this.ValidateHexagram())
                {
                    //更新数据库，isActive = true
                    isActive = true;
                    Dictionary<string, object> queryObject = new Dictionary<string, object>();
                    Dictionary<string, object> subQueryObject = new Dictionary<string, object>();
                    subQueryObject.Add("isActive", true);
                    subQueryObject.Add("ActivateTime", DateTime.Now);
                    queryObject.Add("$set", subQueryObject);
                    _serviceContext.UpdateOne(Id, queryObject);

                    //根据activity的状态，新建最多4个定时器检查activity的状态
                    //先重置db中的statusInDB, 即get status
                    //再定时启动线程, 每次都get status，重置db中的statusInDB
                    _serviceContext.TimeToCheckStatus(this);
                    return true;
                }
                else return false;
            }
            else
            {
                throw new Exception("The activity has been activated!");
            }
        }
        /// <summary>
        /// 通过开放注册时间、关闭注册时间、活动开始时间、活动结束时间判断该活动是否非法
        /// 开放注册时间(小于等于)关闭注册时间
        /// 活动开始时间(小于等于)活动结束时间
        /// 开放注册时间(小于等于)活动开始时间
        /// 关闭注册时间(小于等于)活动结束时间
        /// </summary>
        /// <returns></returns>
        public bool ValidateTime()
        {
            //if (OpenSignInTime <= CloseSignInTime && CloseSignInTime <= StartTime && StartTime <= FinishTime)
            if (OpenSignInTime <= CloseSignInTime && StartTime <= FinishTime && OpenSignInTime <= StartTime && CloseSignInTime <= FinishTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查六芒星属性总和是否大于上限，判断该活动是否合法
        /// </summary>
        /// <returns></returns>
        public bool ValidateHexagram()
        {
            if (HexagramProperty.Strength + HexagramProperty.Intelligence + HexagramProperty.Endurance + HexagramProperty.Compassion + HexagramProperty.Sacrifice <= HexagramPropertyTotalPointLimit)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// volunteer sign in this activity
        /// </summary>
        /// <param name="volunteer">volunteer</param>
        /// <returns></returns>
        public bool SignIn(User volunteer)
        {
            if (this.Status == ActivityStatus.SignIn || this.Status == ActivityStatus.RunningSignInAndCheckIn)
            {
                //如果该activity已经在volunteerProfile.SigninedActivityIds中，则说明用户曾经sign in过后来取消，则不需要修改VolunteerProfile
                //如果该activity不在volunteerProfile.SigninedActivityIds中，则说明用户首次sign in，须在VolunteerProfile中添加activity.Id
                if (!((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds.Contains(this.Id))
                {
                    ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds.Add(this.Id);
                }

                //将该volunteer 加入 activity.volunteerStatus
                //如果该volunteer已经sign in后取消，则只需修改状态
                bool isSignedInAndSignedOut = false;//指示该volunteer是否已经sign in后取消
                foreach (VolunteerParticipateInActivityRecord record in this.VolunteerStatus)
                {
                    if (record.VolunteerId == volunteer.Id)
                    {
                        //当Volunteer状态为kickedOut时，无法signIn
                        if (record.VolunteerStatus == VolunteerStatusInActivity.kickedOut)
                        {
                            return false;
                        }
                        if (record.VolunteerStatus == VolunteerStatusInActivity.unsignedIn)
                        {
                            record.SignInTime = DateTime.Now;
                            record.SignedIn.IsSignedIn = true;
                            isSignedInAndSignedOut = true;
                            break;
                        }
                        else return false;
                    }
                }
                //如果该volunteer从未sign in，则需新建volunteerRecord
                if (isSignedInAndSignedOut == false)
                {
                    VolunteerParticipateInActivityRecord volunteerRecord = new VolunteerParticipateInActivityRecord(volunteer.Id, DateTime.Now);
                    this.VolunteerStatus.Add(volunteerRecord);
                }
                //重新计算已经sign in的人数
                hasSignedInVolunteerNumber = 0;
                foreach (VolunteerParticipateInActivityRecord record in VolunteerStatus)
                {
                    if (record.VolunteerStatus != VolunteerStatusInActivity.unsignedIn && record.VolunteerStatus != VolunteerStatusInActivity.kickedOut)
                    {
                        hasSignedInVolunteerNumber++;
                    }
                }
                this.Save();
                volunteer.Save();
                //产生VolunteerSignIn事件
                EventService.Publish("VolunteerSignInEvent", volunteer.Id.ToString() + "," + this.Id.ToString(), volunteer.Id);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// volunteer sign in 后取消
        /// </summary>
        /// <param name="volunteer">volunteer</param>
        /// <returns></returns>
        public bool SignOut(User volunteer)
        {
            if (this.Status == ActivityStatus.SignIn || this.Status == ActivityStatus.MaxVolunteer || this.Status == ActivityStatus.RunningSignInAndCheckIn)
            {
                //将activity id从volunteerProfile.SigninedActivityIds中移除
                if (!((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds.Remove(this.Id))
                {
                    //该activity不在volunteerProfile.SigninedActivityIds中
                    return false;
                }
                bool IsRecortInActivity = false;//标志该volunteer是否在activity.volunteerStatus中
                //改变activity.volunteerStatus中状态
                foreach (VolunteerParticipateInActivityRecord record in this.VolunteerStatus)
                {
                    if (record.VolunteerId == volunteer.Id)
                    {
                        //只有当前状态为signIn时才能signOut
                        if (record.VolunteerStatus == VolunteerStatusInActivity.signedIn)
                        {
                            record.SignedIn.IsSignedIn = false;
                            IsRecortInActivity = true;
                            break;
                        }
                    }
                }
                if (IsRecortInActivity == false)
                {
                    //该volunteer不在activity.volunteerStatus中
                    return false;
                }
                //重新计算已经sign in的人数
                hasSignedInVolunteerNumber = 0;
                foreach (VolunteerParticipateInActivityRecord record in VolunteerStatus)
                {
                    if (record.VolunteerStatus != VolunteerStatusInActivity.unsignedIn && record.VolunteerStatus != VolunteerStatusInActivity.kickedOut)
                    {
                        hasSignedInVolunteerNumber++;
                    }
                }
                this.Save();
                volunteer.Save();
                //产生VolunteerSignOut事件
                EventService.Publish("VolunteerSignOutEvent", volunteer.Id.ToString() + "," + this.Id.ToString(), volunteer.Id);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// organizer将一个signIn状态下的Volunteer踢出，被踢Volunteer无法再次sign in
        /// </summary>
        /// <param name="volunteer"></param>
        /// <returns></returns>
        public bool KickVolunteerOut(User volunteer)
        {
            if (this.Status == ActivityStatus.SignIn || this.Status == ActivityStatus.MaxVolunteer || this.Status == ActivityStatus.RunningSignInAndCheckIn)
            {
                //将activity id从volunteerProfile.SigninedActivityIds中移除
                if (!((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds.Remove(this.Id))
                {
                    //该activity不在volunteerProfile.SigninedActivityIds中
                    return false;
                }
                bool IsRecortInActivity = false;//标志该volunteer是否在activity.volunteerStatus中
                //改变activity.volunteerStatus中状态
                foreach (VolunteerParticipateInActivityRecord record in this.VolunteerStatus)
                {
                    if (record.VolunteerId == volunteer.Id)
                    {
                        //当前状态为signedIn时才能踢人
                        if (record.VolunteerStatus == VolunteerStatusInActivity.signedIn)
                        {
                            record.KickedOut.IsKickedOut = true;
                            record.KickedOut.KickedOutTime = DateTime.Now;
                            IsRecortInActivity = true;
                            break;
                        }
                    }
                }
                if (IsRecortInActivity == false)
                {
                    //该volunteer不在activity.volunteerStatus中
                    return false;
                }
                //重新计算已经sign in的人数
                hasSignedInVolunteerNumber = 0;
                foreach (VolunteerParticipateInActivityRecord record in VolunteerStatus)
                {
                    if (record.VolunteerStatus != VolunteerStatusInActivity.unsignedIn && record.VolunteerStatus != VolunteerStatusInActivity.kickedOut)
                    {
                        hasSignedInVolunteerNumber++;
                    }
                }
                this.Save();
                volunteer.Save();
                //产生KickVolunteerOutEvent事件
                EventService.Publish("KickVolunteerOutEvent", volunteer.Id.ToString() + "," + this.Id.ToString(), volunteer.Id);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// 误踢用户恢复
        /// </summary>
        /// <param name="volunteer"></param>
        /// <returns></returns>
        public bool UnKickVolunteerOut(User volunteer)
        {
            //先将activity加入VolunteerProfile
            if (!((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds.Contains(this.Id))
            {
                ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds.Add(this.Id);
            }
            //标志该volunteer是否在activity.volunteerStatus中
            bool IsRecortInActivity = false;
            foreach (VolunteerParticipateInActivityRecord record in this.VolunteerStatus)
            {
                if (record.VolunteerId == volunteer.Id)
                {
                    //重新设置kickOut为false
                    if (record.VolunteerStatus == VolunteerStatusInActivity.kickedOut)
                    {
                        record.KickedOut.IsKickedOut = false;
                        IsRecortInActivity = true;
                        break;
                    }
                    else return false;
                }
            }
            if (IsRecortInActivity == false)
            {
                //该volunteer不在activity.volunteerStatus中
                return false;
            }
            //重新计算已经sign in的人数
            hasSignedInVolunteerNumber = 0;
            foreach (VolunteerParticipateInActivityRecord record in VolunteerStatus)
            {
                if (record.VolunteerStatus != VolunteerStatusInActivity.unsignedIn && record.VolunteerStatus != VolunteerStatusInActivity.kickedOut)
                {
                    hasSignedInVolunteerNumber++;
                }
            }
            this.Save();
            volunteer.Save();
            //产生UnKickVolunteerOutEvent事件
            EventService.Publish("UnKickVolunteerOutEvent", volunteer.Id.ToString() + "," + this.Id.ToString(), volunteer.Id);
            return true;
        }

        /// <summary>
        /// check in该volunteer
        /// </summary>
        /// <param name="volunteer">volunteer</param>
        /// <returns></returns>
        public bool CheckIn(User volunteer)
        {
            if (this.Status == ActivityStatus.RunningCheckIn || this.Status == ActivityStatus.RunningSignInAndCheckIn || this.Status == ActivityStatus.Finished)
            {
                foreach (VolunteerParticipateInActivityRecord volunteerRecord in this.VolunteerStatus)
                {
                    if (volunteerRecord.VolunteerId == volunteer.Id)
                    {
                        //当前状态为signedIn时才能正常checkin
                        if (volunteerRecord.VolunteerStatus == VolunteerStatusInActivity.signedIn)
                        {
                            volunteerRecord.CheckedIn.IsCheckedIn = true;
                            volunteerRecord.CheckedIn.CheckedInTime = DateTime.Now;
                        }
                        else
                        {
                            return false;
                        }
                        this.Save();
                        //产生VolunteerCheckIn事件
                        EventService.Publish("VolunteerCheckInEvent", volunteer.Id.ToString() + "," + this.Id.ToString(), volunteer.Id);
                        return true;
                    }
                }
                //运行到这说明该volunteer不在activity的VolunteerStatus中，即该volunteer未参与该activity
                return false;
            }
            else return false;
        }

        /// <summary>
        /// check out该volunteer
        /// </summary>
        /// <param name="volunteer">volunteer</param>
        /// <param name="isComplete">是否完成该活动</param>
        /// <returns></returns>
        public bool CheckOut(User volunteer, bool isComplete)
        {
            if (this.Status == ActivityStatus.RunningCheckIn || this.Status == ActivityStatus.RunningSignInAndCheckIn || this.Status == ActivityStatus.Finished)
            {
                foreach (VolunteerParticipateInActivityRecord volunteerRecord in this.VolunteerStatus)
                {
                    if (volunteerRecord.VolunteerId == volunteer.Id)
                    {
                        //当前状态为checkedIn时,可以checkout,根据isComplete判断volunteer是否完成该活动
                        if (volunteerRecord.VolunteerStatus == VolunteerStatusInActivity.checkedIn)
                        {
                            volunteerRecord.CheckedOut.IsCheckedOut = true;
                            volunteerRecord.CheckedOut.CheckedOutTime = DateTime.Now;
                            if (isComplete)
                            {
                                //完成活动，获得点数和六芒星属性
                                volunteerRecord.CheckedOut.Status = CheckOutStatus.complete;
                                ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Point.AddPoints(this.Id, this.Point, volunteer);
                                ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).AddActivityHexagramProperty(this.HexagramProperty, this.Point);
                                volunteer.Save();
                            }
                            else
                            {
                                volunteerRecord.CheckedOut.Status = CheckOutStatus.quit;
                            }
                        }
                        //当前状态为signedIn时，也能checkout，说明没该志愿者没check in，即volunteer没参加活动
                        else if (volunteerRecord.VolunteerStatus == VolunteerStatusInActivity.signedIn)
                        {
                            volunteerRecord.CheckedOut.IsCheckedOut = true;
                            volunteerRecord.CheckedOut.CheckedOutTime = DateTime.Now;
                            //volunteerRecord.CheckedOut.Status = CheckOutStatus.quit;
                        }
                        else
                        {
                            return false;
                        }
                        this.Save();
                        //产生VolunteerCheckOut事件
                        EventService.Publish("VolunteerCheckOutEvent", volunteer.Id.ToString() + "," + this.Id.ToString() + "," + volunteerRecord.VolunteerStatus.ToString(), volunteer.Id);
                        return true;
                    }
                }
                //运行到这说明该volunteer不在activity的VolunteerStatus中，即该volunteer未参与该activity
                return false;
            }
            else return false;
        }

        /// <summary>
        /// 在activity中直接checkout某个volunteer
        /// volunteer如果处于signIn状态，先check in该volunteer，紧接着check out他
        /// volunteer如果处于checkIn状态，直接check out
        /// </summary>
        /// <param name="volunteer">volunteer</param>
        /// <param name="isComplete">是否完成该活动</param>
        /// <returns></returns>
        public bool CheckOutDirectly(User volunteer, bool isComplete)
        {
            if (this.Status == ActivityStatus.RunningCheckIn || this.Status == ActivityStatus.RunningSignInAndCheckIn || this.Status == ActivityStatus.Finished)
            {
                foreach (VolunteerParticipateInActivityRecord volunteerRecord in this.VolunteerStatus)
                {
                    if (volunteerRecord.VolunteerId == volunteer.Id)
                    {
                        //当volunteer状态为signedIn时，必须先check in，再check out
                        if (volunteerRecord.VolunteerStatus == VolunteerStatusInActivity.signedIn)
                        {
                            if (this.CheckIn(volunteer))
                            {
                                if (this.CheckOut(volunteer, isComplete))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        //当volunteer状态为checkIn时，直接check out
                        else if (volunteerRecord.VolunteerStatus == VolunteerStatusInActivity.checkedIn)
                        {
                            if (this.CheckOut(volunteer, isComplete))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else return false;
                    }
                }
                //运行到这说明该volunteer不在activity的VolunteerStatus中，即该volunteer未参与该activity
                return false;
            }
            else return false;
        }

        /// <summary>
        /// checkout所有该活动的volunteer（除了已经checkout的）
        /// </summary>
        /// <returns></returns>
        public void CheckOutAll()
        {
            foreach (VolunteerParticipateInActivityRecord volunteerRecord in this.VolunteerStatus)
            {
                //只checkout没有checkout过的
                if (volunteerRecord.CheckedOut.IsCheckedOut == false)
                {
                    User volunteer = _serviceContext.FindUser(volunteerRecord.VolunteerId);
                    //直接将IsCheckedOut设为true
                    volunteerRecord.CheckedOut.IsCheckedOut = true;
                    volunteerRecord.CheckedOut.CheckedOutTime = DateTime.Now;
                    //当前状态为checkedIn时,可以checkout,完成该活动,获得点数和六芒星属性
                    if (volunteerRecord.VolunteerStatus == VolunteerStatusInActivity.checkedIn)
                    {
                        volunteerRecord.CheckedOut.Status = CheckOutStatus.complete;
                        ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Point.AddPoints(this.Id, this.Point, volunteer);
                        ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).AddActivityHexagramProperty(this.HexagramProperty, this.Point);
                        volunteer.Save();
                    }
                    //产生VolunteerCheckOut事件
                    EventService.Publish("VolunteerCheckOutEvent", volunteer.Id.ToString() + "," + this.Id.ToString() + "," + volunteerRecord.VolunteerStatus.ToString(), volunteer.Id);
                }
            }
            this.Save();
        }

        public static void RegisterMe(IVolunteerService volunteerService)
        {
            volunteerService.RegisterMap<Activity>(new List<string>() { "statusInDB", "hasSignedInVolunteerNumber", "isActive", "rating", "ratedNumber" });
            HexagramProperty.RegisterMe(volunteerService);
        }


        #region IRateabele
        /// <summary>
        /// 该活动所得平均分
        /// </summary>
        public double Rating
        {
            get
            {
                return rating;
            }
        }
        /// <summary>
        /// 给活动打分,并重新计算该活动平均得分
        /// 只有已经完成该活动的志愿者能够打分
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="rating"></param>
        public void Rate(Guid userId, double rate)
        {
            foreach(VolunteerParticipateInActivityRecord record in VolunteerStatus)
            {
                if (record.VolunteerId == userId)
                {
                    if (record.VolunteerStatus == VolunteerStatusInActivity.complete)
                    {
                        if (record.HasRated == false)
                        {
                            record.Rate = rate;
                            record.HasRated = true;
                            //并重新计算该活动平均得分
                            rating = (rating * ratedNumber + rate) / (ratedNumber + 1);
                            ratedNumber++;
                            this.Save();
                            return;
                        }
                        else
                        {
                            throw new Exception("已经打过分了！");
                        }
                    }
                    else
                    {
                        throw new Exception("只有已经完成该活动的志愿者能够打分！");
                    }
                }
                else
                {
                    continue;
                }
            }
            throw new Exception("只有已经完成该活动的志愿者能够打分！");
        }

        /// <summary>
        /// 获得指定用户的评分
        /// 如果没找到该用户，返回0
        /// </summary>
        public double GetMyRate(Guid userId)
        {
            foreach (VolunteerParticipateInActivityRecord record in VolunteerStatus)
            {
                if (record.VolunteerId == userId)
                {
                    return record.Rate;
                }
            }
            return 0;
        }
        #endregion IRateabele
    }
}
