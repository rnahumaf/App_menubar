using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TinyClipboardTray;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new TrayAppContext());
    }
}

sealed class TrayAppContext : ApplicationContext
{
    private readonly NotifyIcon _tray;
    private readonly ClipboardWindow _clipboardWindow;

    private bool _enabled = true;
    private bool _selfUpdating = false;

    private long _sessionAffected = 0;
    private long _allTimeAffected = 0;
    private readonly StatsStore _store;

    private readonly ToolStripMenuItem _sessionItem;
    private readonly ToolStripMenuItem _allTimeItem;

    public TrayAppContext()
    {
        _store = StatsStore.Load();
        _allTimeAffected = _store.AllTimeAffectedChars;

        var menu = new ContextMenuStrip();

        _sessionItem = new ToolStripMenuItem() { Enabled = false };
        _allTimeItem = new ToolStripMenuItem() { Enabled = false };

        var enabledItem = new ToolStripMenuItem("Enabled")
        {
            Checked = true,
            CheckOnClick = true
        };
        enabledItem.CheckedChanged += (_, __) => _enabled = enabledItem.Checked;

        var exitItem = new ToolStripMenuItem("Exit");

        menu.Items.Add(_sessionItem);
        menu.Items.Add(_allTimeItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(enabledItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _tray = new NotifyIcon
        {
            Text = "Tiny Clipboard Normalizer",
            Visible = true,
            ContextMenuStrip = menu,
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application
        };


        _clipboardWindow = new ClipboardWindow();
        _clipboardWindow.ClipboardTextChanged += OnClipboardTextChanged;

        exitItem.Click += (_, __) =>
        {
            _store.AllTimeAffectedChars = _allTimeAffected;
            _store.Save();

            _tray.Visible = false;
            _clipboardWindow.Dispose();
            ExitThread();
        };

        UpdateMenuCounters();
    }

    private void UpdateMenuCounters()
    {
        _sessionItem.Text = $"Session affected chars: {_sessionAffected}";
        _allTimeItem.Text = $"All time affected chars: {_allTimeAffected}";
    }

    private void OnClipboardTextChanged()
    {
        if (!_enabled) return;
        if (_selfUpdating) return;

        if (!Clipboard.ContainsText(TextDataFormat.UnicodeText))
            return;

        string original;
        try
        {
            original = ReadClipboardTextWithRetry();
        }
        catch
        {
            return;
        }

        var result = ClipboardNormalizer.NormalizeWithStats(original);
        if (result.TotalAffected == 0) return;

        try
        {
            _selfUpdating = true;
            Clipboard.SetText(result.Text, TextDataFormat.UnicodeText);
        }
        finally
        {
            _selfUpdating = false;
        }

        _sessionAffected += result.TotalAffected;
        _allTimeAffected += result.TotalAffected;

        // persiste “ao todo” (simples e seguro; se quiser, dá pra fazer por timer)
        _store.AllTimeAffectedChars = _allTimeAffected;
        _store.Save();

        UpdateMenuCounters();
    }

    private static string ReadClipboardTextWithRetry()
    {
        for (int i = 0; i < 8; i++)
        {
            try
            {
                return Clipboard.GetText(TextDataFormat.UnicodeText);
            }
            catch (ExternalException)
            {
                Thread.Sleep(15);
            }
        }
        return Clipboard.GetText(TextDataFormat.UnicodeText);
    }
}

sealed class ClipboardWindow : NativeWindow, IDisposable
{
    public event Action? ClipboardTextChanged;

    public ClipboardWindow()
    {
        CreateHandle(new CreateParams());
        AddClipboardFormatListener(this.Handle);
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_CLIPBOARDUPDATE = 0x031D;

        if (m.Msg == WM_CLIPBOARDUPDATE)
            ClipboardTextChanged?.Invoke();

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        RemoveClipboardFormatListener(this.Handle);
        DestroyHandle();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
}
