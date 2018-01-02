using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Threading;

namespace StayOnline
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Import Winapi
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg,
     int wParam, StringBuilder lParam);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        #endregion


        public MainWindow()
        {
            InitializeComponent();
                         
        }
     
        // hWnd是句柄，factor是透明度0~255
        bool MakeWindowTransparent(IntPtr hWnd, byte factor)
        {
            const int GWL_EXSTYLE = (-20);
            const uint WS_EX_LAYERED = 0x00080000;
            int Cur_STYLE = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, (uint)(Cur_STYLE | WS_EX_LAYERED));
            const uint LWA_COLORKEY = 1;
            const uint LWA_ALPHA = 2;
            const uint WHITE = 0xffffff;
            return SetLayeredWindowAttributes(hWnd, WHITE, factor, LWA_COLORKEY | LWA_ALPHA);
        }
        DispatcherTimer timer = new DispatcherTimer();
        private void subBTN_Click(object sender, RoutedEventArgs e)
        {
            if (fixedEX.IsExpanded == false && randomEX.IsExpanded == false)
            {
                System.Windows.MessageBox.Show("Please Select how the keep online!");
            }
            else
            {
               
                if (fixedEX.IsExpanded == true)
                    timer.Interval = TimeSpan.FromSeconds(Convert.ToInt32(countTB.Text));
                else if (randomEX.IsExpanded == true)
                {
                    if(Convert.ToInt32(startcount.Text)>Convert.ToInt32(endcount.Text))
                    {
                        System.Windows.MessageBox.Show("The Start count can not bigger than End Count!");
                        return;
                    }
                    Random r = new Random();
                    int result = r.Next(Convert.ToInt32(startcount.Text), Convert.ToInt32(endcount.Text));
                    timer.Interval = TimeSpan.FromSeconds(result);
                }

                timer.Tick += KeepStayOnline;
                timer.Start();
                System.Windows.MessageBox.Show("It is to start to try keep hug coin online!");
                subBTN.Visibility = Visibility.Collapsed;
                endBTN.Visibility = Visibility.Visible;
              
            }
        }

        private void KeepStayOnline(object sender, EventArgs e)
        {
            bool doit = false;
            //获得句柄
            foreach (var item in WindowsEnumerator.GetWindowHandles("HUG - Debug window"))
            {
               
                if (doit == false)
                {
                    //发送addnode hugcoin.online onetry        HUG - Debug window
                    //操作获得句柄的特定窗口
                    StringBuilder kkk = new StringBuilder("addnode");
                    SetForegroundWindow(item);
                    ShowWindow(item, 1);

                    //模拟按键 对应System.Windows.Forms控件
                    SendKeys.SendWait("addnode hugcoin.online add");
                    SendKeys.SendWait("{ENTER}");
                    doit = true;
                }
            }
        }

        private void randomEX_Expanded(object sender, RoutedEventArgs e)
        {
            if (randomEX.IsExpanded == true)
                fixedEX.IsExpanded = false;
           
        }

        private void ShowHelp(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("You should open the hug wallet.and click Help->Debug Window.And Click the Console Tab of the \"HUG-Debug Window\" Window.Then you set the minute of this software and click start button.It will keep the hug wallet online automately.");
        }

        private void fixedEX_Expanded(object sender, RoutedEventArgs e)
        {
            if (fixedEX.IsExpanded == true)
                randomEX.IsExpanded = false;
        }

        private void endBTN_Click(object sender, RoutedEventArgs e)
        {
            timer.IsEnabled = false;
            subBTN.Visibility = Visibility.Visible;
            endBTN.Visibility = Visibility.Hidden;
        }

        private void countTB_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
      (e.Key >= Key.D0 && e.Key <= Key.D9) ||
      e.Key == Key.Back ||
      e.Key == Key.Left || e.Key == Key.Right)
            {
                if (e.KeyboardDevice.Modifiers != ModifierKeys.None)
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }
    }

    // 枚举窗体
    public static class WindowsEnumerator
    {
        private delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumChildWindows(IntPtr hWndStart, EnumWindowsProc callback, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        private static List<IntPtr> handles = new List<IntPtr>();
        private static string targetName;
        public static List<IntPtr> GetWindowHandles(string target)
        {
            targetName = target;
            EnumWindows(EnumWindowsCallback, IntPtr.Zero);
            return handles;
        }
        private static bool EnumWindowsCallback(IntPtr HWND, IntPtr includeChildren)
        {
            StringBuilder name = new StringBuilder(GetWindowTextLength(HWND) + 1);
            GetWindowText(HWND, name, name.Capacity);
            if (name.ToString() == targetName)
                handles.Add(HWND);
            EnumChildWindows(HWND, EnumWindowsCallback, IntPtr.Zero);
            return true;
        }
    }
}
