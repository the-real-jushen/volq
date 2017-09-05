using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.Service;
using Jtext103.Volunteer.Mail;
using Jtext103.Volunteer.ShortMessage;
using System.Security.Cryptography;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Jtext103.MongoDBProvider;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Jtext103.BlogSystem;
using System.Web;
using Jtext103.ImageHandler;


namespace Jtext103.Volunteer.Web.Controllers
{
    /// <summary>
    /// 所有web api controller的父类，定义初始化service，提供一些公用方法
    /// </summary>
    public abstract class ApiControllerBase : ApiController
    {
        protected VolunteerService myService { get; set; }
        protected TokenService tokenService { get; set; }
        protected MailService mailService { get; set; }
        protected ShortMessageService shortMessageService { get; set; }
        protected ApiControllerBase()
        {
            //MongoDBRepository<Entity> mongo = new MongoDBRepository<Entity>("volunteer");//开发：volunteer，测试：volunteerTest
            //MongoDBRepository<TokenModel> tok = new MongoDBRepository<TokenModel>("token");//开发：token，测试：tokenTest
            myService = VolunteerService.Instance;
            tokenService = TokenService.Instance;
            mailService = MailService.Instance;
            shortMessageService = ShortMessageService.Instance;
            //Entity.SetServiceContext(myService);
        }

