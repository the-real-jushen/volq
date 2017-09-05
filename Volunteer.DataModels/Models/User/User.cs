using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.VolunteerEvent;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class ProfileContainer
    {
        List<Profile> profileList;
        public ProfileContainer(List<Profile> userProfileList)
        {
            profileList = userProfileList;
        }
        public Profile this[string profileName]
        {
            get
            {
                foreach (Profile profile in profileList)
                {
                    if (profile.ProfileName == profileName)
                        return profile;
                }
                throw new Exception("ProfileName错误！");
            }
        }
        public List<Profile> AllUserProfile
        {
            get
            {
                return profileList;
            }
        }
    }


    public class User : Entity
    {
        //map this
        private List<Profile> _userProfiles=new List<Profile>();
        public string Name { get; set; }
        public Sex Sex { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        //是否锁定
        public bool IsLockedOut { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneNumberVerified { get; set; }
        //注册时间
        public DateTime SignUpTime { get; set; }
        public List<Role> UserRole { get; set; }
        public ProfileContainer UserProfiles
        {
            get
            {
                return new ProfileContainer(_userProfiles);
            }
        }
        //返回一个匿名用户
        public static User Anonymous
        {
            get
            {
                var user = new User();
                user.UserRole.Clear();
                user.UserRole.Add(Role.Anonymous);
                return user;
            }
        }
        public User()
        {
            this.EntityType = "User";
            this.Sex = Sex.Male;
            IsLockedOut = false;
            IsEmailVerified = false;
            IsPhoneNumberVerified = false;
            SignUpTime = DateTime.Now;
            UserRole = new List<Role>();
        }
        public void AddProfile(Profile profile)
        {
            _userProfiles.Add(profile);
            this.Save();
        }
        /// <summary>
        /// 验证邮箱
        /// 当此人为被邀请注册的账号时，为邀请人加20分，且邀请人的邀请人数+1
        /// </summary>
        public void VerifyEmail()
        {
            if (IsEmailVerified == false)
            {
                IsEmailVerified = true;
                foreach (string key in this.ExtraInformation.Keys)
                {
                    if (key == "invited-inviteVolunteerId")
                    {
                        User inviteVolunteer = _serviceContext.FindUser((Guid)ExtraInformation[key]);
                        //邀请人数+1
                        if (inviteVolunteer.ExtraInformation.ContainsKey("invite-inviteNumber"))
                        {
                            inviteVolunteer.ExtraInformation["invite-inviteNumber"] = (int)inviteVolunteer.ExtraInformation["invite-inviteNumber"] + 1;
                        }
                        else
                        {
                            inviteVolunteer.AddExtraInformation("invite-inviteNumber", 1);
                        }
                        inviteVolunteer.Save();
                        //产生InvitedVolunteerVerifyEmailEvent事件
                        EventService.Publish("InvitedVolunteerVerifyEmailEvent", this.Id.ToString() + "," + inviteVolunteer.Id.ToString(), inviteVolunteer.Id);
                        break;
                    }
                }
                this.Save();
                //产生VerifyEmailEvent事件
                EventService.Publish("VerifyEmailEvent", null, this.Id);
            }
        }

        /// <summary>
        /// 验证手机
        /// </summary>
        public void VerifyPhoneNumber()
        {
            if (IsPhoneNumberVerified == false)
            {
                IsPhoneNumberVerified = true;
                this.Save();
                //产生VerifyPhoneNumberEvent事件
                EventService.Publish("VerifyPhoneNumberEvent", null, this.Id);
            }
        }

        public void ModifyEmail(string newEmail)
        {
            throw new NotImplementedException();
        }

        public void ModifyPhoneNumber(string newPhoneNumber)
        {
            throw new NotImplementedException();
        }

        public static void RegisterMe(IVolunteerService volunteerService)
        {
            volunteerService.RegisterMap<User>(new List<string>() { "_userProfiles" });
            VolunteerProfile.RegisterMe(volunteerService);
            OrganizationProfile.RegisterMe(volunteerService);
            OrganizerProfile.RegisterMe(volunteerService);
        }
    }
}
