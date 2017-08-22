using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RedisStudy;
using ServiceStack.Redis;
using System.Diagnostics;

namespace WindowsFormsRedisApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static System.Configuration.AppSettingsReader AppSettingR = new System.Configuration.AppSettingsReader();
        private static string redisServer = AppSettingR.GetValue("RedisServePath",typeof(String)).ToString();
        private static int redisPort = Convert.ToInt16(AppSettingR.GetValue("RedisServeHost",typeof(Int16)));
        private static string redisPassword = string.Empty;
        private static long redisDB = Convert.ToInt16(AppSettingR.GetValue("RedisServeDB",typeof(Int16)));
        private static RedisClient client = new RedisClient(redisServer, redisPort, null, redisDB);
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
        private void button1_Click(object sender, EventArgs e)
        {
            //HashOperator hashredits = new HashOperator();
            GetRedisSession();
        }
        //打开应用程序立即运行
        private void SysServicesMainForm_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Interval = 1000;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GetRedisSession();
        }

        private void btnstart_Click(object sender, EventArgs e)
        {
            if (Process_offon("redis-server", 1))
            {
                timer1.Enabled = true;
            }
            else {
                timer1.Enabled = false;
            }
        }

        /// <summary>
        /// 处理进程
        /// </summary>
        /// <param name="processname">进程名</param>
        /// <param name="type">0:判断有没有启动  1:如果不存在就启动  2:如果存在就kill</param>
        /// <returns></returns>
        public bool Process_offon(string processname,int type) {
            bool result = false;
            Process[] processes = Process.GetProcessesByName(processname);
            if (processes.Length > 0)
            {
                //存在
                if (type.Equals(2))
                {
                    foreach (Process model in processes)
                    {
                        model.Kill();
                    }
                }
                else
                {
                    result = true;
                }
            }
            else {
                //不存在
                if (type.Equals(1))
                {
                    Process.Start("D:\\Learn\\redis-2.4.5-win32-win64\\64bit\\redis-server.exe");
                    result = true;
                }
            }
            return result;
        }

        private void GetRedisSession()
        {
            List<SessionItem> mylist = new List<SessionItem>();
            try
            {
                List<string> mySessionIds = client.GetAllKeys();
                foreach (string sessionid in mySessionIds)
                {
                    mylist.Add(client.Get<SessionItem>(sessionid));//mySessionIds[0]
                }
            }
            catch(RedisException ex) {
                timer1.Enabled = false;
                if (ex.Message.Contains("could not connect to redis Instance at")) {
                    MessageBox.Show("未启动Redis数据库！");
                }
            }
            this.dataGridView1.DataSource = mylist;
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

        
    }
}
