using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Repository.Interface;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.VolunteerEvent;
using System.Threading;
using Jtext103.Repository;
using System.IO;
using System.Reflection;


namespace Jtext103.Volunteer.Service
{
    public class EventService
    {
        private static IRepository<Event> eventRepository;
        private static IRepository<Subscriber> subscriberRepository;
        private static Queue<Guid> eventIdQueue;
        //线程处理每个event之间的sleep时间
        private static int sleepTimeBetweenEvents;
        //线程处理每次检查eventIdQueue之间的sleep时间
        private static int sleepTimeBetweenQueues;
        private static Thread CheckEventIdQueueThread;
        public static void InitService(IRepository<Event> eventRepository, IRepository<Subscriber> subscriberRepository, int sleepTimeBetweenEvents, int sleepTimeBetweenQueues)
        {
            EventService.eventRepository = eventRepository;
            EventService.subscriberRepository = subscriberRepository;
            EventService.sleepTimeBetweenEvents = sleepTimeBetweenEvents;
            EventService.sleepTimeBetweenQueues = sleepTimeBetweenQueues;
            eventIdQueue = new Queue<Guid>();
            CheckEventIdQueueThread = new Thread(EventService.CheckTheEventIdQueue);
            CheckEventIdQueueThread.Start();
        }
        
        /// <summary>
        /// 发布一个event
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventValue"></param>
        /// <param name="eventSender"></param>
        public static void Publish(string eventType, string eventValue, object eventSender)
        {
            Event newEvent = new Event(eventType, eventValue, eventSender);
            eventRepository.SaveOne(newEvent);
            eventIdQueue.Enqueue(newEvent.Id);
        }

        /// <summary>
        /// 添加一个subscriber
        /// </summary>
        /// <param name="name"></param>
        /// <param name="triggerEventTypes"></param>
        /// <param name="triggerSender"></param>
        /// <param name="handlerName"></param>
        public static void Subscribe(string name, List<string> triggerEventTypes, object triggerSender, string handlerName)
        {
            Subscriber newSubscriber = new Subscriber(name, triggerEventTypes, triggerSender, handlerName);
            subscriberRepository.SaveOne(newSubscriber);
        }

        /// <summary>
        /// 检查eventQueue，如果不为空则处理event
        /// </summary>
        public static void CheckTheEventIdQueue()
        {
            while (true)
            {
                while (eventIdQueue.Any())
                {
                    Guid eventId = eventIdQueue.Dequeue();
                    //对每个event进行处理
                    Event oneEvent = eventRepository.FindOneById(eventId);
                    //subscriber.TriggerSender == oneEvent.Sender || subscriber.TriggerSender == null
                    QueryObject<Subscriber> subQueryObject = new QueryObject<Subscriber>(subscriberRepository);
                    Dictionary<string, object> queryDict1 = new Dictionary<string, object>();
                    Dictionary<string, object> queryDict2 = new Dictionary<string, object>();
                    queryDict1.Add("TriggerSender", null);
                    queryDict2.Add("TriggerSender", oneEvent.Sender);
                    subQueryObject.AppendQuery(queryDict1, QueryLogic.And);
                    subQueryObject.AppendQuery(queryDict2, QueryLogic.Or);
                    //subscriber.TriggerEventTypes包含oneEvent.Type
                    QueryObject<Subscriber> queryObject = new QueryObject<Subscriber>(subscriberRepository);
                    Dictionary<string, object> queryDict = new Dictionary<string, object>();
                    queryDict.Add("TriggerEventTypes", oneEvent.Type);
                    queryObject.AppendQuery(queryDict, QueryLogic.And);
                    //以上二者结合
                    queryObject.AppendQuery(subQueryObject, QueryLogic.And);
                    IEnumerable<Subscriber> subscribers = subscriberRepository.Find(queryObject);
                    foreach (Subscriber subscriber in subscribers)
                    {
                        //handle the event

                    }

                    //标记为已处理
                    oneEvent.hasHandled = true;
                    eventRepository.SaveOne(oneEvent);

                    Thread.Sleep(sleepTimeBetweenEvents);
                }
                Thread.Sleep(sleepTimeBetweenQueues);
            }
        }

        /// <summary>
        /// 找到path下dll中实现IVolunteerEventHandler的handler类
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<IVolunteerEventHandler> FindAllHandler(string path)
        {
            List<IVolunteerEventHandler> result = new List<IVolunteerEventHandler>();
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                string ext = file.Substring(file.LastIndexOf("."));
                if (ext != ".dll")
                {
                    continue;
                }
                Assembly dll = Assembly.LoadFile(file);
                Type[] types = dll.GetTypes();
                foreach (Type type in types)
                {
                    //当type实现了IVolunteerEventHandler接口
                    if (typeof(IVolunteerEventHandler).IsAssignableFrom(type))
                    {
                        result.Add((IVolunteerEventHandler)Activator.CreateInstance(type));
                    }
                }
            }
            return result; 
        }
    }
}
