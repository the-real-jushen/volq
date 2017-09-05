using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Reflection;
using Jtext103.Repository;
using Jtext103.Repository.Interface;

namespace Jtext103.Volunteer.VolunteerEvent
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
        private static Thread checkEventIdQueueThread;
        public static List<IVolunteerEventHandler> EventHandlerList;


        public static void InitService(IRepository<Event> eventRepository, IRepository<Subscriber> subscriberRepository, int sleepTimeBetweenEvents, int sleepTimeBetweenQueues, string eventHandlerPath)
        {
            EventService.eventRepository = eventRepository;
            EventService.subscriberRepository = subscriberRepository;
            EventService.sleepTimeBetweenEvents = sleepTimeBetweenEvents;
            EventService.sleepTimeBetweenQueues = sleepTimeBetweenQueues;
            eventIdQueue = new Queue<Guid>();
            //先将所有未handle的event添加到queue中
            addNotHandledEventToQueue();
            EventHandlerList = FindAllHandler(eventHandlerPath);
            //先清空所有的subscriber，再添加
            subscriberRepository.RemoveAll();
            registerSubscribrer();
            //用来检查event队列的线程
            checkEventIdQueueThread = new Thread(EventService.CheckTheEventIdQueue);
            checkEventIdQueueThread.Start();
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

        public static void SaveEvent(Event oneEvent)
        {
            eventRepository.SaveOne(oneEvent);
        }

        /// <summary>
        /// 添加一个subscriber
        /// </summary>
        /// <param name="name"></param>
        /// <param name="triggerEventTypes"></param>
        /// <param name="triggerSender"></param>
        /// <param name="handlerName"></param>
        public static void Subscribe(string name, List<string> triggerEventTypes, List<string> handlerNames, object triggerSender)
        {
            Subscriber newSubscriber = new Subscriber(name, triggerEventTypes, handlerNames, triggerSender);
            subscriberRepository.SaveOne(newSubscriber);
        }

        public static void Subscribe(Subscriber newSubscriber)
        {
            
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
                        //每个subscriber调用handler处理event
                        foreach (IVolunteerEventHandler eventHandler in EventHandlerList)
                        {
                            if (subscriber.HandlerNames.Contains(eventHandler.Name))
                            {
                                eventHandler.HandleEvent(oneEvent);
                            }
                        }
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
        private static List<IVolunteerEventHandler> FindAllHandler(string path)
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

        /// <summary>
        /// 向数据库中写入每个handler需要的subscriber
        /// </summary>
        private static void registerSubscribrer()
        {
            foreach (var handler in EventHandlerList)
            {
                var subscribers = handler.GetHandlerSubscribers();
                {
                    foreach (var subscriber in subscribers)
                    {
                        //save in the subscriber repository
                        subscriberRepository.SaveOne(subscriber);
                    }
                }
            }
        }

        /// <summary>
        /// 将所有未handle过的event放到queue的结尾处
        /// 程序异常停止，重新启动时容错
        /// </summary>
        private static void addNotHandledEventToQueue()
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("hasHandled", false);
            QueryObject<Event> queryObject = new QueryObject<Event>(eventRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Event> notHandledEvents = eventRepository.Find(queryObject);
            if (notHandledEvents.Any())
            {
                foreach (Event notHandledEvent in notHandledEvents)
                {
                    eventIdQueue.Enqueue(notHandledEvent.Id);
                }
            }
        }

    }
}
