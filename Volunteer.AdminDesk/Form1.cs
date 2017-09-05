using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using HtmlAgilityPack;
using Jtext103.Volunteer.Service;
using Jtext103.Volunteer.VolunteerMessage;
using Jtext103.Volunteer.Tag;
using Jtext103.MongoDBProvider;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Repository.Interface;
using System.Net;
using Message = Jtext103.Volunteer.VolunteerMessage.Message;
using Jtext103.Volunteer.ActionValidation;
using Jtext103.Volunteer.VolunteerEvent;
using Jtext103.BlogSystem;
using Jtext103.Volunteer.Mail;
using Jtext103.Volunteer.Badge;
using Jtext103.Volunteer.Friend;

namespace Volunteer.AdminDesk
{
    public partial class Form1 : Form
    {
        private VolunteerService myService;
        private TokenService tokenService;
        private MailService mailService;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButtonVolunteerProduction.Checked = true;
            buttonIndexRefresh_Click(buttonIndexRefresh, e);
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            this.buttonIndexRefresh.Click += new System.EventHandler(this.buttonIndexRefresh_Click);
            this.buttonRemoveApi.Click += new System.EventHandler(this.buttonRemoveApi_Click);
            this.buttonAddApi.Click += new System.EventHandler(this.buttonAddApi_Click);
            this.buttonApiRefresh.Click += new System.EventHandler(this.buttonApiRefresh_Click);
            this.buttonApiSave.Click += new System.EventHandler(this.buttonApiSave_Click);
            this.listViewApiList.SelectedIndexChanged += new System.EventHandler(this.listViewApiList_SelectedIndexChanged);
            this.buttonOrganizationRefresh.Click += new System.EventHandler(this.buttonOrganizationRefresh_Click);
            this.buttonOrganizationSave.Click += new System.EventHandler(this.buttonOrganizationSave_Click);
            this.listViewOrganization.SelectedIndexChanged += new System.EventHandler(this.listViewOrganization_SelectedIndexChanged);
            this.buttonOrganizerRefresh.Click += new System.EventHandler(this.buttonOrganizerRefresh_Click);
            this.buttonPublishFeed.Click += new System.EventHandler(this.buttonPublishFeed_Click);
            this.buttonVolunteerRefresh.Click += new System.EventHandler(this.buttonVolunteerRefresh_Click);
            this.buttonActivityRefresh.Click += new System.EventHandler(this.buttonActivityRefresh_Click);
            toolStripStatusLabel2.Text = "加载成功！";
        }

