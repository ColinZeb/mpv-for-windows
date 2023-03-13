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


            Init();


        }

        private void OnCountClicked(object sender, RoutedEventArgs e)
        {
            Core.Shown = true;
            count++;
            btnCount.Content = count switch
            {
                1 => $"Clicked {count} time",
                _ => $"Clicked {count} times",
            };
        }

        /// <summary>
        /// ´°¿Ú¾ä±ú
        /// </summary>
        nint Handle => WinRT.Interop.WindowNative.GetWindowHandle(this);

        void Init()
        {
            Core.Init(Handle);

        }

        private void StackPanel_DragEnter(object sender, DragEventArgs e)
        { 

            
        }

        private void StackPanel_DragOver(object sender, DragEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Core.LoadFiles(new[] { @"I:\TV\1883\1883.S01E01.1883.2160p.WEB-DL.DDP5.1.H.265-NTb.mkv" }, true, false);
        }

        private void TextBlock_Drop(object sender, DragEventArgs e)
        {

        }

        private void TextBlock_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        { 

        }
    }
}
