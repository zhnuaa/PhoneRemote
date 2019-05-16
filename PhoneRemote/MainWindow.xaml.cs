using StyxFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhoneRemote
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppState state;
        private bool cameraOn;
        private double screencapScale;
        public MainWindow()
        {
            InitializeComponent();
            state = new AppState();
            _GridRoot.DataContext = state;
            cameraOn = false;
            screencapScale = 1;
            state.IP = "192.168.1.110";
        }             

        private async void _BTNConnect_Click(object sender, RoutedEventArgs e)
        {
            CmdBuilder cmd = new CmdBuilder()
            {
                Command = "adb"
            };
            cmd.UpdateInputArgs("-a", "-a");
            cmd.UpdateInputArgs("Connect", "connect");
            cmd.UpdateInput("IP", state.IP);
            Debug.WriteLine(cmd.GetFullCmdString());
            var excutor = new CommandExcutor(cmd);
            var stdout = await excutor.AsyncExcute(5000);
            state.UpdateCmdStdOut(stdout["StdOut"]);
            Debug.WriteLine(cmd.GetFullCmdString());
            cmd = new CmdBuilder()
            {
                Command = "adb"
            };
            cmd.UpdateInputArgs("Devices", "devices");
            excutor = new CommandExcutor(cmd);
            stdout = await excutor.AsyncExcute(5000);
            state.UpdateCmdStdOut(stdout["StdOut"]);
            state.UpdateCmdStdOut(@"Waiting for Screencap...");
            state.Screencap = await AsyncScreencap();
            state.UpdateCmdStdOut(@"Screencap Recieved!");
        }

        public async Task<BitmapImage> AsyncScreencap(int delay=100)
        {
            string pngName = string.Format("screencap_{0:D5}.png", new Random().Next(99999));
            string pngPathOnPhone = string.Format("/sdcard/{0}", pngName);
            string pngPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), pngName);
            BitmapImage result = null;
            CmdBuilder cmd = new CmdBuilder()
            {
                Command = "adb"
            };            
            cmd.UpdateInputArgs("Shell", "shell");
            cmd.UpdateInputArgs("Screencap", "screencap");
            cmd.UpdateInputArgs("SaveAsPng", "-p");
            cmd.UpdateOutput("Output", pngPathOnPhone);
            Debug.WriteLine(cmd.GetFullCmdString());
            CommandExcutor excutor = new CommandExcutor(cmd);
            Thread.Sleep(delay); //wait for phone's response
            await excutor.AsyncExcute(5000);            
            cmd = new CmdBuilder()
            {
                Command = "adb"
            };            
            cmd.UpdateInputArgs("Pull", "pull");
            cmd.UpdateInput("Input", pngPathOnPhone);
            cmd.UpdateOutput("Output", pngPath);
            excutor = new CommandExcutor(cmd);
            await excutor.AsyncExcute();
            result = ImageLoader.Open(pngPath);
            File.Delete(pngPath);
            cmd = new CmdBuilder()
            {
                Command = "adb"
            };            
            cmd.UpdateInputArgs("Shell", "shell");
            cmd.UpdateInputArgs("Delete", "delete");
            cmd.UpdateInput("Input", pngPathOnPhone);
            excutor = new CommandExcutor(cmd);
            await excutor.AsyncExcute();
            return result;
        }
        
        public BitmapImage Screencap(int delay=100)
        {
            string pngName = string.Format("screencap_{0:D5}.png", new Random().Next(99999));
            string pngPathOnPhone = string.Format("/sdcard/{0}", pngName);
            string pngPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), pngName);
            BitmapImage result = null;
            CmdBuilder cmd = new CmdBuilder()
            {
                Command = "adb"
            };            
            cmd.UpdateInputArgs("Shell", "shell");
            cmd.UpdateInputArgs("Screencap", "screencap");
            cmd.UpdateInputArgs("SaveAsPng", "-p");
            cmd.UpdateOutput("Output", pngPathOnPhone);
            CommandExcutor excutor = new CommandExcutor(cmd);
            Thread.Sleep(delay); //wait for phone's response
            excutor.Excute();
            cmd = new CmdBuilder()
            {
                Command = "adb"
            };            
            cmd.UpdateInputArgs("Pull", "pull");
            cmd.UpdateInput("Input", pngPathOnPhone);
            cmd.UpdateOutput("Output", pngPath);
            excutor = new CommandExcutor(cmd);
            excutor.Excute();
            result = ImageLoader.Open(pngPath);
            File.Delete(pngPath);
            cmd = new CmdBuilder()
            {
                Command = "adb"
            };            
            cmd.UpdateInputArgs("Shell", "shell");
            cmd.UpdateInputArgs("Delete", "delete");
            cmd.UpdateInput("Input", pngPathOnPhone);
            excutor = new CommandExcutor(cmd);
            excutor.Excute();
            return result;
        }
        
        private void SendKey(int keyCode)
        {
            CmdBuilder cmd = new CmdBuilder()
            {
                Command = "adb"
            };            
            cmd.UpdateInputArgs("Shell", "shell");
            cmd.UpdateInputArgs("Input", "input");
            cmd.UpdateInputArgs("InputType", "keyevent");
            cmd.UpdateInput("KeyCode", keyCode.ToString());            
            CommandExcutor excutor = new CommandExcutor(cmd);
            excutor.Excute();            
        }
        private void SendTap(int x,int y)
        {
            CmdBuilder cmd = new CmdBuilder()
            {
                Command = "adb"
            };            
            cmd.UpdateInputArgs("Shell", "shell");
            cmd.UpdateInputArgs("Input", "input");
            cmd.UpdateInputArgs("InputType", "tap");
            cmd.UpdateInput("TapX", x.ToString());
            cmd.UpdateInput("TapY", y.ToString());
            CommandExcutor excutor = new CommandExcutor(cmd);
            excutor.Excute();
        }
        private void SendSwipe(int x1, int y1,int x2,int y2)
        {
            CmdBuilder cmd = new CmdBuilder()
            {
                Command = "adb"
            };            
            cmd.UpdateInputArgs("Shell", "shell");
            cmd.UpdateInputArgs("Input", "input");
            cmd.UpdateInputArgs("InputType", "swipe");
            cmd.UpdateInput("TapX1", x1.ToString());
            cmd.UpdateInput("TapY1", y1.ToString());
            cmd.UpdateInput("TapX2", x1.ToString());
            cmd.UpdateInput("TapY2", y1.ToString());
            CommandExcutor excutor = new CommandExcutor(cmd);
            excutor.Excute();
        }
        private void StartApp(string appName,bool useActionMode=true)
        {
            string actionType = useActionMode ? "-a" : "-n";
            CmdBuilder cmd = new CmdBuilder()
            {
                Command = "adb"
            };
            
            cmd.UpdateInputArgs("Shell", "shell");
            cmd.UpdateInputArgs("Start", "am start");
            cmd.UpdateInputArgs("ActionType", actionType);
            //cmd.UpdateInputArgs("WaitUntilComplete", "-W");
            cmd.UpdateInput("AppName", appName);
            CommandExcutor excutor = new CommandExcutor(cmd);
            excutor.Excute(5000);
        }
        private async void _BTNMenu_Click(object sender, RoutedEventArgs e)
        {
            state.UpdateCmdStdOut("\n");
            state.UpdateCmdStdOut(@"Sending Menu Key...");
            SendKey(82);
            state.UpdateCmdStdOut(@"Menu Key Sended! Waiting for Screencap...");
            state.Screencap = await AsyncScreencap();
            state.UpdateCmdStdOut(@"Screencap Recieved!");
        }

        private async void _BTNHome_Click(object sender, RoutedEventArgs e)
        {
            state.UpdateCmdStdOut("\n");
            state.UpdateCmdStdOut(@"Sending Home Key...");
            SendKey(3);
            state.UpdateCmdStdOut(@"Home Key Sended! Waiting for Screencap...");
            state.Screencap = await AsyncScreencap();
            state.UpdateCmdStdOut(@"Screencap Recieved!");
            cameraOn = false;
        }

        private async void _BTNBack_Click(object sender, RoutedEventArgs e)
        {
            state.UpdateCmdStdOut("\n");
            state.UpdateCmdStdOut(@"Sending Back Key...");
            SendKey(4);
            state.UpdateCmdStdOut(@"Back Key Sended! Waiting for Screencap...");
            state.Screencap = await AsyncScreencap();
            state.UpdateCmdStdOut(@"Screencap Recieved!");
        }

        private async void _BTNPower_Click(object sender, RoutedEventArgs e)
        {
            state.UpdateCmdStdOut("\n");
            state.UpdateCmdStdOut(@"Sending Power Key...");
            SendKey(26);
            state.UpdateCmdStdOut(@"Power Key Sended! Waiting for Screencap...");
            state.Screencap = await AsyncScreencap();
            state.UpdateCmdStdOut(@"Screencap Recieved!");
        }

        private async void _BTNVolUp_Click(object sender, RoutedEventArgs e)
        {
            state.UpdateCmdStdOut("\n");
            state.UpdateCmdStdOut(@"Sending Volumn_Up Key...");
            SendKey(24);
            state.UpdateCmdStdOut(@"Volumn_UpKey Sended! Waiting for Screencap...");
            state.Screencap = await AsyncScreencap();
            state.UpdateCmdStdOut(@"Screencap Recieved!");
        }

        private async void _BTNVolDown_Click(object sender, RoutedEventArgs e)
        {
            state.UpdateCmdStdOut("\n");
            state.UpdateCmdStdOut(@"Sending Volumn_Down Key...");
            SendKey(25);
            state.UpdateCmdStdOut(@"Volumn_Down Key Sended! Waiting for Screencap...");
            state.Screencap = await AsyncScreencap();
            state.UpdateCmdStdOut(@"Screencap Recieved!");
        }

        private async void _BTNCamera_Click(object sender, RoutedEventArgs e)
        {
            state.UpdateCmdStdOut("\n");
            state.UpdateCmdStdOut(@"Starting Camera...");
            StartApp("android.media.action.STILL_IMAGE_CAMERA");
            state.UpdateCmdStdOut(@"Camera Started! Waiting for Screencap...");
            state.Screencap = await AsyncScreencap(500);
            state.UpdateCmdStdOut(@"Screencap Recieved!");
            cameraOn = true;
        }

        private async void _BTNScreencap_Click(object sender, RoutedEventArgs e)
        {
            state.UpdateCmdStdOut("\n");
            state.UpdateCmdStdOut(@"Waiting for Screencap...");
            state.Screencap = await AsyncScreencap(0);
            state.UpdateCmdStdOut(@"Screencap Recieved!");
        }

        private async void _BTNRecord_Click(object sender, RoutedEventArgs e)
        {
            if (cameraOn == true)
            {
                state.UpdateCmdStdOut("\n");
                state.UpdateCmdStdOut(@"Tap Camera Record Button...");
                SendTap(640,1200);
                state.UpdateCmdStdOut(@"Record Button Tapped! Waiting for Screencap...");
                state.Screencap = await AsyncScreencap(300);
                state.UpdateCmdStdOut(@"Screencap Recieved!");
            }
            else
            {
                state.UpdateCmdStdOut("\n");
                state.UpdateCmdStdOut(@"!!Make Sure Camera is ON...");
            }
        }

        private async void _BTNFocus_Click(object sender, RoutedEventArgs e)
        {
            if (cameraOn == true)
            {
                state.UpdateCmdStdOut("\n");
                state.UpdateCmdStdOut(@"Tap Screen to Auto Focus...");
                SendTap(360, 640);
                state.UpdateCmdStdOut(@"Screen Tapped! Waiting for Screencap...");
                state.Screencap = await AsyncScreencap(500);
                state.UpdateCmdStdOut(@"Screencap Recieved!");
            }
            else
            {
                state.UpdateCmdStdOut("\n");
                state.UpdateCmdStdOut(@"!!Make Sure Camera is ON...");
            }
        }

        private async void _BTNShot_Click(object sender, RoutedEventArgs e)
        {
            if (cameraOn == true)
            {
                state.UpdateCmdStdOut("\n");
                state.UpdateCmdStdOut(@"Tap Camera Shot/Pause Button...");
                SendTap(360, 1200);
                state.UpdateCmdStdOut(@"Shot/Pause Button Tapped! Waiting for Screencap...");                
                state.Screencap = await AsyncScreencap(300);
                state.UpdateCmdStdOut(@"Screencap Recieved!");
            }
            else
            {
                state.UpdateCmdStdOut("\n");
                state.UpdateCmdStdOut(@"!!Make Sure Camera is ON...");
            }
        }

        private async void _BTNFTP_Click(object sender, RoutedEventArgs e)
        {            
            state.UpdateCmdStdOut("\n");
            state.UpdateCmdStdOut(@"Starting FTP Server...");
            StartApp("com.estrongs.android.pop/com.estrongs.android.pop.ftp.ESFtpShortcut",false);
            state.UpdateCmdStdOut(@"FTP Server Started! Waiting for Screencap...");
            state.Screencap = await AsyncScreencap();
            state.UpdateCmdStdOut(@"Screencap Recieved!");
            cameraOn = true;
        }

        private async void _ScreencapImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            screencapScale = 1280 / _ScreencapImage.ActualHeight;
            Debug.WriteLine(string.Format("scale:{0}", screencapScale));
            Point position = e.GetPosition(_ScreencapImage);
            Point posOnPhone = new Point(position.X * screencapScale, position.Y * screencapScale);
            Debug.WriteLine(string.Format("Posion:{0},{1}", position.X, position.Y));
            Debug.WriteLine(string.Format("PosionOnPhone:{0},{1}", posOnPhone.X, posOnPhone.Y));
            int posOnPhoneX = (int)Math.Round(posOnPhone.X);
            int posOnPhoneY = (int)Math.Round(posOnPhone.Y);
            state.UpdateCmdStdOut("\n");
            state.UpdateCmdStdOut(@"Tapping Screen...");
            SendTap(posOnPhoneX, posOnPhoneY);
            state.UpdateCmdStdOut(@"Screen Tapped! Waiting for Screencap...");
            state.Screencap = await AsyncScreencap(300);
            state.UpdateCmdStdOut(@"Screencap Recieved!");
        }

        private async void _BTNSendCom_Click(object sender, RoutedEventArgs e)
        {
            state.UpdateCmdStdOut("\n");
            CmdBuilder cmd = new CmdBuilder()
            {
                Command = "adb"
            };            
            cmd.UpdateInputArgs("args", _TBCommand.Text);
            CommandExcutor excutor = new CommandExcutor(cmd);
            var stdout = await excutor.AsyncExcute(5000);
            state.UpdateCmdStdOut(stdout["StdOut"]);
            state.UpdateCmdStdOut(@"Command Sended!Waiting for Screencap...");
            state.Screencap = await AsyncScreencap(0);
            state.UpdateCmdStdOut(@"Screencap Recieved!");
        }
    }


    public class AppState : System.ComponentModel.INotifyPropertyChanged
    {
        private BitmapImage screencap;
        public BitmapImage Screencap
        {
            get { return screencap; }
            set
            {
                screencap = value;
                NotifyPropertyChanged("Screencap");
            }
        }

        private string ip;
        public string IP
        {
            get { return ip; }
            set
            {
                ip = value;
                NotifyPropertyChanged("IP");
            }
        }

        private string cmdStdOut;
        public string CmdStdOut
        {
            get { return cmdStdOut; }
            set
            {
                cmdStdOut = value;
                NotifyPropertyChanged("CmdStdOut");
            }
        }

        //构造函数
        public AppState()
        {
            Screencap = null;
            IP = @"192.168.1.110";
            CmdStdOut = string.Empty;
        }
        public void UpdateCmdStdOut(string stdout)
        {
            CmdStdOut = cmdStdOut.Insert(0, stdout + "\n");
        }
        //注册属性改变事件，便于通告属性改变
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop));
        }
    }


}
