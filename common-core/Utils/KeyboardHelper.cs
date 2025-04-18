using log4net;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class KeyboardHelper
{
    private static ILog logger = LogManager.GetLogger(nameof(KeyboardHelper));

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public const int SW_SHOW = 5;

    public static void ShowOnScreenKeyboard()
    {
        try
        {
            // 确保键盘已打开
            IntPtr hwnd = FindWindow("IPTip_Main_Window", null);
            if (hwnd == IntPtr.Zero) // 如果没有找到窗口
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "osk",
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(processInfo); // 启动屏幕键盘
            }
            else
            {
                // 如果已打开，可以选择显示或最小化等操作
                ShowWindow(hwnd, SW_SHOW); // 显示已存在的屏幕键盘
            }
        }
        catch (Exception ex)
        {
            logger.Debug($"Error: {ex.Message}");
        }
    }
}