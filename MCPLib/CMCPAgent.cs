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
        public string name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("name");
            }
        }

        public string description { get; set; }
        public string type { get; set; }
        public string cmdShellUrl { get; set; }
        public int? parent { get; set; }
        public string category { get; set; }
        public IList<int> ancestors { get; set; }
        public string icon { get; set; }

        public string picSource
        {
            get
            {
                return string.Format("Content/agenticons/{0}", string.IsNullOrEmpty(this.icon) ? "nopic.png" : this.icon);
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
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    
}
