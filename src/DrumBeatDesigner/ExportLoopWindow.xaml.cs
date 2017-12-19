using System.Windows;
using DrumBeatDesigner.Models;
using DrumBeatDesigner.ViewModels;
using MoonAndSun.Commons.Mvvm;


namespace DrumBeatDesigner
{
    /// <summary>
    /// Interaction logic for ExportLoopWindow.xaml
    /// </summary>
    public partial class ExportLoopWindow : Window
    {
        public ExportLoopWindow(Project project)
        {
            InitializeComponent();

            Vm.Project = project;
        }
        
        ExportLoopViewModel Vm
        {
            get { return (ExportLoopViewModel)DataContext; }
        }

        public int SampleRate
        {
            get { return Vm.SelectedSampleRate; }
        }

        public int BitsPerSample
        {
            get { return Vm.SelectedBitsPerSample; }
        }

        public int Channels
        {
            get { return Vm.SelectedChannels; }
        }

        public string SavePath
        {
            get { return Vm.SavePath; }
        }
        
        void ViewModelDialogResultChanged(object sender, DialogResultEventArgs e)
        {
            DialogResult = e.Result;

            Close();
        }
    }
}
