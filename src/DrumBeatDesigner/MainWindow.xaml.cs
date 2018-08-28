using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace DrumBeatDesigner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IDictionary<ScrollViewer, Control> _scrollPairs = new Dictionary<ScrollViewer, Control>();
            
        public MainWindow()
        {
            InitializeComponent();

            _scrollPairs.Add(uxBeats, uxInstruments);
            _scrollPairs.Add(uxPatternItems, uxPatterns);
        }

        private void ScrollViewerSynchronize(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer) sender;
            var ctrl = _scrollPairs[scrollViewer];

            switch (ctrl)
            {
                case ListBox _:
                {
                    if (VisualTreeHelper.GetChild(ctrl, 0) is Decorator border)
                    {
                        if (border.Child is ScrollViewer scrollV)
                        {
                            scrollV.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
                        }
                    }

                    break;
                }
                case ScrollViewer _:
                {
                    ((ScrollViewer) ctrl).ScrollToVerticalOffset(scrollViewer.VerticalOffset);
                    break;
                }
            }

        }

        void ListBoxItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}