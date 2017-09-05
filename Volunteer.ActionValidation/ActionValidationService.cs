using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Jtext103.Repository;
using Jtext103.Repository.Interface;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;


namespace Jtext103.Volunteer.ActionValidation
{
    public class ActionValidationService
    {
        private IRepository<ActionValidationModel> actionValidationRepository;
        public ActionValidationService(IRepository<ActionValidationModel> actionValidationRepository)
        {
            this.actionValidationRepository = actionValidationRepository;
        }

        /// <summary>
        /// 生成一个ActionValidate，存数据库
        /// </summary>
        /// <param name="action"></param>
        /// <param name="target"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        public ActionValidationModel GenerateActionValidate(string action, object target, DateTime expireTime)
        {
            ActionValidationModel actionValidate = new ActionValidationModel(action, target, expireTime);
            actionValidationRepository.SaveOne(actionValidate);
            return actionValidate;
        }

        /// <summary>
        /// 通过一个id找到ActionValidation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionValidationModel FindOneById(Guid id)
        {
            ActionValidationModel actionValidtation = actionValidationRepository.FindOneById(id);
            return actionValidtation;
        }
        public ActionValidationModel FindOneById(string id)
        {
            Guid actionValidationId = new Guid(id);
            ActionValidationModel actionValidtation = actionValidationRepository.FindOneById(actionValidationId);
            return actionValidtation;
        }

        /// <summary>
        /// 将string转为二维码
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="ms">输出流</param>
        public void GenerateQRCode(string content, MemoryStream ms)
        {
            int moduleSize = 12;//二维码大小
            QuietZoneModules quietZones = QuietZoneModules.Two;//空白区域
            ErrorCorrectionLevel ecl = ErrorCorrectionLevel.M;//误差校正水平

            QrEncoder qrEncoder = new QrEncoder(ecl);
            QrCode qrCode = qrEncoder.Encode(content);
            var render = new GraphicsRenderer(new FixedModuleSize(moduleSize, quietZones));
            render.WriteToStream(qrCode.Matrix, System.Drawing.Imaging.ImageFormat.Png, ms);
        }

        /// <summary>
        /// 检查是否过期
        /// </summary>
        /// <param name="actionValidationId"></param>
        /// <returns></returns>
        public bool Validate(string actionValidationId)
        {
            ActionValidationModel actionValidtation = actionValidationRepository.FindOneById(new Guid(actionValidationId));
            if (actionValidtation == null)
            {
                //没找到，则验证不成功
                return false;
            }
            if (actionValidtation.ExpireTime.ToLocalTime() >= DateTime.Now)
            {
                return true;
            }
            else
            {
                //已过期
                return false;
            }
        }

        /// <summary>
        /// 检查是否过期且action是否对应
        /// </summary>
        /// <param name="actionValidationId"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool Validate(string actionValidationId, string action)
        {
            ActionValidationModel actionValidtation = actionValidationRepository.FindOneById(new Guid(actionValidationId));
            if (actionValidtation == null)
            {
                //没找到，则验证不成功
                return false;
            }
            //检查是否过期且action是否对应
            if (actionValidtation.ExpireTime.ToLocalTime() >= DateTime.Now && actionValidtation.Action == action)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 手动删除一个actionValidationId
        /// </summary>
        /// <param name="actionValidationId"></param>
        public void Delete(Guid actionValidationId)
        {
            QueryObject<ActionValidationModel> queryObject = new QueryObject<ActionValidationModel>(actionValidationRepository);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("_id", actionValidationId);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            actionValidationRepository.Delete(queryObject);
        }
        public void Delete(string actionValidationId)
        {
            Guid id = new Guid(actionValidationId);
            QueryObject<ActionValidationModel> queryObject = new QueryObject<ActionValidationModel>(actionValidationRepository);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("_id", id);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            actionValidationRepository.Delete(queryObject);
        }

        /// <summary>
        /// 从数据库中删除所有已经过期的ActionValidation
        /// </summary>
        public void CleanAllExpiredActionValidation()
        {
            QueryObject<ActionValidationModel> queryObject = new QueryObject<ActionValidationModel>(actionValidationRepository);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDic = new Dictionary<string, object>();
            subQueryDic.Add("$lt", DateTime.UtcNow);
            queryDic.Add("ExpireTime", subQueryDic);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            actionValidationRepository.Delete(queryObject);
        }
    }
}
