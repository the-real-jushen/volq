using Jtext103.Volunteer.DataModels.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    //六芒星属性
    public class HexagramProperty
    {
        //力量
        public double Strength { get; set; }
        //智力
        public double Intelligence { get; set; }
        //耐力
        public double Endurance { get; set; }
        //爱心
        public double Compassion { get; set; }
        //奉献
        public double Sacrifice { get; set; }

        public HexagramProperty(double strength, double intelligence, double endurance, double compassion, double sacrifice)
        {
            Strength = strength;
            Intelligence = intelligence;
            Endurance = endurance;
            Compassion = compassion;
            Sacrifice = sacrifice;
        }

        public static HexagramProperty operator +(HexagramProperty h1, HexagramProperty h2)
        {
            return new HexagramProperty(h1.Strength + h2.Strength, h1.Intelligence + h2.Intelligence, h1.Endurance + h2.Endurance, h1.Compassion + h2.Compassion, h1.Sacrifice + h2.Sacrifice);
        }
        public static HexagramProperty operator *(HexagramProperty h1, double ratio)
        {
            return new HexagramProperty(h1.Strength * ratio, h1.Intelligence * ratio, h1.Endurance * ratio, h1.Compassion * ratio, h1.Sacrifice * ratio);
        }
        public static HexagramProperty operator *(double ratio, HexagramProperty h1)
        {
            return new HexagramProperty(ratio * h1.Strength, ratio * h1.Intelligence, ratio * h1.Endurance, ratio * h1.Compassion, ratio * h1.Sacrifice);
        }
        public static void RegisterMe(IVolunteerService volunteerService)
        {
            volunteerService.RegisterMap<HexagramProperty>();
        }
    }
}
