using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonAndSun.Commons.Mvvm;

namespace DrumBeatDesigner.ViewModels
{
    public class InputBoxViewModel : DialogViewModelBase, IDataErrorInfo
    {
        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();
        private string _inputLabel;
        private string _inputText;
        
        public string this[string columnName]
        {
            get
            {
                string msg = "";

                _errors.Remove(columnName);

                switch (columnName)
                {
                    case "InputText":
                    {
                        msg = (string.IsNullOrWhiteSpace(InputText)) ? "Input required." : "";
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(msg))
                {
                    _errors.Add(columnName, msg);
                }

                OkCommand.RaiseCanExecuteChanged();

                return msg;
            }
        }

        public string Error { get; private set; }

        public string InputLabel
        {
            get => _inputLabel;
            set
            {
                if (value == _inputLabel)
                {
                    return;
                }
                _inputLabel = value;
                RaisePropertyChanged(() => InputLabel);
            }
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                if (value == _inputText)
                {
                    return;
                }
                _inputText = value;
                RaisePropertyChanged(() => InputText);
            }
        }

        protected override bool CanDoOk()
        {
            return !_errors.Any();
        }
    }
}
