using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

public class AutoClosingMessageBox
{
    System.Threading.Timer _timeoutTimer;
    string _caption;
    AutoClosingMessageBox(string text, string caption, int timeout, MessageBoxIcon icon = 0)
    {
        _caption = caption;
        _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
            null, timeout, System.Threading.Timeout.Infinite);
        using (_timeoutTimer)
            MessageBox.Show(text, caption, 0, icon);
    }
    public static void Show(string text, string caption, int timeout, MessageBoxIcon icon = 0)
    {
        new AutoClosingMessageBox(text, caption, timeout, icon);
    }
    void OnTimerElapsed(object state)
    {
        IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
        if (mbWnd != IntPtr.Zero)
            SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        _timeoutTimer.Dispose();
    }
    const int WM_CLOSE = 0x0010;
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
}

namespace OGF_tool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new OGF_Editor());
        }
    }

    class IniFile   // revision 11
    {
        private string Path;
        private string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string IniPath = null)
        {
            string file_name = (IniPath ?? EXE + ".ini");
            if (!File.Exists(file_name))
                File.Create(file_name);

            Path = new FileInfo(file_name).FullName;
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? this.EXE, Key, "", RetVal, 255, this.Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? this.EXE, Key, Value, this.Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? this.EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? this.EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
