using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;

using Jtext103.MongoDBProvider;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.VolunteerEvent;
using Jtext103.Volunteer.Service;
using Jtext103.Volunteer.VolunteerMessage;
using Jtext103.Volunteer.Tag;
using Jtext103.Volunteer.Badge;
using Jtext103.Volunteer.Badge.Interface;
using Jtext103.Volunteer.Friend;
using Jtext103.Volunteer.ActionValidation;
using Jtext103.Volunteer.Mail;
using Jtext103.Volunteer.ShortMessage;
using Jtext103.BlogSystem;
using System.IO;
using Jtext103.ImageHandler;

namespace Jtext103.Volunteer.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //先读config
            Jtext103.StringConfig.ConfigString.Load(HttpRuntime.AppDomainAppPath + "Static\\StringConfig");

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
            Configure(System.Web.Http.GlobalConfiguration.Configuration);
            string db = ConfigurationSettings.AppSettings["DataBase"];
            string eventHandler = ConfigurationSettings.AppSettings["EventHandler"];
            string connection = @"mongodb://115.156.252.5:27017";
            //new service and inject to entity
            MongoDBRepository<Entity> entityRepository = new MongoDBRepository<Entity>(connection, db, "volunteer");
            MongoDBRepository<Message> messageRepository = new MongoDBRepository<Message>(connection, db, "message");
            MongoDBRepository<Message> feedRepository = new MongoDBRepository<Message>(connection, db, "feed");
            MongoDBRepository<TagEntity> activityTagRepository = new MongoDBRepository<TagEntity>(connection, db, "activityTag");
            MongoDBRepository<TagEntity> affiliationRepository = new MongoDBRepository<TagEntity>(connection, db, "affiliation");
            MongoDBRepository<TokenModel> tokenRepository = new MongoDBRepository<TokenModel>(connection, db, "token");
            MongoDBRepository<AuthorizationModel> authorizationRepository = new MongoDBRepository<AuthorizationModel>(connection, "Volunteer", "authorization");
            MongoDBRepository<Event> eventRepository = new MongoDBRepository<Event>(connection, db, "event");
            MongoDBRepository<Subscriber> subscriberRepository = new MongoDBRepository<Subscriber>(connection, db, "subscriber");
            MongoDBRepository<BadgeDescription> badgeDescriptionRepository = new MongoDBRepository<BadgeDescription>(connection, db, "badgeDescription");
            MongoDBRepository<BadgeEntity> badgeEntityRepository = new MongoDBRepository<BadgeEntity>(connection, db, "badgeEntity");
            MongoDBRepository<FriendRelationshipEntity> friendRelationshipEntityRepository = new MongoDBRepository<FriendRelationshipEntity>(connection, db, "friendRelationship");
            MongoDBRepository<ActionValidationModel> actionValidationRepository = new MongoDBRepository<ActionValidationModel>(connection, db, "actionValidation");
            MongoDBRepository<CommentEntity> commentRepository = new MongoDBRepository<CommentEntity>(connection, db, "comment");
            MongoDBRepository<BlogPostEntity> summaryRepository = new MongoDBRepository<BlogPostEntity>(connection, db, "summary");

            //初始化service
            VolunteerService myService = new VolunteerService(entityRepository);
            TokenService tokenService = new TokenService(tokenRepository);
            MessageService messageService = new MessageService(messageRepository);
            MessageService feedService = new MessageService(feedRepository);
            TagService activityTagService = new TagService(activityTagRepository);
            TagService affiliationService = new TagService(affiliationRepository);
            ActionValidationService actionValidationService = new ActionValidationService(actionValidationRepository);
            BlogService blogService = new BlogService(commentRepository, summaryRepository);

            Entity.SetServiceContext(myService);
            EventService.InitService(eventRepository, subscriberRepository, 100, 1000, eventHandler);
            BadgeService.InitService(badgeDescriptionRepository, badgeEntityRepository);
            FriendService.InitService(friendRelationshipEntityRepository);
            ValidationService.InitService(tokenService, myService, authorizationRepository);
            myService.InitVolunteerService(messageService, feedService, activityTagService, affiliationService, actionValidationService, blogService);
            //新建badgeDescription
            BadgeService.RegisterBadgeDescriptions(getIBadge(EventService.EventHandlerList));

            ShortMessageService shortMessageService = new ShortMessageService("VerifyShortMessageSenderAccount.xml");
            MailService mailService = new MailService("NoReplyMailSenderAccount.xml");
            
            //setActivityCover(myService);
        }

        //对所有没有封面且有图片的活动，找到活动中第一张图，裁剪为270*200保存为封面，并保存到本地
        private void setActivityCover(VolunteerService myService)
        {
            IEnumerable<Activity> allActivities = myService.FindAllActivities("", "", false, 0, 0);
            foreach (Activity activity in allActivities)
            {
                if (activity.Cover == null)
                {
                    if (activity.Photos.Count > 0)
                    {
                        try
                        {
                            string firstPhotoPath = HttpContext.Current.Server.MapPath("~" + activity.Photos.FirstOrDefault());
                            Stream fileStream = new FileStream(firstPhotoPath, FileMode.Open);
                            string imageName = Guid.NewGuid().ToString();//生成图像名称
                            string path = "/Static/Images/Activity/" + imageName + ".jpg";//相对路径+图像名称+图像格式
                            string filePath = HttpContext.Current.Server.MapPath("~" + path);//绝对路径
                            activity.Cover = path;
                            //270*200
                            HandleImageService.CutForCustom(fileStream, filePath, 270, 200, 75);
                        }
                        catch
                        {
                            activity.Cover = "/Static/Images/Activity/default.jpg";
                        }
                        activity.Save();
                    }
                    else
                    {
                        activity.Cover = "/Static/Images/Activity/default.jpg";
                        activity.Save();
                    }
                }
            }
        }

        private List<IBadge> getIBadge(List<IVolunteerEventHandler> handlers)
        {
            List<IBadge> result = new List<IBadge>();
            foreach (IVolunteerEventHandler handler in handlers)
            {
                if (typeof(IBadge).IsAssignableFrom(handler.GetType()))
                {
                    result.Add((IBadge)handler);
                }
            }
            return result;
        }

        private void Configure(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.Filters.Add(
                new UnhandledExceptionFilter()
            );
        }


    }
}