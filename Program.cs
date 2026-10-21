using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace TrayProxy;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ProxyApplicationContext());
    }
}

internal sealed class ProxyApplicationContext : ApplicationContext
{
    private const string RegistryPath =
        @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    private const string ProxyEnableName = "ProxyEnable";

    private readonly NotifyIcon trayIcon;
    private readonly ToolStripMenuItem toggleMenuItem;
    private readonly Icon offIcon;
    private readonly Icon onIcon;

    private bool isEnabled;
    private bool isDisposed;

    public ProxyApplicationContext()
    {
        offIcon = CreateProxyIcon(false);
        onIcon = CreateProxyIcon(true);

        toggleMenuItem = new ToolStripMenuItem();
        toggleMenuItem.Click += (_, _) => ToggleProxy();

        var exitMenuItem = new ToolStripMenuItem("Выход");
        exitMenuItem.Click += (_, _) => ExitThread();

        var menu = new ContextMenuStrip();
        menu.Items.Add(toggleMenuItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitMenuItem);

        trayIcon = new NotifyIcon
        {
            Icon = offIcon,
            Text = "TrayProxy",
            ContextMenuStrip = menu,
            Visible = true
        };

        trayIcon.MouseClick += OnTrayMouseClick;

        RefreshState();
    }

    private void OnTrayMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            ToggleProxy();
    }

    private void ToggleProxy()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                RegistryPath, writable: true);

            if (key is null)
            {
                MessageBox.Show(
                    "Не удалось открыть настройки прокси Windows.",
                    "TrayProxy",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            bool currentState =
                Convert.ToInt32(key.GetValue(ProxyEnableName, 0)) != 0;

            key.SetValue(
                ProxyEnableName,
                currentState ? 0 : 1,
                RegistryValueKind.DWord);

            NotifyWindows();

            RefreshState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось переключить прокси:\n{ex.Message}",
                "TrayProxy",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            RefreshState();
        }
    }

    private void RefreshState()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);

            isEnabled = Convert.ToInt32(
                key?.GetValue(ProxyEnableName, 0) ?? 0) != 0;
        }
        catch
        {
            isEnabled = false;
        }

        trayIcon.Icon = isEnabled ? onIcon : offIcon;
        trayIcon.Text = isEnabled
            ? "ВКЛ"
            : "ВЫКЛ";

        toggleMenuItem.Text = isEnabled
            ? "Выключить прокси"
            : "Включить прокси";
    }

    private static void NotifyWindows()
    {
        InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
        InternetSetOption(IntPtr.Zero, 37, IntPtr.Zero, 0);
    }

    [DllImport("wininet.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool InternetSetOption(
        IntPtr internet,
        int option,
        IntPtr buffer,
        int bufferLength);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private static Icon CreateProxyIcon(bool enabled)
    {
        using var bitmap = new Bitmap(32, 32);

        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            Color color = enabled
                ? Color.FromArgb(40, 210, 105)
                : Color.FromArgb(155, 155, 155);

            using var pen = new Pen(color, 3.2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            graphics.DrawEllipse(pen, 4, 4, 24, 24);

            if (enabled)
            {
                graphics.DrawLines(pen, new[]
                {
                    new Point(10, 16),
                    new Point(14, 20),
                    new Point(22, 11)
                });
            }
            else
            {
                graphics.DrawLine(pen, 11, 11, 21, 21);
            }
        }

        IntPtr handle = bitmap.GetHicon();

        try
        {
            using Icon temporaryIcon = Icon.FromHandle(handle);
            return (Icon)temporaryIcon.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    protected override void ExitThreadCore()
    {
        if (!isDisposed)
        {
            isDisposed = true;

            trayIcon.Visible = false;
            trayIcon.Dispose();

            offIcon.Dispose();
            onIcon.Dispose();
        }

        base.ExitThreadCore();
    }
}
