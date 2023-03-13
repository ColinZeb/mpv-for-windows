using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUIApp1
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        internal static void RunTask(Action value)
        {
            Task.Run(value);
        }

        internal static void ShowException(Exception ex)
        {
            throw new NotImplementedException();
        }

        private Window m_window;

        public static bool ShowLogo { get; internal set; }
        public static string[] HistoryFilter { get; internal set; }
        public static bool AutoLoadFolder { get; internal set; }
        public static bool AutoPlay { get; internal set; }
        public static int QuickBookmark { get; internal set; }
        public static string StartSize { get; internal set; }
        public static bool DebugMode { get; internal set; }
        public static int StartThreshold { get; internal set; }
        public static bool Version { get; internal set; }
        public static bool IsTerminalAttached { get; internal set; }
        public static bool MediaInfo { get; internal set; }
        public static string ProductName { get; internal set; }
    }
}
