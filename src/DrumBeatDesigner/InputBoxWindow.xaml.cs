using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DrumBeatDesigner.ViewModels;
using MoonAndSun.Commons.Mvvm;

namespace DrumBeatDesigner
{
    /// <summary>
    /// Interaction logic for InputBoxWindow.xaml
    /// </summary>
    public partial class InputBoxWindow : Window
    {
        public InputBoxWindow(string inputLabel, string inputText)
        {
            InitializeComponent();

            Vm.InputLabel = inputLabel;
            Vm.InputText = inputText;
        }

        InputBoxViewModel Vm => (InputBoxViewModel)DataContext;

        public string InputLabel => Vm.InputLabel;

        public string InputText => Vm.InputText;

        void ViewModelDialogResultChanged(object sender, DialogResultEventArgs e)
        {
            DialogResult = e.Result;

            Close();
        }
    }
}
