using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Jtext103.Volunteer.ShortMessage
{
    public class ShortMessageService
    {
        public static ShortMessageService Instance;
        //应用ID
        private string app_id;
        //应用的密钥
        private string app_secret;
        //时间戳，格式为：yyyy-MM-dd hh:mm:ss
        private string timestamp;
        //发送到的手机号
        private string phone;
        //验证码格式必须为6位的纯数字
        private string randcode;
        //验证码过期时间，单位是分钟，默认有效2分钟
        private string exp_time;
        //Client Credentials授权模式
        private string grant_type;
        //获取access_token的URL
        private string access_token_url;
        //获取token的URL
        private string token_url;
        //发送短信的URL
        private string send_url;

        /// <summary>
        /// ShortMessageService构造函数
        /// </summary>
        /// <param name="xmlFileName">xml配置文件名</param>
        public ShortMessageService(string xmlFileName)
        {
            //应用ID
            app_id = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "app_id");
            //应用的密钥
            app_secret = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "app_secret");
            //时间戳，格式为：yyyy-MM-dd hh:mm:ss
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //Client Credentials授权模式
            grant_type = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "grant_type");
            //获取access_token的URL
            access_token_url = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "access_token_url");
            //获取token的URL
            token_url = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "token_url");
            //发送短信的URL
            send_url = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "send_url");
            Instance = this;
        }

        /// <summary>
        /// 发送验证短信
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="randcode">验证码</param>
        /// <param name="expireTime">验证码过期时间</param>
        /// <returns>成功返回true，失败返回false</returns>
        public bool SendShortMessage(string phone, string randcode, int expireTime)
        {
            try
            {
                //时间戳，格式为：yyyy-MM-dd hh:mm:ss
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //发送到的手机号
                this.phone = phone;
                //验证码格式必须为6位的纯数字
                this.randcode = randcode;
                //验证码过期时间，单位是分钟，默认有效2分钟
                this.exp_time = Convert.ToString(expireTime);
                //调用电信提供的webapi发验证短信
                string accessToken = postAccessToken();
                string token = getToken(accessToken);
                string identifier = sendShortMessage(accessToken, token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获得AccessToken
        /// </summary>
        /// <returns></returns>
        private string postAccessToken()
        {
            HttpClient httpClient = new HttpClient();
            var response = httpClient.PostAsync(access_token_url + "?grant_type=" + grant_type + "&app_id=" + app_id + "&app_secret=" + app_secret, null).Result;
            string reponseStringSource = response.Content.ReadAsStringAsync().Result;
            Dictionary<string, string> result = formatHttpReponse(reponseStringSource);
            return result["access_token"];
        }

        /// <summary>
        /// 获得Token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private string getToken(string accessToken)
        {
            string needToEncrypt = "access_token=" + accessToken + "&app_id=" + app_id + "&timestamp=" + timestamp;
            //HMAC hmac = HMACSHA1.Create();
            //hmac.Key = System.Text.Encoding.UTF8.GetBytes(accessKey);
            //var signedData = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(needToEncrypt));
            //string sign = Convert.ToBase64String(signedData);
            string sign = encryptByHMACSHA1(needToEncrypt, app_secret);

            HttpClient httpClient = new HttpClient();
            var response = httpClient.GetAsync(token_url + "?" + needToEncrypt + "&sign=" + sign).Result;
            string reponseStringSource = response.Content.ReadAsStringAsync().Result;
            Dictionary<string, string> result = formatHttpReponse(reponseStringSource);
            return result["token"];
        }

        /// <summary>
        /// 发验证短信
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private string sendShortMessage(string accessToken, string token)
        {
            string needToEncrypt = "access_token=" + accessToken + "&app_id=" + app_id + "&exp_time=" + exp_time + "&phone=" + phone + "&randcode=" + randcode + "&timestamp=" + timestamp + "&token=" + token;
            string sign = encryptByHMACSHA1(needToEncrypt, app_secret);

            HttpClient httpClient = new HttpClient();
            var response = httpClient.PostAsync(send_url + "?" + needToEncrypt + "&sign=" + sign, null).Result;
            string reponseStringSource = response.Content.ReadAsStringAsync().Result;
            Dictionary<string, string> result = formatHttpReponse(reponseStringSource);
            return result["identifier"];
        }

        /// <summary>
        /// 进行HMAC-SHA1加密，再转为base64字符串，最后对url编码
        /// </summary>
        /// <param name="needToEncrypt"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string encryptByHMACSHA1(string needToEncrypt, string key)
        {
            UTF8Encoding utf8encoding = new UTF8Encoding();
            byte[] keybyte = utf8encoding.GetBytes(key);
            byte[] contentbyte = utf8encoding.GetBytes(needToEncrypt);
            byte[] cipherbyte;
            using (HMACSHA1 hmacsha1 = new HMACSHA1(keybyte))
            {
                cipherbyte = hmacsha1.ComputeHash(contentbyte);
            }
            string toBase64 = Convert.ToBase64String(cipherbyte);
            string result = HttpUtility.UrlEncode(toBase64);
            return result;
        }

        /// <summary>
        /// 格式化http相应字符串
        /// </summary>
        /// <param name="reponseSource"></param>
        /// <returns>返回对应字符串的Dictionary<string, string> </returns>
        private Dictionary<string, string> formatHttpReponse(string reponseSource)
        {
            string reponseStringResult = reponseSource.Replace("\"", "").Replace("\n", "").TrimStart('{').TrimEnd('}');
            string[] keyValuePair = reponseStringResult.Split(',');
            Dictionary<string, string> dic = new Dictionary<string, string>();
            for (int i = 0; i < keyValuePair.Length; i++)
            {
                string[] result = keyValuePair[i].Split(':');
                dic.Add(result[0].Trim(), result[1].Trim());
            }
            return dic;
        }

    }
}
