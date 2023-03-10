using Microsoft.UI.Xaml;
using Windows.Services.Maps;

using static mpvnet.Native;
using static mpvnet.Global;
using System.Globalization;
using System.Reflection.Metadata;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUIApp1
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public IntPtr mpvWindowHandle { get; set; }
        int count = 0;

        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;

            Core.FileLoaded += Core_FileLoaded;
            Core.MoveWindow += Core_MoveWindow;
            Core.Pause += Core_Pause;
            Core.PlaylistPosChanged += Core_PlaylistPosChanged;
            Core.ScaleWindow += Core_ScaleWindow;
            Core.Seek += () => UpdateProgressBar();
            Core.ShowMenu += Core_ShowMenu;
            Core.Shutdown += Core_Shutdown;
            Core.VideoSizeChanged += Core_VideoSizeChanged;
            Core.WindowScaleMpv += Core_WindowScaleMpv;
            Core.WindowScaleNET += Core_WindowScaleNET;

            if (Core.Screen > -1)
            {
                int targetIndex = Core.Screen;
                Screen[] screens = Screen.AllScreens;

                if (targetIndex < 0)
                    targetIndex = 0;

                if (targetIndex > screens.Length - 1)
                    targetIndex = screens.Length - 1;

                Screen screen = screens[Array.IndexOf(screens, screens[targetIndex])];
                Rectangle target = screen.Bounds;
                Left = target.X + (target.Width - Width) / 2;
                Top = target.Y + (target.Height - Height) / 2;
            }

            if (!Core.Border)
                FormBorderStyle = FormBorderStyle.None;

            Point pos = App.Settings.WindowPosition;

            if ((pos.X != 0 || pos.Y != 0) && App.RememberWindowPosition)
            {
                Left = pos.X - Width / 2;
                Top = pos.Y - Height / 2;

                Point location = App.Settings.WindowLocation;

                if (location.X == -1) Left = pos.X;
                if (location.X == 1) Left = pos.X - Width;
                if (location.Y == -1) Top = pos.Y;
                if (location.Y == 1) Top = pos.Y - Height;
            }

            if (Core.WindowMaximized)
            {
                SetFormPosAndSize(true);
                WindowState = FormWindowState.Maximized;
            }

            if (Core.WindowMinimized)
            {
                SetFormPosAndSize(true);
                WindowState = FormWindowState.Minimized;
            }
        }

        private void OnCountClicked(object sender, RoutedEventArgs e)
        {
            count++;
            btnCount.Content = count switch
            {
                1 => $"Clicked {count} time",
                _ => $"Clicked {count} times",
            };
        }

        void Core_MoveWindow(string direction)
        {
            BeginInvoke(new Action(() =>
            {
                Screen screen = Screen.FromControl(this);
                Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);

                switch (direction)
                {
                    case "left":
                        Left = workingArea.Left;
                        break;
                    case "top":
                        Top = 0;
                        break;
                    case "right":
                        Left = workingArea.Width - Width + workingArea.Left;
                        break;
                    case "bottom":
                        Top = workingArea.Height - Height;
                        break;
                    case "center":
                        Left = (screen.Bounds.Width - Width) / 2;
                        Top = (screen.Bounds.Height - Height) / 2;
                        break;
                }
            }));
        }

        void Core_PlaylistPosChanged(int pos)
        {
            if (pos == -1)
                SetTitle();
        }

        void Init()
        {
            Core.Init(Handle);

            // bool methods not working correctly
            Core.ObserveProperty("window-maximized", PropChangeWindowMaximized);
            Core.ObserveProperty("window-minimized", PropChangeWindowMinimized);

            Core.ObservePropertyBool("border", PropChangeBorder);
            Core.ObservePropertyBool("fullscreen", PropChangeFullscreen);
            Core.ObservePropertyBool("keepaspect-window", value => Core.KeepaspectWindow = value);
            Core.ObservePropertyBool("ontop", PropChangeOnTop);

            Core.ObservePropertyString("sid", PropChangeSid);
            Core.ObservePropertyString("aid", PropChangeAid);
            Core.ObservePropertyString("vid", PropChangeVid);

            Core.ObservePropertyString("title", PropChangeTitle);

            Core.ObservePropertyInt("edition", PropChangeEdition);

            Core.ProcessCommandLine(false);
        }

        void Core_ShowMenu()
        {
            BeginInvoke(new Action(() =>
            {
                if (IsMouseInOSC())
                    return;

                CursorHelp.Show();
                UpdateMenu();
                ContextMenu.IsOpen = true;
            }));
        }

        void Core_ScaleWindow(float scale)
        {
            BeginInvoke(new Action(() =>
            {
                int w, h;

                if (KeepSize())
                {
                    w = (int)(ClientSize.Width * scale);
                    h = (int)(ClientSize.Height * scale);
                }
                else
                {
                    w = (int)(ClientSize.Width * scale);
                    h = (int)Math.Ceiling(w * Core.VideoSize.Height / (double)Core.VideoSize.Width);
                }

                SetSize(w, h, Screen.FromControl(this), false);
            }));
        }

        void Core_WindowScaleNET(float scale)
        {
            BeginInvoke(new Action(() =>
            {
                SetSize(
                    (int)(Core.VideoSize.Width * scale),
                    (int)Math.Ceiling(Core.VideoSize.Height * scale),
                    Screen.FromControl(this), false);
                Core.Command($"show-text \"window-scale {scale.ToString(CultureInfo.InvariantCulture)}\"");
            }));
        }

        void Core_WindowScaleMpv(double scale)
        {
            if (!Core.Shown)
                return;

            BeginInvoke(new Action(() =>
            {
                SetSize(
                    (int)(Core.VideoSize.Width * scale),
                    (int)Math.Ceiling(Core.VideoSize.Height * scale),
                    Screen.FromControl(this), false);
            }));
        }

        void Core_Shutdown() => BeginInvoke(new Action(() => Close()));

        void CM_Popup(object sender, EventArgs e) => CursorHelp.Show();

        void Core_VideoSizeChanged(Size value) => BeginInvoke(new Action(() =>
        {
            if (!KeepSize())
                SetFormPosAndSize();
        }));
    }
    void SetSize(int width, int height, Screen screen, bool checkAutofit = true)
    {
        Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);

        int maxHeight = workingArea.Height - (Height - ClientSize.Height) - 2;
        int maxWidth = workingArea.Width - (Width - ClientSize.Width);

        int startWidth = width;
        int startHeight = height;

        if (checkAutofit)
        {
            if (height < maxHeight * Core.AutofitSmaller)
            {
                height = Convert.ToInt32(maxHeight * Core.AutofitSmaller);
                width = Convert.ToInt32(height * startWidth / (double)startHeight);
            }

            if (height > maxHeight * Core.AutofitLarger)
            {
                height = Convert.ToInt32(maxHeight * Core.AutofitLarger);
                width = Convert.ToInt32(height * startWidth / (double)startHeight);
            }
        }

        if (width > maxWidth)
        {
            width = maxWidth;
            height = (int)Math.Ceiling(width * startHeight / (double)startWidth);
        }

        if (height > maxHeight)
        {
            height = maxHeight;
            width = Convert.ToInt32(height * startWidth / (double)startHeight);
        }

        if (height < maxHeight * 0.1)
        {
            height = Convert.ToInt32(maxHeight * 0.1);
            width = Convert.ToInt32(height * startWidth / (double)startHeight);
        }

        Point middlePos = new Point(Left + Width / 2, Top + Height / 2);
        var rect = new RECT(new Rectangle(screen.Bounds.X, screen.Bounds.Y, width, height));
        AddWindowBorders(Handle, ref rect, GetDPI(Handle));

        int left = middlePos.X - rect.Width / 2;
        int top = middlePos.Y - rect.Height / 2;

        Rectangle currentRect = new Rectangle(Left, Top, Width, Height);

        if (GetHorizontalLocation(screen) == -1) left = Left;
        if (GetHorizontalLocation(screen) == 1) left = currentRect.Right - rect.Width;

        if (GetVerticalLocation(screen) == -1) top = Top;
        if (GetVerticalLocation(screen) == 1) top = currentRect.Bottom - rect.Height;

        Screen[] screens = Screen.AllScreens;

        int minLeft = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).X).Min();
        int maxRight = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).Right).Max();
        int minTop = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).Y).Min();
        int maxBottom = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).Bottom).Max();

        if (left < minLeft)
            left = minLeft;

        if (left + rect.Width > maxRight)
            left = maxRight - rect.Width;

        if (top < minTop)
            top = minTop;

        if (top + rect.Height > maxBottom)
            top = maxBottom - rect.Height;

        uint SWP_NOACTIVATE = 0x0010;
        SetWindowPos(Handle, IntPtr.Zero, left, top, rect.Width, rect.Height, SWP_NOACTIVATE);
    }
}
