using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Diagnostics;
using System.IO;
using RedisStudy;
using ServiceStack.Redis;
using ServiceStack.Redis.Support;
using System.Threading;
using ServiceStack.Redis.Generic;
using ServiceStack.Text;

namespace Pic
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        private static void WriteLogErr(string myErrEx, int type)
        {
            //depth所请求的堆栈帧的索引，在获得所调用的方法0，获得此方法的方法1，获得调用此方法的方法的方法2。。。类推
            //MethodInfo method = (MethodInfo)MethodBase.GetCurrentMethod();
            try
            {
                StackTrace st = new StackTrace();
                string methodName = st.GetFrame(1).GetMethod().Name;
                string className = st.GetFrame(1).GetMethod().DeclaringType.ToString();
                if (type == 0)
                {
                    FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + @"LogErr.txt", FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter m_streamWriter = new StreamWriter(fs);
                    m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    m_streamWriter.WriteLine("");
                    m_streamWriter.WriteLine("类名：" + className + "; 方法：" + methodName + "; 时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss[.fff] ") + " 错误信息：" + myErrEx);
                    m_streamWriter.Flush();
                    m_streamWriter.Close();
                    fs.Close();
                }
                else if (type == 1)
                {
                    FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + @"LogErr1.txt", FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter m_streamWriter = new StreamWriter(fs);
                    m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    m_streamWriter.WriteLine("");
                    m_streamWriter.WriteLine("类名：" + className + "; 方法：" + methodName + "; 时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss[.fff] ") + " 错误信息：" + myErrEx);
                    m_streamWriter.Flush();
                    m_streamWriter.Close();
                    fs.Close();
                }
                else
                {
                    FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + @"Log.txt", FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter m_streamWriter = new StreamWriter(fs);
                    m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    m_streamWriter.WriteLine("");
                    m_streamWriter.WriteLine("类名：" + className + "; 方法：" + methodName + "; 时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss[.fff] ") + " 错误信息：" + myErrEx);
                    m_streamWriter.Flush();
                    m_streamWriter.Close();
                    fs.Close();
                }

            }
            catch
            {

            }
        }

        #region redis
        protected void Button6_Click(object sender, EventArgs e)
        {
            Label4.Text = "";
            IRedisClient Redis = RedisManager.GetClient();
            //将字符串列表添加到redis  
            List<string> storeMembers = new List<string>() { "tom1", "tom2", "tom3" };
            storeMembers.ForEach(x => Redis.AddItemToList("additemtolist", x));
            //得到指定的key所对应的value集合  
            List<string> members = Redis.GetAllItemsFromList("additemtolist");
            members.ForEach(s => Label4.Text += (s + "/n"));
        }

        private class man
        {
            public string name { set; get; }
            public int age { set; get; }
        }

        protected void Button6_Click1(object sender, EventArgs e)
        {
            //对象处理
            IRedisClient redisclient = RedisManager.GetClient();
            IRedisTypedClient<man> redis = redisclient.As<man>();
            var currentShippers = redis.Lists["urn:man:current"];
            redis.RemoveAllFromList(currentShippers);//移除对象数据
            currentShippers.Add(new man{name = "Trains R Us",age = 1});
            currentShippers.Add(new man{name = "TrainsUSA",age = 2});
            currentShippers.Add(new man{name = "TrainsUK",age = 3});
            Label5.Text = "";
            string text = TypeSerializer.SerializeToString(currentShippers);
            //make it a little easier on the eyes
            var prettyLines = text.Split(new[] { "[", "},{", "]" },StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(x => x.Replace("{", "").Replace("}", ""));
            foreach (var l in prettyLines) {
                Label5.Text += l + "<br />";
            };
        }
        protected void Button7_Click(object sender, EventArgs e)
        {
            HashOperator hashredits = new HashOperator();
            hashredits.Set<man>("mylist", "1", new man() { name = "fc1", age = 1 });
            hashredits.Set<man>("mylist", "2", new man() { name = "fc2", age = 2 });
            hashredits.Set<man>("mylist", "3", new man() { name = "fc3", age = 3 });
            hashredits.Set<man>("mylist", "4", new man() { name = "fc4", age = 4 });
            hashredits.Set<man>("mylist", "5", new man() { name = "fc5", age = 5 });
            List<man> mylist = hashredits.GetAll<man>("mylist");
            
            Label5.Text = mylist.ToJson();
            
        }
        static void Dump<T>(string message, T entity)
        {
            var text = TypeSerializer.SerializeToString(entity);
            //make it a little easier on the eyes
            var prettyLines = text.Split(new[] { "[", "},{", "]" },
                StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(x => x.Replace("{", "").Replace("}", ""));

            Debug.WriteLine("\n" + message);
            foreach (var l in prettyLines) Debug.WriteLine(l);
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            IRedisClient Redis = RedisManager.GetClient();
            HashOperator operators = new HashOperator();
            // 获取指定索引位置数据  
            //Console.WriteLine("获取指定索引位置数据：");
            //var item = Redis.GetItemFromList("additemtolist", 2);

            //将数据存入Hash表中  
            //Console.WriteLine("Hash表数据存储:");
            //UserInfo userInfos = new UserInfo() { UserName = "李雷", Age = 45 };
            //var ser = new ObjectSerializer();    //位于namespace ServiceStack.Redis.Support;  
            //bool results = operators.Set<byte[]>("userInfosHash", "userInfos", ser.Serialize(userInfos));
            //byte[] infos = operators.Get<byte[]>("userInfosHash", "userInfos");
            //userInfos = ser.Deserialize(infos) as UserInfo;
            //Console.WriteLine("name=" + userInfos.UserName + "   age=" + userInfos.Age);


            //object序列化方式存储  
            //Console.WriteLine("object序列化方式存储:");
            //UserInfo uInfo = new UserInfo() { UserName = "张三", Age = 12 };
            //bool result = Redis.Set<byte[]>("uInfo", ser.Serialize(uInfo));
            //UserInfo userinfo2 = ser.Deserialize(Redis.Get<byte[]>("uInfo")) as UserInfo;
            //Console.WriteLine("name=" + userinfo2.UserName + "   age=" + userinfo2.Age);


            //存储值类型数据  
            //Console.WriteLine("存储值类型数据:");
            //Redis.Set<int>("my_age", 12);//或Redis.Set("my_age", 12);  
            //int age = Redis.Get<int>("my_age");
            //Console.WriteLine("age=" + age);


            //序列化列表数据  
            //Console.WriteLine("列表数据:");
            //List<UserInfo> userinfoList = new List<UserInfo> {  
            //    new UserInfo{UserName="露西",Age=1,Id=1},  
            //    new UserInfo{UserName="玛丽",Age=3,Id=2},  
            //};
            //Redis.Set<byte[]>("userinfolist_serialize", ser.Serialize(userinfoList));
            //List<UserInfo> userList = ser.Deserialize(Redis.Get<byte[]>("userinfolist_serialize")) as List<UserInfo>;
            //userList.ForEach(i =>
            //{
            //    Console.WriteLine("name=" + i.UserName + "   age=" + i.Age);
            //});  
        }
        protected void btn_redis_remove_Click(object sender, EventArgs e)
        {
            IRedisClient Redis = RedisManager.GetClient();
            //移除某个缓存数据  
            bool isTrue = Redis.Remove("additemtolist");
            Label4.Text = "Item value is: " + Redis.Get<string>("additemtolist");
            Redis.Dispose();
        }

        private class tomman
        {
            public string Sessionid { set; get; }
            public string User { set; get; }
            public int UserID { set; get; }
            public int UID { set; get; }
            public int UserType { set; get; }
            public string LoginName { set; get; }
        }
       
        protected void Button8_Click(object sender, EventArgs e)
        {
            //登入
            HttpContext.Current.Session["User"] = "tom";
            HttpContext.Current.Session["UserID"] = 1;
            HttpContext.Current.Session["UID"] = 0;
            HttpContext.Current.Session["UserType"] = 0;
            HttpContext.Current.Session["LoginName"] = "tom";
            Label6.Text = HttpContext.Current.Session.SessionID;
        }

        protected void Button9_Click(object sender, EventArgs e)
        {
            //取消会话
            //HttpContext.Current.Session.Timeout = 0;
            HttpContext.Current.Session.Abandon();
            //HashOperator hashredits = new HashOperator();
            //hashredits.Remove("mysession", HttpContext.Current.Session.SessionID);
        }

        protected void Button10_Click(object sender, EventArgs e)
        {
            //获取Session个数
            Label7.Text = HttpContext.Current.Session.Count.ToString();
        }

        protected void Button11_Click(object sender, EventArgs e)
        {
            //获取Session个数
            HttpContext.Current.Session.RemoveAll();
            Label7.Text = HttpContext.Current.Session.Count.ToString();
        }

        protected void Button12_Click(object sender, EventArgs e)
        {
            //获取当前SessionID
            Label8.Text = HttpContext.Current.Session.SessionID;
        }
        #endregion

        
    }
}