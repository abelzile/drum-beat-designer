using System.Windows;
using System.Windows.Controls;


namespace DrumBeatDesigner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ScrollViewerSynchronize(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                uxChannels.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
            }
        }
    }
}