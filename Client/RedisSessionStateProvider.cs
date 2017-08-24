using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Web.SessionState;
using ServiceStack.Redis;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;
using System.IO;

namespace RedisProvider.SessionProvider
{

    #region Session Item Model  Redis里保存的SessionModel
    public class SessionItem
    {
        #region Properties
        public DateTime CreatedAt { get; set; }
        public DateTime LockDate { get; set; }
        public int LockID { get; set; }
        public int Timeout { get; set; }
        public bool Locked { get; set; }
        public string SessionItems { get; set; }
        public int Flags { get; set; }
        #endregion Properties

    }
    #endregion Session Item Model

    public class CustomServiceProvider : System.Web.SessionState.SessionStateStoreProviderBase, IDisposable
    {

        #region Redis Properties
        private string ApplicationName
        {
            get
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains("Application.Name"))
                {
                    return ConfigurationManager.AppSettings["Application.Name"];
                }
                return string.Empty;
            }
        }

        //得到Redis连接
        private RedisClient RedisSessionClient
        {
            get
            {
                if (!string.IsNullOrEmpty(this.redisPassword))
                {
                    return new RedisClient(this.redisServer, this.redisPort, this.redisPassword, redisDB);
                }
                return new RedisClient(this.redisServer, this.redisPort, null, redisDB);
            }
        }
        private static System.Configuration.AppSettingsReader AppSettingR = new System.Configuration.AppSettingsReader();
        private string redisServer = AppSettingR.GetValue("RedisServePath", typeof(String)).ToString();
        private int redisPort = Convert.ToInt16(AppSettingR.GetValue("RedisServeHost", typeof(Int16)));
        private string redisPassword = string.Empty;
        private long redisDB = Convert.ToInt16(AppSettingR.GetValue("RedisServeDB", typeof(Int16)));
        private SessionStateSection sessionStateConfig = null;
        private bool writeExceptionsToLog = false;
        #endregion Properties

        #region Private Methods
        /// <summary>
        /// Prepends the application name to the redis key if one exists. Querying by application name is recommended for session
        /// </summary>
        /// <param name="id">The session id</param>
        /// <returns>Concatenated string applicationname:sessionkey</returns>
        private string RedisKey(string id)
        {
            return string.Format("{0}{1}", !string.IsNullOrEmpty(this.ApplicationName) ? this.ApplicationName + ":" : "", id);
        }
        #endregion Private Methods

        #region Constructor
        public CustomServiceProvider()
        {

        }
        #endregion Constructor

        #region Overrides
        public override void Dispose()
        {

        }

        /// <summary>
        /// 不需要为继承 SessionStateStoreProviderBase 抽象类的类(ProviderBase抽象类)实现构造函数。 SessionStateStoreProviderBase 实现的初始化值传递给 Initialize 方法实现。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config">名称/值对的集合，表示在配置中为该提供程序指定的、提供程序特定的特性。</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config.
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (name == null || name.Length == 0)
            {
                name = "RedisSessionStateStore";
            }
            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Redis Session State Provider");
            }
            // Initialize the abstract base class.
            base.Initialize(name, config);

            // Get <sessionState> configuration element.
            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(ApplicationName);
            sessionStateConfig = (SessionStateSection)cfg.GetSection("system.web/sessionState");

            if (config["writeExceptionsToEventLog"] != null)
            {
                if (config["writeExceptionsToEventLog"].ToUpper() == "TRUE")
                    this.writeExceptionsToLog = true;
            }
            if (config["server"] != null)
            {
                this.redisServer = config["server"];
            }
            if (config["port"] != null)
            {
                int.TryParse(config["port"], out this.redisPort);
            }
            if (config["password"] != null)
            {
                this.redisPassword = config["password"];
            }
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return true;
        }
        #region 修改会话状态
        //如果修改了会话状态值，则SessionStateModule实例调用 SessionStateStoreProviderBase.SetAndReleaseItemExclusive 方法将更新的值写入会话状态存储区
        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            using (RedisClient client = this.RedisSessionClient)
            {
                // Serialize the SessionStateItemCollection as a string.
                string sessionItems = Serialize((SessionStateItemCollection)item.Items);

                try
                {
                    if (newItem)
                    {
                        SessionItem sessionItem = new SessionItem();
                        sessionItem.CreatedAt = DateTime.UtcNow;
                        sessionItem.LockDate = DateTime.UtcNow;
                        sessionItem.LockID = 0;
                        sessionItem.Timeout = item.Timeout;
                        sessionItem.Locked = false;
                        sessionItem.SessionItems = sessionItems;
                        sessionItem.Flags = 0;

                        client.Set<SessionItem>(this.RedisKey(id), sessionItem, DateTime.UtcNow.AddMinutes(item.Timeout));
                    }
                    else
                    {
                        SessionItem currentSessionItem = client.Get<SessionItem>(this.RedisKey(id));
                        if (currentSessionItem != null && currentSessionItem.LockID == (int?)lockId)
                        {
                            currentSessionItem.Locked = false;
                            currentSessionItem.SessionItems = sessionItems;
                            client.Set<SessionItem>(this.RedisKey(id), currentSessionItem, DateTime.UtcNow.AddMinutes(item.Timeout));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion

        #region 获取会话状态
        //会话状态由 SessionStateModule 类进行管理，在请求过程中的不同时间，该类调用会话状态存储提供程序在数据存储区中读写会话数据。
        //请求开始时，SessionStateModule 实例通过调用 GetItemExclusive 方法或 GetItem 方法（如果 EnableSessionState 页属性已设置为 ReadOnly）从数据源检索数据。
        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return GetSessionStoreItem(true, context, id, out locked, out lockAge, out lockId, out actions);
        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            return GetSessionStoreItem(false, context, id, out locked, out lockAge, out lockId, out actionFlags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lockRecord"></param>
        /// <param name="context">当前请求的 HttpContext</param>
        /// <param name="id">当前请求的 SessionID</param>
        /// <param name="locked">如果成功获得锁定，设置为 true 的布尔值</param>
        /// <param name="lockAge">一个设置为会话数据存储区中的项锁定时间的 TimeSpan 对象</param>
        /// <param name="lockId">一个设置为当前请求的锁定标识符的对象</param>
        /// <param name="actionFlags">包含 SessionStateActions 值之一，指示当前会话是否为未初始化的无 Cookie 会话</param>
        /// <returns></returns>
        private SessionStateStoreData GetSessionStoreItem(bool lockRecord, HttpContext context,string id,out bool locked,out TimeSpan lockAge,out object lockId,out SessionStateActions actionFlags)
        {
            // Initial values for return value and out parameters.
            SessionStateStoreData item = null;
            lockAge = TimeSpan.Zero;
            lockId = null;
            locked = false;
            actionFlags = 0;

            // String to hold serialized SessionStateItemCollection.
            string serializedItems = "";

            // Timeout value from the data store.
            int timeout = 0;

            using (RedisClient client = this.RedisSessionClient)
            {
                try
                {
                    if (lockRecord)
                    {
                        locked = false;
                        SessionItem currentItem = client.Get<SessionItem>(this.RedisKey(id));

                        if (currentItem != null)
                        {
                            // If the item is locked then do not attempt to update it
                            if (!currentItem.Locked)
                            {
                                currentItem.Locked = true;
                                currentItem.LockDate = DateTime.UtcNow;
                                client.Set<SessionItem>(this.RedisKey(id), currentItem, DateTime.UtcNow.AddMinutes(sessionStateConfig.Timeout.TotalMinutes));
                            }
                            else
                            {
                                locked = true;
                            }
                        }
                    }

                    SessionItem currentSessionItem = client.Get<SessionItem>(this.RedisKey(id));

                    if (currentSessionItem != null)
                    {
                        serializedItems = currentSessionItem.SessionItems;
                        lockId = currentSessionItem.LockID;
                        lockAge = DateTime.UtcNow.Subtract(currentSessionItem.LockDate);
                        actionFlags = (SessionStateActions)currentSessionItem.Flags;
                        timeout = currentSessionItem.Timeout;
                    }
                    else
                    {
                        locked = false;
                    }

                    if (currentSessionItem != null && !locked)
                    {
                        // Delete the old item before inserting the new one
                        client.Remove(this.RedisKey(id));

                        lockId = (int?)lockId + 1;
                        currentSessionItem.LockID = lockId != null ? (int)lockId : 0;
                        currentSessionItem.Flags = 0;

                        client.Set<SessionItem>(this.RedisKey(id), currentSessionItem, DateTime.UtcNow.AddMinutes(sessionStateConfig.Timeout.TotalMinutes));

                        // If the actionFlags parameter is not InitializeItem,
                        // deserialize the stored SessionStateItemCollection.
                        if (actionFlags == SessionStateActions.InitializeItem)
                        {
                            item = CreateNewStoreData(context, 30);
                        }
                        else
                        {
                            item = Deserialize(context, serializedItems, timeout);
                        }
                    }
                }

                catch (Exception e)
                {
                    throw e;
                }
            }

            return item;
        }
        #endregion

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {

            using (RedisClient client = this.RedisSessionClient)
            {
                SessionItem currentSessionItem = client.Get<SessionItem>(this.RedisKey(id));

                if (currentSessionItem != null && (int?)lockId == currentSessionItem.LockID)
                {
                    currentSessionItem.Locked = false;
                    client.Set<SessionItem>(this.RedisKey(id), currentSessionItem, DateTime.UtcNow.AddMinutes(sessionStateConfig.Timeout.TotalMinutes));
                }
            }
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            using (RedisClient client = this.RedisSessionClient)
            {
                // Delete the old item before inserting the new one
                client.Remove(this.RedisKey(id));
            }
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            using (RedisClient client = this.RedisSessionClient)
            {
                SessionItem sessionItem = new SessionItem();
                sessionItem.CreatedAt = DateTime.Now.ToUniversalTime();
                sessionItem.LockDate = DateTime.Now.ToUniversalTime();
                sessionItem.LockID = 0;
                sessionItem.Timeout = timeout;
                sessionItem.Locked = false;
                sessionItem.SessionItems = string.Empty;
                sessionItem.Flags = 0;

                client.Set<SessionItem>(this.RedisKey(id), sessionItem, DateTime.UtcNow.AddMinutes(timeout));
            }
        }

        public override SessionStateStoreData CreateNewStoreData(System.Web.HttpContext context, int timeout)
        {
            return new SessionStateStoreData(new SessionStateItemCollection(),
                SessionStateUtility.GetSessionStaticObjects(context),
                timeout);
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {

            using (RedisClient client = this.RedisSessionClient)
            {
                try
                {
                    // TODO :: GET THIS VALUE FROM THE CONFIG
                    client.ExpireEntryAt(id, DateTime.UtcNow.AddMinutes(sessionStateConfig.Timeout.TotalMinutes));
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        //执行会话状态存储提供程序必需的所有初始化操作。
        public override void InitializeRequest(HttpContext context)
        {
            //SessionStateModule 对象在调用任何其他 SessionStateStoreProviderBase 方法前，首先调用 InitializeRequest 方法。 可以使用 InitializeRequest 方法执行会话状态存储提供程序要求的任何每次请求初始化。
            // Was going to open the redis connection here but sometimes I had 5 connections open at one time which was strange
        }
        //执行会话状态存储提供程序必需的所有清理操作。
        public override void EndRequest(HttpContext context)
        {
            //SessionStateModule 对象在对 ASP.NET 页的请求结束时调用 EndRequest 方法。 可以使用 EndRequest 方法执行会话状态存储提供程序要求的任何每次请求清理。
            this.Dispose();
        }
        #endregion Overrides

        #region Serialization
        /// <summary>
        /// Serialize is called by the SetAndReleaseItemExclusive method to
        /// convert the SessionStateItemCollection into a Base64 string to
        /// be stored in MongoDB.
        /// </summary>
        private string Serialize(SessionStateItemCollection items)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                if (items != null)
                    items.Serialize(writer);

                writer.Close();

                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private SessionStateStoreData Deserialize(HttpContext context, string serializedItems, int timeout)
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(serializedItems)))
            {
                SessionStateItemCollection sessionItems = new SessionStateItemCollection();

                if (ms.Length > 0)
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        sessionItems = SessionStateItemCollection.Deserialize(reader);
                    }
                }

                return new SessionStateStoreData(sessionItems,
                  SessionStateUtility.GetSessionStaticObjects(context),
                  timeout);
            }
        }
        #endregion Serialization
    }
}