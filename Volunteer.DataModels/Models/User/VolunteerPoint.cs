using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.VolunteerEvent;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class VolunteerPoint
    {
        private double totalPoint;
        public double TotalPoint
        {
            get
            {
                return totalPoint;
            }
        }

        //points.key是activity的id，如果不是activity得分，则为Guid.Empty
        //points.value是完成该activity获得的点数
        
       
        private Dictionary<Guid, double> points;
        
        public void AddPoints(Guid activityID, double pointValue, User volunteer)
        {
            int level = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevel;
            //当已有非activity得分时，更新该activity得分
            if (activityID == Guid.Empty && points.ContainsKey(Guid.Empty))
            {

                points[Guid.Empty] = points[Guid.Empty] + pointValue;
            }
            else
            {
                points.Add(activityID, pointValue);
            }
            //重新计算totalPoint
            totalPoint = 0;
            foreach (KeyValuePair<Guid, double> point in points)
            {
                totalPoint += point.Value;
            }
            //产生分数变化事件
            EventService.Publish("PointChangeEvent", pointValue.ToString(), volunteer.Id);

            int nowLevel = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevel;
            if (nowLevel > level)
            {
                //升级
                //产生升级事件
                EventService.Publish("LevelupEvent", (nowLevel - level).ToString(), volunteer.Id);
            }
            volunteer.Save();
        }
        public VolunteerPoint()
        {
            points = new Dictionary<Guid, double>();
            totalPoint = 0;
        }
        public static void RegisterMe(IVolunteerService volunteerService)
        {
            volunteerService.RegisterMap<VolunteerPoint>(new List<string>() { "totalPoint", "points" });
        }
    }
}
