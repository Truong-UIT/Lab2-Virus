using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Lab2_4
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSSendMessage(
                                           IntPtr hServer,
                                           [MarshalAs(UnmanagedType.I4)] int SessionId,
                                           String pTitle,
                                           [MarshalAs(UnmanagedType.U4)] int TitleLength,
                                           String pMessage,
                                           [MarshalAs(UnmanagedType.U4)] int MessageLength,
                                           [MarshalAs(UnmanagedType.U4)] int Style,
                                           [MarshalAs(UnmanagedType.U4)] int Timeout,
                                           [MarshalAs(UnmanagedType.U4)] out int pResponse,
                                           bool bWait);

        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern int WTSGetActiveConsoleSessionID();

        public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
        public static int WTS_CURRENT_SESSION = 1;
        System.Timers.Timer timer = new System.Timers.Timer();
        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(Popup);
            timer.Interval = 5000; //number in milisecinds
            timer.Enabled = true;
        }

        protected override void OnStop()
        {

        }

        public void Popup(object source, ElapsedEventArgs e)
        {
            EventLog log = new EventLog();
            log.Log = "Security";

            foreach (EventLogEntry entry in log.Entries)
            {
                if (entry.EventID == 4624 || entry.EventID == 4672)
                {
                    if (DateTime.Now.Day == entry.TimeGenerated.Day && DateTime.Now.Month == entry.TimeGenerated.Month &&
                        DateTime.Now.Year == entry.TimeGenerated.Year && DateTime.Now.Hour == entry.TimeGenerated.Hour)
                    {
                        int timenow = DateTime.Now.Minute * 60 + DateTime.Now.Second;
                        int timeevent = entry.TimeGenerated.Minute * 60 + entry.TimeGenerated.Second;

                        if ((timenow - timeevent) < 9)
                        {
                            for (int user_session = 10; user_session > 0; user_session--)
                            {
                                Thread t = new Thread(() =>
                                {
                                    try
                                    {
                                        bool result = false;
                                        String title = "Lab2";
                                        int tlen = title.Length;
                                        String msg = "18520182 && 18521332";
                                        int mlen = msg.Length;
                                        int resp = 7;
                                        result = WTSSendMessage(WTS_CURRENT_SERVER_HANDLE, user_session, title, tlen, msg, mlen, 0, 0, out resp, true);

                                        int err = Marshal.GetLastWin32Error();
                                        if (err == 0)
                                        {
                                            if (result) //user responded to box
                                            {
                                                if (resp == 7) //user clicked no
                                                {

                                                }
                                                else if (resp == 6) //user clicked yes
                                                {

                                                }
                                                Debug.WriteLine("user_session:" + user_session + " err:" + err + " resp:" + resp);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("no such thread exists", ex);
                                    }
                                });
                                t.SetApartmentState(ApartmentState.STA);
                                t.Start();
                            }
                            break;
                        }
                    }
                }

            }
            Process DProcess = new Process();
            string command = "msg * hello";

            System.Diagnostics.Process.Start("CMD.exe", command);
        }
    }
}
