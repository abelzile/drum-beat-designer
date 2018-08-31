using System.Windows;
using DrumBeatDesigner.Models;
using DrumBeatDesigner.ViewModels;
using MoonAndSun.Commons.Mvvm;


namespace DrumBeatDesigner
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        public ExportWindow(Project project)
        {
            InitializeComponent();

            Vm.Project = project;
        }
        
        ExportViewModel Vm => (ExportViewModel)DataContext;

        public int SampleRate => Vm.SelectedSampleRate;

        public int BitsPerSample => Vm.SelectedBitsPerSample;

        public int Channels => Vm.SelectedChannels;

        public string SavePath => Vm.SavePath;

        void ViewModelDialogResultChanged(object sender, DialogResultEventArgs e)
        {
            DialogResult = e.Result;

            Close();
        }
    }
}
