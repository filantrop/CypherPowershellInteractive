using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;
using WindowsInput.Native;

namespace PowershellInteractive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PowerShell powershell ;

        private Process pDocked;
        private IntPtr hWndOriginalParent;
        private IntPtr hWndDocked;
        public System.Windows.Forms.Panel panel;
        public MainWindow()
        {
            InitializeComponent();
            //powershell = PowerShell.Create(RunspaceMode.NewRunspace);
            
            panel = new System.Windows.Forms.Panel();
            host.Child = panel;

            Init();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            Func1();
        }
        // import the function in your class

        private System.Diagnostics.Process CMDprocess;
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        private void Init()
        {
            //powershell.AddCommand(Command.Text);
            //powershell.Invoke();
            CMDprocess = new System.Diagnostics.Process();
            var StartProcessInfo = new System.Diagnostics.ProcessStartInfo();
            StartProcessInfo.FileName = @"C:\Windows\SysWOW64\WindowsPowershell\v1.0\powershell.exe";
            StartProcessInfo.Arguments= "-NoExit -Command \"$Host.UI.RawUI.WindowTitle='Your Title Here'; $host.UI.RawUI.BufferSize = New-Object System.Management.Automation.Host.size(1040,9999);\"";
            //StartProcessInfo.Arguments = @"C:\Users\user\Desktop\Test.ps1";

            CMDprocess.StartInfo = StartProcessInfo;
            CMDprocess.Start();

            //dockIt(CMDprocess);
                      

            //CMDprocess.WaitForExit();
        }
        private void Func1()
        {
            IntPtr h = CMDprocess.MainWindowHandle;
            SetForegroundWindow(h);
            var ins = new InputSimulator();
            ins.Keyboard.TextEntry(Command.Text).KeyPress(VirtualKeyCode.RETURN);
        }

        private void dockIt(Process process)
        {
            if (hWndDocked != IntPtr.Zero) //don't do anything if there's already a window docked.
                return;

            pDocked = process;
            while (hWndDocked == IntPtr.Zero)
            {
                //pDocked.WaitForInputIdle(1000); //wait for the window to be ready for input;
                pDocked.Refresh();              //update process info
                if (pDocked.HasExited)
                {
                    return; //abort if the process finished before we got a handle.
                }
                hWndDocked = pDocked.MainWindowHandle;  //cache the window handle
            }
            //Windows API call to change the parent of the target window.
            //It returns the hWnd of the window's parent prior to this call.
            hWndOriginalParent = SetParent(hWndDocked, panel.Handle);

            //Wire up the event to keep the window sized to match the control
            SizeChanged += window_SizeChanged;
            //Perform an initial call to set the size.
            AlignToPannel();
        }

        private void AlignToPannel()
        {
            MoveWindow(hWndDocked, 0, 0, panel.Width, panel.Height, true);
        }

        void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlignToPannel();
        }
    }
}