        /// <summary>
        /// 切换tabpage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPageIndex)
            {
                case 0:
                    break;
                case 1:
                    buttonApiRefresh_Click(buttonApiRefresh, e);
                    break;
                case 2:
                    buttonOrganizationRefresh_Click(buttonOrganizationRefresh, e);
                    break;
                case 3:
                    buttonOrganizerRefresh_Click(buttonOrganizerRefresh, e);
                    break;
                case 4:
                    buttonVolunteerRefresh_Click(buttonVolunteerRefresh, e);
                    break;
                case 5:
                    buttonActivityRefresh_Click(buttonActivityRefresh, e);
                    break;
                default:
                    break;
            }
            toolStripStatusLabel2.Text = "";
        }

        #region 首页
        /// <summary>
        /// collection和service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonIndexRefresh_Click(object sender, EventArgs e)
        {
            string db="";
            if(radioButtonVolunteerTest.Checked)
            {
                db = "VolunteerTest";
                toolStripStatusLabel1.Text = "【开发模式】";
            }
            else if (radioButtonVolunteer.Checked)
            {
                db = "Volunteer";
                toolStripStatusLabel1.Text = "【测试模式】";
            }
            else
            {
                db = "VolunteerProduction";
                toolStripStatusLabel1.Text = "【产品模式】";
            }
            string connection = @"mongodb://115.156.252.5:27017";
            MongoDBRepository<Entity> mongo = new MongoDBRepository<Entity>(connection, db, "volunteer");
            MongoDBRepository<TokenModel> tok = new MongoDBRepository<TokenModel>(connection, db, "token");
            MongoDBRepository<AuthorizationModel> aut = new MongoDBRepository<AuthorizationModel>(connection, "Volunteer", "authorization");
            MongoDBRepository<Message> messageRepository = new MongoDBRepository<Message>(connection, db, "message");
            MongoDBRepository<Message> feedRepository = new MongoDBRepository<Message>(connection, db, "feed");
            MongoDBRepository<TagEntity> activityTagRepository = new MongoDBRepository<TagEntity>(connection, db, "activityTag");
            MongoDBRepository<TagEntity> affiliationRepository = new MongoDBRepository<TagEntity>(connection, db, "affiliation");
            MongoDBRepository<Event> eventRepository = new MongoDBRepository<Event>(connection, db, "event");
            MongoDBRepository<Subscriber> subscriberRepository = new MongoDBRepository<Subscriber>(connection, db, "subscriber");
            MongoDBRepository<ActionValidationModel> actionValidationRepository = new MongoDBRepository<ActionValidationModel>(connection, db, "actionValidation");
            MongoDBRepository<CommentEntity> commentRepository = new MongoDBRepository<CommentEntity>(connection, db, "comment");
            MongoDBRepository<BlogPostEntity> summaryRepository = new MongoDBRepository<BlogPostEntity>(connection, db, "summary");
            MongoDBRepository<BadgeDescription> badgeDescriptionRepository = new MongoDBRepository<BadgeDescription>(connection, db, "badgeDescription");
            MongoDBRepository<BadgeEntity> badgeEntityRepository = new MongoDBRepository<BadgeEntity>(connection, db, "badgeEntity");
            MongoDBRepository<FriendRelationshipEntity> friendRelationshipEntityRepository = new MongoDBRepository<FriendRelationshipEntity>(connection, db, "friendRelationship");

            MessageService messageService = new MessageService(messageRepository);
            MessageService feedService = new MessageService(feedRepository);
            TagService activityTagService = new TagService(activityTagRepository);
            TagService affiliationService = new TagService(affiliationRepository);
            ActionValidationService actionValidationService = new ActionValidationService(actionValidationRepository);
            BlogService commentService = new BlogService(commentRepository, summaryRepository);
            BadgeService.InitService(badgeDescriptionRepository, badgeEntityRepository);
            EventService.InitService(eventRepository, subscriberRepository, 100, 1000, @"C:\eventHandler");
            FriendService.InitService(friendRelationshipEntityRepository);

            myService = new VolunteerService(mongo);
            myService.InitVolunteerService(messageService, feedService, activityTagService, affiliationService, actionValidationService, commentService);
            tokenService = new TokenService(tok);
            //先读config
            string staticFilePath = System.AppDomain.CurrentDomain.BaseDirectory;
            staticFilePath = staticFilePath.Substring(0, staticFilePath.IndexOf("\\Volunteer\\") + 11) + "Volunteer.Web\\";
            Jtext103.StringConfig.ConfigString.Load(staticFilePath + "Static\\StringConfig");
            mailService = new MailService("ServiceMailSenderAccount.xml");
            Entity.SetServiceContext(myService);
            ValidationService.InitService(tokenService, myService, aut);
            toolStripStatusLabel2.Text = "已重载模式！";
        }
        private void buttonDeleteAllUnReferencedStaticFile_Click(object sender, EventArgs e)
        {
            string staticFilePath = System.AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");
            staticFilePath = staticFilePath.Substring(0, staticFilePath.IndexOf("/Volunteer/") + 10) + "/Volunteer.Web/Static";
            toolStripStatusLabel2.Text = myService.DeleteAllUnReferencedStaticFile(staticFilePath);
        }
        private void buttonSendEmailToAll_Click(object sender, EventArgs e)
        {
            long count = 0;
            IEnumerable<User> users = myService.FindAllUsers("Name", true, 0, 0);
            foreach (User user in users)
            {
                if (mailService.SendMail(user.Email, textBoxEmailTitleToAll.Text, textBoxEmailContentToAll.Text))
                {
                    count++;
                }
                //跟新状态栏
                toolStripStatusLabel2.Text = "已向" + count + "人成功发送邮件，" + (users.LongCount() - count) + "人发送失败！";
            }
        }
        #endregion

        #region WebApi授权
        /// <summary>
        /// 响应listViewApiList选中item改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewApiList_SelectedIndexChanged(object sender, EventArgs e)
        {
            AuthorizationModel authorizationModel = null;
            List<Guid> allowedUsers = new List<Guid>() { };
            List<Role> allowedRoles = new List<Role>() { };
            List<Guid> forbidUsers = new List<Guid>() { };
            List<Role> forbidRoles = new List<Role>() { };
            bool isSelected = true;
            //清空各个授权策略列表
            toolStripStatusLabel2.Text = "";
            checkedListBoxAR.Items.Clear();
            checkedListBoxAU.Items.Clear();
            checkedListBoxFR.Items.Clear();
            checkedListBoxFU.Items.Clear();
            labelFR.Text = "0";
            labelFU.Text = "0";
            labelAR.Text = "0";
            labelAU.Text = "0";
            textBoxInput.Text = "";
            textBoxOutput.Text = "";
            textBoxDescription.Text = "";
            textBoxLastModifyTime.Text = "";
            checkBoxIfHighlight.Checked = false;
            //判断是否有选中webapi，如果有，则从数据库中查找authorizationModel
            if (((ListView)sender).SelectedItems.Count > 0)
            {
                textBoxApiName.Text = ((ListView)sender).SelectedItems[0].Text;
                textBoxGuid.Text = ((ListView)sender).SelectedItems[0].ToolTipText;
                authorizationModel = ValidationService.FindAuthorizationModel(textBoxApiName.Text);
            }
            else
            {
                textBoxApiName.Text = "(未选中)";
                textBoxGuid.Text = "(未选中)";
                isSelected = false;
            }
            //如果authorizationModel存在，则获取相关授权信息，并对其中已经失效的用户id予以删除
            if (authorizationModel != null)
            {
                textBoxLastModifyTime.Text = ((DateTime)authorizationModel.ExtraInformation["LastModifyTime"]).ToLocalTime().ToString();
                checkBoxIfHighlight.Checked = (bool)authorizationModel.ExtraInformation["IfHighlight"];
                allowedUsers = authorizationModel.AllowedUsers;
                allowedRoles = authorizationModel.AllowedRoles;
                forbidUsers = authorizationModel.ForbidUsers;
                forbidRoles = authorizationModel.ForbidRoles;
                foreach (var a in allowedUsers)
                {
                    if (myService.FindUser(a) == null)
                    {
                        allowedUsers.Remove(a);
                    }
                }
                foreach (var a in forbidUsers)
                {
                    if (myService.FindUser(a) == null)
                    {
                        forbidUsers.Remove(a);
                    }
                }
                ValidationService.SaveOne(authorizationModel);
                textBoxInput.Text = authorizationModel.Input;
                textBoxOutput.Text = authorizationModel.Output;
                textBoxDescription.Text = authorizationModel.Description;
            }

            //添加新的策略列表，并根据已有策略决定是否选中
            IEnumerable<User> users = myService.FindAllUsers("Name", true, 0, 0);
            foreach (User user in users)
            {
                checkedListBoxAU.Items.Add(user.Name + "#" + user.Email, isSelected && allowedUsers.Contains(user.Id));
                checkedListBoxFU.Items.Add(user.Name + "#" + user.Email, isSelected && forbidUsers.Contains(user.Id));
            }
            foreach (string rolename in Enum.GetNames(typeof(Role)))
            {
                checkedListBoxAR.Items.Add(rolename, isSelected && allowedRoles.Contains((Role)Enum.Parse(typeof(Role), rolename)));
                checkedListBoxFR.Items.Add(rolename, isSelected && forbidRoles.Contains((Role)Enum.Parse(typeof(Role), rolename)));
            }
            //显示各个策略列表的选中数目
            labelAR.Text = checkedListBoxAR.CheckedItems.Count.ToString();
            labelAU.Text = checkedListBoxAU.CheckedItems.Count.ToString();
            labelFR.Text = checkedListBoxFR.CheckedItems.Count.ToString();
            labelFU.Text = checkedListBoxFU.CheckedItems.Count.ToString(); 
        }
        private void buttonApiRefresh_Click(object sender, EventArgs e)
        {
            //从服务器获取WebApiList.html，提取webapi信息，加载到授权页面
            //Uri url = new Uri("http://115.156.252.231:8088/WebApiList.html");
            //WebClient client = new WebClient();
            //string html = client.DownloadString(url);
            //HtmlAgilityPack.HtmlDocument apiListHtml = new HtmlAgilityPack.HtmlDocument();
            //apiListHtml.LoadHtml(html);
            //HtmlNodeCollection details = apiListHtml.DocumentNode.SelectNodes("//details");
            List<AuthorizationModel> models = ValidationService.FindAllAuthorizationModel().ToList();

            listViewApiList.Items.Clear();

            foreach (var model in models)
            {
                if (!model.ExtraInformation.ContainsKey("LastModifyTime"))
                {
                    model.AddExtraInformation("LastModifyTime", DateTime.Now);
                }
                if (!model.ExtraInformation.ContainsKey("IfHighlight"))
                {
                    model.AddExtraInformation("IfHighlight", false);
                }
                ValidationService.SaveOne(model);
                string kind, apiName = model.ApiName;
                int start = apiName.IndexOf("api/") + 4;
                if (apiName.IndexOf("/", start) > 0)
                {
                    int length = apiName.IndexOf("/", start) - start;
                    kind = apiName.Substring(start, length);
                }
                else if (apiName.IndexOf("?", start) > 0)
                {
                    int length = apiName.IndexOf("?", start) - start;
                    kind = apiName.Substring(start, length);
                }
                else
                {
                    kind = apiName.Substring(start);
                }
                ListViewItem item;
                try
                {
                    item = new ListViewItem(apiName, listViewApiList.Groups[kind]);
                }
                catch
                {
                    item = new ListViewItem(apiName);
                }
                if ((bool)model.ExtraInformation["IfHighlight"])
                {
                    item.BackColor = Color.Pink;
                }
                else
                {
                    item.BackColor = Color.White;
                }
                item.ToolTipText = model.Id.ToString();
                listViewApiList.Items.Add(item);
            }
            labelWebApiNumber.Text = models.Count().ToString();
            //清空各个授权策略列表
            //添加新的策略列表，并根据已有策略决定是否选中
            checkedListBoxAR.Items.Clear();
            checkedListBoxAU.Items.Clear();
            checkedListBoxFR.Items.Clear();
            checkedListBoxFU.Items.Clear();
            IEnumerable<User> users = myService.FindAllUsers("Name", true, 0, 0);
            foreach (User user in users)
            {
                checkedListBoxAU.Items.Add(user.Name + "#" + user.Email, false);
                checkedListBoxFU.Items.Add(user.Name + "#" + user.Email, false);
            }
            foreach (string rolename in Enum.GetNames(typeof(Role)))
            {
                checkedListBoxAR.Items.Add(rolename, false);
                checkedListBoxFR.Items.Add(rolename, false);
            }
            labelFR.Text = "0";
            labelFU.Text = "0";
            labelAR.Text = "0";
            labelAU.Text = "0";
            textBoxLastModifyTime.Text = "";
            checkBoxIfHighlight.Checked = false;
            textBoxInput.Text = "";
            textBoxOutput.Text = "";
            textBoxDescription.Text = "";
            textBoxApiName.Text = "(未选中)";
            textBoxGuid.Text = "(未选中)";
            toolStripStatusLabel2.Text = "已更新！";
        }
        private void buttonApiSave_Click(object sender, EventArgs e)
        {
            AuthorizationModel authorizationModel = null;
            List<Guid> allowedUsers = new List<Guid>() { };
            List<Role> allowedRoles = new List<Role>() { };
            List<Guid> forbidUsers = new List<Guid>() { };
            List<Role> forbidRoles = new List<Role>() { };
            //判断是否选中webapi(含新加)，如果选中，则从数据库查找相应的authorizationModel
            if (textBoxGuid.Text.Equals("(未选中)"))
            {
                return;

            }
            else if (textBoxGuid.Text.Equals("New AuthorizationModel Guid"))
            {
                authorizationModel = new AuthorizationModel();
                authorizationModel.AddExtraInformation("LastModifyTime", DateTime.Now);
                authorizationModel.AddExtraInformation("IfHighlight", true);
                labelWebApiNumber.Text = (int.Parse(labelWebApiNumber.Text) + 1).ToString();
            }
            else
            {
                authorizationModel = ValidationService.FindAuthorizationModel(new Guid(textBoxGuid.Text));
            }
            AuthorizationModel lastAuthorizationModel = new AuthorizationModel
            {
                ApiName = authorizationModel.ApiName,
                Description = authorizationModel.Description,
                Input = authorizationModel.Input,
                Output = authorizationModel.Output,
                ForbidRoles = authorizationModel.ForbidRoles,
                ForbidUsers = authorizationModel.ForbidUsers,
                AllowedRoles = authorizationModel.AllowedRoles,
                AllowedUsers = authorizationModel.AllowedUsers,
            };
            //保存ApiName
            authorizationModel.ApiName = textBoxApiName.Text.Trim().ToLower();
            string kind;
            int start = authorizationModel.ApiName.IndexOf("api/") + 4;
            if (authorizationModel.ApiName.IndexOf("/", start) > 0)
            {
                int length = authorizationModel.ApiName.IndexOf("/", start) - start;
                kind = authorizationModel.ApiName.Substring(start, length);
            }
            else if (authorizationModel.ApiName.IndexOf("?", start) > 0)
            {
                int length = authorizationModel.ApiName.IndexOf("?", start) - start;
                kind = authorizationModel.ApiName.Substring(start, length);
            }
            else
            {
                kind = authorizationModel.ApiName.Substring(start);
            }
            //根据各个策略列表，获取新的授权策略
            foreach (var item in checkedListBoxAU.CheckedItems)
            {
                allowedUsers.Add(myService.FindUser(item.ToString().Substring(item.ToString().IndexOf("#") + 1)).Id);
            }
            foreach (var item in checkedListBoxFU.CheckedItems)
            {
                forbidUsers.Add(myService.FindUser(item.ToString().Substring(item.ToString().IndexOf("#") + 1)).Id);
            }
            foreach (var item in checkedListBoxAR.CheckedItems)
            {
                allowedRoles.Add((Role)Enum.Parse(typeof(Role), item.ToString()));
            }
            foreach (var item in checkedListBoxFR.CheckedItems)
            {
                forbidRoles.Add((Role)Enum.Parse(typeof(Role), item.ToString()));
            }
            //将新的授权策略保存到数据库中
            authorizationModel.AllowedUsers = allowedUsers;
            authorizationModel.ForbidUsers = forbidUsers;
            authorizationModel.AllowedRoles = allowedRoles;
            authorizationModel.ForbidRoles = forbidRoles;
            authorizationModel.Input = textBoxInput.Text;
            authorizationModel.Output = textBoxOutput.Text;
            authorizationModel.Description = textBoxDescription.Text;
            //保存修改信息
            authorizationModel.ExtraInformation["LastModifyTime"] = DateTime.Now;
            if (!authorizationModel.ExtraInformation.ContainsKey("LastVersion"))
            {
                authorizationModel.AddExtraInformation("LastVersion", lastAuthorizationModel);
            }
            else
            {
                authorizationModel.ExtraInformation["LastVersion"] = lastAuthorizationModel;
            }

            ValidationService.SaveOne(authorizationModel);

            //显示各个策略列表的选中数目
            labelAR.Text = checkedListBoxAR.CheckedItems.Count.ToString();
            labelAU.Text = checkedListBoxAU.CheckedItems.Count.ToString();
            labelFR.Text = checkedListBoxFR.CheckedItems.Count.ToString();
            labelFU.Text = checkedListBoxFU.CheckedItems.Count.ToString();
            textBoxGuid.Text = authorizationModel.Id.ToString();
            ListViewItem newItem;
            try
            {
                newItem = new ListViewItem(authorizationModel.ApiName, listViewApiList.Groups[kind]);
            }
            catch
            {
                newItem = new ListViewItem(authorizationModel.ApiName);
            }
            if (listViewApiList.SelectedItems.Count > 0)
            {
                listViewApiList.Items.Remove(listViewApiList.SelectedItems[0]);
            }
            newItem.ToolTipText = authorizationModel.Id.ToString();
            newItem.Selected = true;
            listViewApiList.Items.Add(newItem);

            //跟新状态栏
            toolStripStatusLabel2.Text = authorizationModel.ApiName + "已保存！";
        }
        private void buttonAddApi_Click(object sender, EventArgs e)
        {
            //listViewApiList.SelectedItems.Clear();
            textBoxApiName.Text = "";
            textBoxGuid.Text = "New AuthorizationModel Guid";
            textBoxLastModifyTime.Text = "";
            checkBoxIfHighlight.Checked = true;
            //跟新状态栏
            toolStripStatusLabel2.Text = "请添加新API授权规则！（不要输入中文字符和空格，大写字母将会被转换为小写字母！）";
        }
        private void buttonRemoveApi_Click(object sender, EventArgs e)
        {
            AuthorizationModel authorizationModel = null;

            //判断是否选中webapi，如果选中，则从数据库查找相应的authorizationModel
            if (listViewApiList.SelectedItems.Count > 0)
            {
                authorizationModel = ValidationService.FindAuthorizationModel(new Guid(textBoxGuid.Text));
            }
            if (authorizationModel == null)
            {
                return;
            }
            ValidationService.DeleteOne(authorizationModel);
            listViewApiList.Items.Remove(listViewApiList.SelectedItems[0]);
            labelWebApiNumber.Text = (int.Parse(labelWebApiNumber.Text) - 1).ToString();
            //跟新状态栏
            toolStripStatusLabel2.Text = authorizationModel.ApiName + "已删除！";
        }
        private void checkBoxIfHighlight_CheckStateChanged(object sender, EventArgs e)
        {
            AuthorizationModel authorizationModel = null;
            if (listViewApiList.SelectedItems.Count > 0 && !textBoxGuid.Text.Equals("New AuthorizationModel Guid"))
            {
                authorizationModel = ValidationService.FindAuthorizationModel(new Guid(textBoxGuid.Text));
                if (checkBoxIfHighlight.Checked)
                {
                    toolStripStatusLabel2.Text = "已将" + authorizationModel.ApiName + "设置高亮！";
                    listViewApiList.SelectedItems[0].BackColor = Color.Pink;
                }
                else
                {
                    toolStripStatusLabel2.Text = "已取消" + authorizationModel.ApiName + "高亮显示！";
                    listViewApiList.SelectedItems[0].BackColor = Color.White;
                }
                authorizationModel.ExtraInformation["IfHighlight"] = checkBoxIfHighlight.Checked;
                ValidationService.SaveOne(authorizationModel);
            }
        }
        #endregion

        #region Organization
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewOrganization_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrganizationProfile organizationProfile = null;
            //清空各个授权策略列表
            toolStripStatusLabel2.Text = "";
            numericUpDownTotalPoint.Value = 0;
            numericUpDownRemainPoint.Value = 0;

            //判断是否有选中organization，如果有，则从数据库中查找organizationProfile
            if (((ListView)sender).SelectedItems.Count > 0)
            {
                labelOrganizationName.Text = ((ListView)sender).SelectedItems[0].Text;
                User organization = myService.FindUser(((ListView)sender).SelectedItems[0].SubItems[1].Text);
                organizationProfile = (OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"];
            }
            else
            {
                labelOrganizationName.Text = "(未选中)";
                return;
            }

            if (organizationProfile == null)
            {
                return ;
            }

            numericUpDownTotalPoint.Value = (decimal)organizationProfile.TotalPointEachMonth;
            numericUpDownRemainPoint.Value = (decimal)organizationProfile.RemainingSum;
        }

        private void buttonOrganizationRefresh_Click(object sender, EventArgs e)
        {
            listViewOrganization.Items.Clear();
            foreach (var a in myService.FindAllOrganizations("Name", true, 0, 0))
            {
                ListViewItem item = new ListViewItem(a.Name);
                ListViewItem.ListViewSubItem subItem1 = new ListViewItem.ListViewSubItem(item, a.Email);
                string profile = string.Empty;
                foreach (var b in a.UserProfiles.AllUserProfile)
                {
                    profile += b.ProfileName + "；";
                }
                ListViewItem.ListViewSubItem subItem2 = new ListViewItem.ListViewSubItem(item, profile);
                item.SubItems.Add(subItem1);
                item.SubItems.Add(subItem2);
                listViewOrganization.Items.Add(item);
            }
            labelOrganizationNumber.Text = listViewOrganization.Items.Count.ToString();
            //清空各个授权策略列表
            toolStripStatusLabel2.Text = "";
            numericUpDownTotalPoint.Value = 0;
            numericUpDownRemainPoint.Value = 0;
            toolStripStatusLabel2.Text = "已更新！";
        }

        private void buttonOrganizationSave_Click(object sender, EventArgs e)
        {
            User organization = null;
            OrganizationProfile organizationProfile = null;
            //判断是否选中webapi，如果选中，则从数据库查找相应的authorizationModel
            if (listViewOrganization.SelectedItems.Count > 0)
            {
                organization = myService.FindUser(listViewOrganization.SelectedItems[0].SubItems[1].Text);
                organizationProfile = (OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"];
            }
            else
            {
                return;
            }
            organizationProfile.TotalPointEachMonth = (double)numericUpDownTotalPoint.Value;
            organizationProfile.RemainingSum = (double)numericUpDownRemainPoint.Value;
            myService.SaveOne(organization);
            //跟新状态栏
            toolStripStatusLabel2.Text = organization.Name+"已保存！";
        }

        private void buttonRemoveOrganization_Click(object sender, EventArgs e)
        {
            bool success = true;
            if (listViewOrganization.SelectedItems.Count > 0)
            {
                success = false;
            }
            if (success == true)
            {
                //跟新状态栏
                toolStripStatusLabel2.Text = listViewOrganization.SelectedItems[0].Text + "已删除！";
                listViewOrganization.Items.Remove(listViewOrganization.SelectedItems[0]);
                labelOrganizationNumber.Text = (int.Parse(labelOrganizationNumber.Text) - 1).ToString();
            }
        }

        private void buttonInviteOrganization_Click(object sender, EventArgs e)
        {
            Guid id = myService.GenerateOrganizationRegisterActionValidation();
            textBoxInviteLink.Text = "http://www.volq.org/views/register.html?id=" + id.ToString();
        }
        private void buttonSendEmailToOrganization_Click(object sender, EventArgs e)
        {
            int success = 0;
            int fail = 0;
            foreach (ListViewItem selectedItem in listViewOrganization.SelectedItems)
            {
                if (mailService.SendMail(selectedItem.SubItems[1].Text, textBoxEmailTitleToOrganization.Text, textBoxEmailContentToOrganization.Text))
                {
                    success++;
                }
                else
                {
                    fail++;
                }
                //跟新状态栏
                toolStripStatusLabel2.Text = "还剩" + (listViewVolunteer.SelectedItems.Count - success - fail) + "人，已向" + success + "人成功发送邮件，" + fail + "人发送失败！";
            }
        }
        #endregion

        #region Organizer
        private void buttonOrganizerRefresh_Click(object sender, EventArgs e)
        {
            listViewOrganizer.Items.Clear();
            foreach (var a in myService.FindAllOrganizers("Name", true, 0, 0))
            {
                ListViewItem item = new ListViewItem(a.Name);
                ListViewItem.ListViewSubItem subItem1 = new ListViewItem.ListViewSubItem(item, a.Email);
                string profile = string.Empty;
                foreach (var b in a.UserProfiles.AllUserProfile)
                {
                    profile += b.ProfileName + "；";
                }
                ListViewItem.ListViewSubItem subItem2 = new ListViewItem.ListViewSubItem(item, profile);
                item.SubItems.Add(subItem1);
                item.SubItems.Add(subItem2);
                listViewOrganizer.Items.Add(item);
            }
            labelOrganizerNumber.Text = listViewOrganizer.Items.Count.ToString();
            toolStripStatusLabel2.Text = "已更新！";
        }
        private void buttonRemoveOrganizer_Click(object sender, EventArgs e)
        {
            bool success = true;
            if (listViewOrganizer.SelectedItems.Count > 0)
            {
                success = false;
            }
            if (success == true)
            {
                //跟新状态栏
                toolStripStatusLabel2.Text = listViewOrganizer.SelectedItems[0].Text + "已删除！";
                listViewOrganizer.Items.Remove(listViewOrganizer.SelectedItems[0]);
                labelOrganizerNumber.Text = (int.Parse(labelOrganizerNumber.Text) - 1).ToString();
            }
        }
        private void buttonSendEmailToOrganizer_Click(object sender, EventArgs e)
        {
            int success = 0;
            int fail = 0;
            foreach (ListViewItem selectedItem in listViewOrganizer.SelectedItems)
            {
                if (mailService.SendMail(selectedItem.SubItems[1].Text, textBoxEmailTitleToOrganizer.Text, textBoxEmailContentToOrganizer.Text))
                {
                    success++;
                }
                else
                {
                    fail++;
                }
                //跟新状态栏
                toolStripStatusLabel2.Text = "还剩" + (listViewVolunteer.SelectedItems.Count - success - fail) + "人，已向" + success + "人成功发送邮件，" + fail + "人发送失败！";
            }
        }
        #endregion

        #region Volunteer
        private void buttonVolunteerRefresh_Click(object sender, EventArgs e)
        {
            listViewVolunteer.Items.Clear();
            foreach (var a in myService.FindAllVolunteers("Name", true, 0, 0))
            {
                ListViewItem item = new ListViewItem(a.Name);
                ListViewItem.ListViewSubItem subItem1 = new ListViewItem.ListViewSubItem(item, a.Email);
                string profile = string.Empty;
                foreach (var b in a.UserProfiles.AllUserProfile)
                {
                    profile += b.ProfileName + "；";
                }
                ListViewItem.ListViewSubItem subItem2 = new ListViewItem.ListViewSubItem(item, profile);
                item.SubItems.Add(subItem1);
                item.SubItems.Add(subItem2);
                item.Tag = a.Id;
                listViewVolunteer.Items.Add(item);
            }
            comboBoxBadge.Items.Clear();
            foreach (BadgeDescription badgeDescription in BadgeService.FindAllBadges())
            {
                comboBoxBadge.Items.Add(badgeDescription.BadgeName);
            }
            labelVolunteerNumber.Text = listViewVolunteer.Items.Count.ToString();
            textBoxFeedTitle.Text = "";
            textBoxFeedContent.Text = "";
            toolStripStatusLabel2.Text = "已更新！";
        }
        private void buttonPublishFeed_Click(object sender, EventArgs e)
        {
            foreach(ListViewItem selectedItem in listViewVolunteer.SelectedItems)
            {
                myService.FeedService.SendMessage("Admin",(Guid)selectedItem.Tag,textBoxFeedTitle.Text,textBoxFeedContent.Text,null,null,false);
            }
            //跟新状态栏
            toolStripStatusLabel2.Text = "新Feed已发布！";
        }
        private void buttonRemoveVolunteer_Click(object sender, EventArgs e)
        {
            bool success = true;
            if (listViewVolunteer.SelectedItems.Count > 0)
            {
                success = false;
            }
            if (success == true)
            {
                //跟新状态栏
                toolStripStatusLabel2.Text = listViewVolunteer.SelectedItems[0].Text + "已删除！";
                listViewVolunteer.Items.Remove(listViewVolunteer.SelectedItems[0]);
                labelVolunteerNumber.Text = (int.Parse(labelVolunteerNumber.Text) - 1).ToString();
            }
        }
        private void buttonSendEmailToVolunteer_Click(object sender, EventArgs e)
        {
            int success = 0;
            int fail = 0;
            foreach (ListViewItem selectedItem in listViewVolunteer.SelectedItems)
            {
                if (mailService.SendMail(selectedItem.SubItems[1].Text, textBoxFeedTitle.Text, textBoxFeedContent.Text))
                {
                    success++;
                }
                else
                {
                    fail++;
                }
                //跟新状态栏
                toolStripStatusLabel2.Text = "还剩" + (listViewVolunteer.SelectedItems.Count - success - fail) + "人，已向" + success + "人成功发送邮件，" + fail + "人发送失败！";
            }
        }
        private void buttonGiveBadgeToVolunteer_Click(object sender, EventArgs e)
        {
            int success = 0;
            int fail = 0;
            if (comboBoxBadge.SelectedItem == null)
            {
                return;
            }
            foreach (ListViewItem selectedItem in listViewVolunteer.SelectedItems)
            {
                if (!BadgeService.CheckIfBadgeGranted((Guid)selectedItem.Tag, (string)comboBoxBadge.SelectedItem))
                {
                    BadgeService.GrantBadge((Guid)selectedItem.Tag, (string)comboBoxBadge.SelectedItem);
                    success++;
                }
                else
                {
                    fail++;
                }
                //跟新状态栏
                toolStripStatusLabel2.Text = "还剩" + (listViewVolunteer.SelectedItems.Count - success - fail) + "人，" + fail + "人已有此徽章，成功向" + success + "人颁发徽章" + comboBoxBadge.SelectedText + "！";
            }
        }
        private void buttonUngiveBadgeToVolunteer_Click(object sender, EventArgs e)
        {
            int success = 0;
            int fail = 0;
            if (comboBoxBadge.SelectedItem == null)
            {
                return;
            }
            foreach (ListViewItem selectedItem in listViewVolunteer.SelectedItems)
            {
                if (BadgeService.CheckIfBadgeGranted((Guid)selectedItem.Tag, (string)comboBoxBadge.SelectedItem))
                {
                    BadgeService.UngrantBadge((Guid)selectedItem.Tag, (string)comboBoxBadge.SelectedItem);
                    success++;
                }
                else
                {
                    fail++;
                }
                //跟新状态栏
                toolStripStatusLabel2.Text = "还剩" + (listViewVolunteer.SelectedItems.Count - success - fail) + "人，" + fail + "人本无此徽章，成功向" + success + "人收回徽章" + comboBoxBadge.SelectedText + "！";
            }
        }
        #endregion

        #region Activity
        private void buttonActivityRefresh_Click(object sender, EventArgs e)
        {
            listViewActivity.Items.Clear();
            foreach (var a in myService.FindAllActivities("", "Name", true, 0, 0))
            {
                ListViewItem item = new ListViewItem(a.Name);
                ListViewItem.ListViewSubItem subItem1 = new ListViewItem.ListViewSubItem(item, a.OrganizationName);
                ListViewItem.ListViewSubItem subItem2 = new ListViewItem.ListViewSubItem(item, a.Status.ToString());
                ListViewItem.ListViewSubItem subItem3 = new ListViewItem.ListViewSubItem(item, a.VolunteerViewedTime.ToString());
                ListViewItem.ListViewSubItem subItem4 = new ListViewItem.ListViewSubItem(item, a.VolunteerFavoritedTime.ToString());
                item.SubItems.Add(subItem1);
                item.SubItems.Add(subItem2);
                item.SubItems.Add(subItem3);
                item.SubItems.Add(subItem4);
                item.Tag = a.Id;
                listViewActivity.Items.Add(item);
            }
            labelActivityNumber.Text = listViewActivity.Items.Count.ToString();
            toolStripStatusLabel2.Text = "已更新！";
        }

        private void buttonRemoveActivity_Click(object sender, EventArgs e)
        {
            bool success = true;
            if (listViewActivity.SelectedItems.Count > 0)
            {
                success = myService.DeleteActivity((Guid)listViewActivity.SelectedItems[0].Tag);
            }
            if (success == true)
            {
                //跟新状态栏
                toolStripStatusLabel2.Text = listViewActivity.SelectedItems[0].Text + "已删除！";
                listViewActivity.Items.Remove(listViewActivity.SelectedItems[0]);
                labelActivityNumber.Text = (int.Parse(labelActivityNumber.Text) - 1).ToString();
            }
        }
        #endregion

        
    }
}