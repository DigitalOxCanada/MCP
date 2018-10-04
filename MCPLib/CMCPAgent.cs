using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MCP.Lib
{
    /// <summary>
    /// MCP Agent is the root object to collect packages against.
    /// An Agent has a unique ID that all packages reference by FunctionID == this._id
    /// </summary>
    public class CMCPAgent : INotifyPropertyChanged
    {
        public string _id { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("name");
            }
        }

        public string Description { get; set; }
        public string Type { get; set; }
        public string CmdShellUrl { get; set; }
        public int? Parent { get; set; }
        public string Category { get; set; }
        public IList<int> Ancestors { get; set; }
        public string Icon { get; set; }

        public string PicSource
        {
            get
            {
                return string.Format("Content/agenticons/{0}", string.IsNullOrEmpty(this.Icon) ? "nopic.png" : this.Icon);
            }
        }

        private int _cntChanged;
        public int CntChanged
        {
            get { return _cntChanged; }
            set
            {
                _cntChanged = value;
                OnPropertyChanged("CntChanged");
            }
        }

        //public ImageSource theimage {
        //    get
        //    {
        //        return (ImageSource)new ImageSourceConverter().ConvertFromString(picSource);
        //    }
        //}
        private int _errorCount;
        public int ErrorCount { get { return _errorCount; } set { _errorCount = value; OnPropertyChanged("ErrorCount"); } }
        private int _warningCount;
        public int WarningCount { get { return _warningCount; } set { _warningCount = value; OnPropertyChanged("WarningCount"); } }
        private int _infoCount;
        public int InfoCount { get { return _infoCount; } set { _infoCount = value; OnPropertyChanged("InfoCount"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    
}