        /// <summary>
        /// MD5加密函数
        /// </summary>
        /// <param name="data">加密前字符串</param>
        /// <param name="Key_64">加密关键字</param>
        /// <param name="Iv_64">加密向量</param>
        /// <returns>加密后字符串</returns>
        protected string Encode(string data, string Key_64, string Iv_64)
        {
            string KEY_64 = Key_64;
            string IV_64 = Iv_64;
            try
            {
                byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
                byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);
                DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                int i = cryptoProvider.KeySize;
                MemoryStream ms = new MemoryStream();
                CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);
                StreamWriter sw = new StreamWriter(cst);
                sw.Write(data);
                sw.Flush();
                cst.FlushFinalBlock();
                sw.Flush();
                return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }
            catch (Exception x)
            {
                return x.Message;
            }
        }

        /// <summary>
        /// 读取byte[]并转化为图片
        /// </summary>
        /// <param name="bytes">byte[]</param>
        /// <returns>Image</returns>
        protected Image GetImageByBytes(byte[] bytes)
        {
            Image photo = null;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                ms.Write(bytes, 0, bytes.Length);
                photo = Image.FromStream(ms, true);
            }
            return photo;
        }

        protected byte[] GetByteImage(Image img)
        {
            byte[] bt = null;
            if (!img.Equals(null))
            {
                using (MemoryStream mostream = new MemoryStream())
                {
                    Bitmap bmp = new Bitmap(img);
                    bmp.Save(mostream, System.Drawing.Imaging.ImageFormat.Jpeg);//将图像以指定的格式存入缓存内存流
                    bt = new byte[mostream.Length];
                    mostream.Position = 0;//设置留的初始位置
                    mostream.Read(bt, 0, Convert.ToInt32(bt.Length));
                }
            }
            return bt;
        }

        /// <summary>
        /// 获取访问web api http请求中的token
        /// </summary>
        /// <returns></returns>
        protected Guid GetToken()
        {
            string token = null;
            CookieHeaderValue cookie = Request.Headers.GetCookies("Token").FirstOrDefault();
            if (cookie == null)
            {
                IEnumerable<string> Tokens = new List<string>();
                Request.Headers.TryGetValues("Token", out Tokens);
                if (Tokens == null)
                {
                    return Guid.Empty;
                }
                token = Tokens.FirstOrDefault();
            }
            else
            {
                foreach (CookieState cookieState in cookie.Cookies)
                {
                    string name = cookieState.Name;
                    if (name == "token")
                    {
                        token = cookieState.Value;
                        break;
                    }
                }
                //CookieModel cookieModel = JsonConvert.DeserializeObject<CookieModel>(cookie.ToString());
                //token = cookieModel.token;
            }
            if (token == null)
            {
                return Guid.Empty;
            }
            return new Guid(token);
        }
        /// <summary>
        /// 获取访问web api http请求中的role
        /// </summary>
        /// <returns></returns>
        protected string GetRole()
        {
            string role = null;
            CookieHeaderValue cookie = Request.Headers.GetCookies("role").FirstOrDefault();
            if (cookie == null)
            {
                IEnumerable<string> Roles = new List<string>();
                Request.Headers.TryGetValues("role", out Roles);
                if (Roles == null)
                {
                    return "";
                }
                role = Roles.FirstOrDefault();
            }
            else
            {
                foreach (CookieState cookieState in cookie.Cookies)
                {
                    string name = cookieState.Name;
                    if (name == "role")
                    {
                        role = cookieState.Value;
                        break;
                    }
                }
                //CookieModel cookieModel = JsonConvert.DeserializeObject<CookieModel>(cookie.ToString());
                //token = cookieModel.token;
            }
            if (role == null)
            {
                return "";
            }
            return role;
        }
        /// <summary>
        /// 通过Header判断是否使用手机客户端登录
        /// </summary>
        /// <returns></returns>
        protected bool IfAppLogIn()
        {
            string key = "Client";
            IEnumerable<string> value = new List<string>() { "Volunteer-Android" };
            return Request.Headers.TryGetValues(key, out value);
        }

        protected List<List<string>> ParseToListList(string s)
        {
            List<List<string>> result = new List<List<string>>();
            string[] separator = { "},{", "{{", "}}" };
            string[] s1 = s.Split(separator,100,StringSplitOptions.RemoveEmptyEntries);
            for(var i=0;i<s1.Length;i++)
            {
                List<string> result1 = s1[i].Split(',').ToList<string>();
                result.Add(result1);
            }
            return result;
        }
        protected List<string> ParseToList(string s)
        {
            string[] separator = { "{", "}", "},{", "{{", "}}", ","};
            List<string> result = s.Split(separator, 100, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            return result;
        }

        /// <summary>
        /// 把Action List转换成列表显示的object
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        protected List<ActivityToListShow> transformActivityToListShow(IEnumerable<Activity> activities, User user)
        {
            List<ActivityToListShow> result = new List<ActivityToListShow>();
            foreach (Activity activity in activities)
            {
                ActivityToListShow activityToListShow = new ActivityToListShow()
                {
                    Id = activity.Id,
                    Name = activity.Name,
                    OrganizationName = activity.OrganizationName,
                    Point = activity.Point,
                    Status = activity.Status,
                    OpenSignInTime = activity.OpenSignInTime,
                    StartTime = activity.StartTime,
                    FinishTime = activity.FinishTime,
                    Coordinate = activity.Coordinate,
                    Location = activity.Location,
                    Cover = activity.Cover,
                    Tags = activity.Tags,
                    HasSignedInVolunteerNumber = activity.HasSignedInVolunteerNumber,
                    VolunteerViewedTime = activity.VolunteerViewedTime,
                    VolunteerFavoritedTime = activity.VolunteerFavoritedTime
                };
                if (user != null)
                {
                    if (user.UserRole.Contains(Role.Volunteer))
                    {
                        activityToListShow.hasFavorited = myService.CheckIfVolunteerFavoriteActivity(user, activity.Id);
                        activityToListShow.hasSignined = myService.CheckIfVolunteerSignInActivity(user, activity.Id);
                        activityToListShow.hasViewed = myService.CheckIfVolunteerViewActivity(user, activity.Id);
                    }
                    else
                    {
                        activityToListShow.hasFavorited = false;
                        activityToListShow.hasSignined = false;
                        activityToListShow.hasViewed = false;
                    }
                }
                else
                {
                    activityToListShow.hasFavorited = false;
                    activityToListShow.hasSignined = false;
                    activityToListShow.hasViewed = false;
                }
                result.Add(activityToListShow);
            }
            return result;
        }

        protected List<VolunteerRecordToListShow> transformVolunteerRecordToListShow(IEnumerable<VolunteerParticipateInActivityRecord> records)
        {
            List<VolunteerRecordToListShow> result = new List<VolunteerRecordToListShow>();
            foreach (var record in records)
            {
                User user = (User)(myService.FindOneById(record.VolunteerId));
                VolunteerRecordToListShow one = new VolunteerRecordToListShow
                {
                    VolunteerId = record.VolunteerId,
                    VolunteerName = user.Name,
                    VolunteerSex = user.Sex,
                    VolunteerEmail = user.Email,
                    VolunteerPhoneNumber = user.PhoneNumber,
                    VolunteerStatus = record.VolunteerStatus,
                    SignedIn = record.SignedIn,
                    CheckedIn = record.CheckedIn,
                    CheckedOut = record.CheckedOut,
                    KickedOut = record.KickedOut
                };
                result.Add(one);
            }
            return result;
        }
    }
    /// <summary>
    /// 生成验证码的类
    /// </summary>
    public class ValidateCode
    {
        public ValidateCode()
        {
        }
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="length">指定验证码的长度</param>
        /// <returns></returns>
        public string CreateValidateCode(int length)
        {
            int[] randMembers = new int[length];
            int[] validateNums = new int[length];
            string validateNumberStr = "";
            //生成起始序列值
            int seekSeek = unchecked((int)DateTime.Now.Ticks);
            Random seekRand = new Random(seekSeek);
            int beginSeek = (int)seekRand.Next(0, Int32.MaxValue - length * 10000);
            int[] seeks = new int[length];
            for (int i = 0; i < length; i++)
            {
                beginSeek += 10000;
                seeks[i] = beginSeek;
            }
            //生成随机数字
            for (int i = 0; i < length; i++)
            {
                Random rand = new Random(seeks[i]);
                int pownum = 1 * (int)Math.Pow(10, length);
                randMembers[i] = rand.Next(pownum, Int32.MaxValue);
            }
            //抽取随机数字
            for (int i = 0; i < length; i++)
            {
                string numStr = randMembers[i].ToString();
                int numLength = numStr.Length;
                Random rand = new Random();
                int numPosition = rand.Next(0, numLength - 1);
                validateNums[i] = Int32.Parse(numStr.Substring(numPosition, 1));
            }
            //生成验证码
            for (int i = 0; i < length; i++)
            {
                validateNumberStr += validateNums[i].ToString();
            }
            return validateNumberStr;
        }
        /// <summary>
        /// 创建验证码的图片,base64
        /// </summary>
        /// <param name="containsPage">要输出到的page对象</param>
        /// <param name="validateNum">验证码</param>
        public string CreateValidateGraphic(string validateCode)
        {
            Bitmap image = new Bitmap((int)Math.Ceiling(validateCode.Length * 20.0), 35);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机生成器
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                //画图片的干扰线
                for (int i = 0; i < 30; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", 20, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateCode, font, brush, 3, 2);
                //画图片的前景干扰点
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                //保存图片数据
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                //输出图片流
                return Convert.ToBase64String(stream.ToArray());
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }
    }
}