using System;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using System.Runtime.InteropServices; // This is to unzip the files downloaded from Dropbox

namespace GMODit
{
    public partial class Form1 : Form
    {
        
        #region Mouse Capture Code

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        #endregion

        public Form1()
        {
            InitializeComponent();
            this.password.KeyUp += new KeyEventHandler(password_KeyUp); //necessary for Enter key capture.
            this.MouseDown += new MouseEventHandler(Form1_MouseDown); //necessary for mouse capture.

            if (Directory.Exists(@"C:\GMODit\.minecraft") == false)
            {
                Directory.CreateDirectory(@"C:\GMODit\.minecraft");
            }

            if (File.Exists(@"C:\GMODit\.minecraft\gmoditlastlogin") == false)
            {
                StreamWriter URaDummyJoel = new StreamWriter(@"C:\GMODit\.minecraft\gmoditlastlogin"); // This creates a blank file
                URaDummyJoel.Close(); //I'm a dummy because the lack of this section would cause a crash on the first run. Every. Time.
            }

            StreamReader loginInfo = new StreamReader(@"C:\GMODit\.minecraft\gmoditlastlogin");

            string username;

            if ((username = loginInfo.ReadLine()) != null)
            {
                this.rememberMeBox.Checked = true;
            }
            string password = loginInfo.ReadLine();

            //decode password string using decryption algorithm that I haven't written yet... :'(

            this.username.Text = username;
            this.password.Text = password;

            loginInfo.Close();

            
        }

        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (rememberMeBox.Checked == true)
            {
                StreamWriter logininfo = new StreamWriter(@"C:\GMODit\.minecraft\gmoditlastlogin");
                logininfo.WriteLine(this.username.Text);
                logininfo.WriteLine(this.password.Text);

                logininfo.Close();
            }
            else
            {
                StreamWriter logininfo = new StreamWriter(@"C:\GMODit\.minecraft\gmoditlastlogin"); // creates blank file; overwrites old file
                logininfo.Close();
            }

            bool ismodded = false;
            bool clientExists = false;
            string folderlocation = @"C:\GMODIT\.minecraft";
            string modlocation = folderlocation + @"\mods";
            string coremodlocation = folderlocation + @"\coremods";
            string templocation = "c:\\temp\\";

            if (Directory.Exists(folderlocation))
            {
                clientExists = true;
            }

            if (!Directory.Exists(templocation))
            {
                Directory.CreateDirectory(templocation);
            }

            if (File.Exists(folderlocation + "\\version3.modit"))
            {
                ismodded = true;
            }
            else
            {
                //download and extract the three files

                if (InternetCheck.Checked == true) //Secret features being planned!
                {
                    WebClient downloader = new WebClient();
                    label4.Text = "Downloading client";
                    this.Update();
                    downloader.DownloadFile("<link to file to download>", templocation + "client.zip");
                    label4.Text = "Downloading Mods";
                    this.Update();
                    downloader.DownloadFile("<link to file to download>", templocation + "mods.zip");
                    label4.Text = "Downloading Coremods";
                    this.Update();
                    downloader.DownloadFile("<link to file to download>", templocation + "coremods.zip");

                    label4.Text = "Extracting...";
                    Shell32.Shell sc = new Shell32.Shell();
                    Directory.CreateDirectory(@"C:\temp\.minecraft");
                    Shell32.Folder output = sc.NameSpace(@"C:\temp\.minecraft\");
                    Shell32.Folder input = sc.NameSpace(templocation + "client.zip");
                    output.CopyHere(input.Items(), 256);

                    Directory.CreateDirectory(@"C:\temp\.minecraft\mods");
                    output = sc.NameSpace(@"C:\temp\.minecraft\mods");
                    input = sc.NameSpace(templocation + "mods.zip");
                    output.CopyHere(input.Items(), 256);

                    Directory.CreateDirectory(@"C:\temp\.minecraft\coremods");
                    output = sc.NameSpace(@"C:\temp\.minecraft\coremods");
                    input = sc.NameSpace(templocation + "coremods.zip");
                    output.CopyHere(input.Items(), 256);

                    label4.Text = "Cleaning up...";
                    this.Update();
                    File.Delete(templocation + "client.zip");
                    File.Delete(templocation + "mods.zip");
                    File.Delete(templocation + "coremods.zip");
                }
                else
                {
                    WebClient downloader = new WebClient();
                    label4.Text = "Downloading full client. May take a while";
                    this.Update();
                    downloader.DownloadFile("<link to file to download>", templocation + "client_full.zip");

                    label4.Text = "Extracting...";
                    Shell32.Shell sc = new Shell32.Shell();
                    Directory.CreateDirectory(@"C:\temp\.minecraft");
                    Shell32.Folder output = sc.NameSpace(@"C:\temp\.minecraft\");
                    Shell32.Folder input = sc.NameSpace(templocation + "client_full.zip");
                    output.CopyHere(input.Items(), 256);

                    label4.Text = "Cleaning up...";
                    File.Delete(templocation + "client_full.zip");
                }

                new Microsoft.VisualBasic.Devices.Computer().FileSystem.CopyDirectory("C:\\temp\\.minecraft", folderlocation, true);

                Directory.Delete("C:\\temp\\.minecraft", true);

                clientExists = true;
                ismodded = true;
            }

            if (clientExists && ismodded)
            {
                bool authed = false;
                string user, session = "";
                string[] res = verifyUser(username.Text, password.Text, out authed);
                if (authed)
                {
                    user = res[2];
                    session = res[3];
                    StartMinecraft(user, session);
                    Application.Exit();
                }
            }
        }

        private string[] verifyUser(string username, string password, out bool authed)
        {
            HttpWebResponse response;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://login.minecraft.net/?user=" + username + "&password=" + password + "&version=13");
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                authed = false;
                return null;
            }
            string str = new StreamReader(response.GetResponseStream()).ReadToEnd();
            if (str.Contains(":") == false)
            {
                authed = false;
                this.label4.Text = str;
                return null;
            }
            authed = true;
            return str.Split(new char[] { ':' });
        }

        private void StartMinecraft(string username, string session)
        {
            Environment.SetEnvironmentVariable("APPDATA", "C:\\GMODIT\\");

            Process process = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            string dir = @"C:\GMODIT\.minecraft\bin\";
            info.FileName = "javaw";
            info.CreateNoWindow = true;
            info.Arguments = "-cp \"" + dir + "minecraft.jar;" + dir + "lwjgl.jar;" + dir + "lwjgl_util.jar;" + dir + "jinput.jar;\" ";
            info.Arguments += "\"-Djava.library.path=" + dir + "natives\" -Xmx1024M -Xms512M net.minecraft.client.Minecraft " + username + " " + session;
            process.StartInfo = info;
            process.Start();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void password_TextChanged(object sender, EventArgs e)
        {
            

            if (password.Text == "")
            {
                button1.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
            }
        }

        private void password_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button1_Click(null, null);
        }
    }
}
