using System;
using System.Collections.Generic;
using System.Text;

namespace MCP.Lib
{
    /// <summary>
    /// MCP Package
    /// </summary>
    public class CMCPPackage
    {
        public enum PackageType
        {
            Info = 0,
            Warning = 1,
            Error = 2
        }

        public long _id { get; set; }

        public string FunctionID { get; set; }

        public string DevKeyID { get; set; }

        public string JobID { get; set; }

        public DateTime DT { get; set; }

        public PackageType Type { get; set; }
        public int NoticeCount { get; set; }

        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }

        public string Blob { get; set; }
        public Dictionary<string, string> KeyVals { get; set; }


        internal string Outbox;

        internal const string DEFAULT_OUTBOX_PATH = ".";

        public CMCPPackage()
        {
            KeyVals = new Dictionary<string, string>();
        }

        public CMCPPackage(Guid Func, Guid Dev, string outbox = DEFAULT_OUTBOX_PATH)
        {
            KeyVals = new Dictionary<string, string>();
            FunctionID = Func.ToString();
            DevKeyID = Dev.ToString();
            Outbox = outbox;
            StartNewJob();
        }

        public void ClearKeyVals()
        {
            if (KeyVals != null) KeyVals.Clear();
        }

        public void SaveToFolder(string outbox = null)
        {
            if (string.IsNullOrEmpty(outbox)) { outbox = Outbox; }
            //			string js = JsonConvert.SerializeObject(this);
            //var it = this.ToBsonDocument();

            //            File.WriteAllText(string.Format("{0}{1}_{2}.json", outbox, FunctionID, DateTime.Now.ToString("yyyyMMddHHmmssfff")), it.ToString());
        }

        public void StartNewJob()
        {
            JobID = Guid.NewGuid().ToString();
        }

        public void SetJob(Guid newjobid)
        {
            JobID = newjobid.ToString();
        }
    }
}
