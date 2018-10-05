using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CommandLineParser.Core;
using MCP.Lib;

namespace MCP.Agent.IHLogs
{
    class Program
    {
        public class CommandObject
        {
            public bool OneDay { get; set; }
            public string StartDate { get; set; }
        }

        public static readonly Guid DeveloperKey = new Guid("11111111-1111-1111-1111-111111111111");    //key of the developer responsible for this agent
        public static readonly Guid FunctionKey = new Guid("a906b109-06fa-4efe-b2ee-581c7c621451");     //agent identity key

        private const string outbox = @"\\mcpserver\MCPDropBox\";   //drop box for json files
        private static bool singleday = false;
        private static DateTime startrange = DateTime.Today;
        private static Guid JobKey = Guid.NewGuid();    //use for grouping packages into job chunks
        private const string log_path = @"\\inthealthserver\log\";
        private const string fnLastRun = "lastlog.txt";
        // Regex used to file warning and extract line. 
        const string regExString = @"(..:..:.. Warning  )(.{1,9999})";

        static void Main(string[] args)
        {
            var p = new FluentCommandLineParser<CommandObject>();
            p.Setup(arg => arg.OneDay)
                .As('o', "oneday"); // define the short and long option name
            p.Setup(arg => arg.StartDate)
                .As('s', "startdate");

            var result = p.Parse(args);
            if (result.HasErrors == true)
            {
                Console.WriteLine("Something is wrong with your parameters.");
                Console.WriteLine("args:  /oneday /startdate MM/DD/YYYY");
                return;
            }

            if(result.EmptyArgs)
            {
                Console.WriteLine("args:  /oneday /startdate MM/DD/YYYY");
                return;
            }

            if(!string.IsNullOrEmpty(p.Object.StartDate))
            {
                startrange = Convert.ToDateTime(p.Object.StartDate); 
                Console.WriteLine($"Start Date specified as [{startrange.ToShortDateString()}]");
            }

            if (p.Object.OneDay)
            {
                Console.WriteLine("Single Day specified");
                singleday = true;
            }

            DoWork();
        }

        private static void DoWork()
        {
            int cnt = 0;
            // To be written to file to make sure we do not process the same files multiple times.
            DateTime dtTodaysDate = DateTime.Now;
            Regex regexObj = new Regex(regExString);
            MatchCollection mc = null;
            StringBuilder mystr = new StringBuilder();

            string strnewlog = "";

            string strLastLog = "";
            try
            {
                strLastLog = File.ReadAllText(fnLastRun);
            }
            catch (FileNotFoundException ex)
            {
                strLastLog = "1900-01-01";
            }

            //Reads from "lastlog.txt" to determine if files need to be checked or not.
            string[] fileHolder;
            try
            {
                fileHolder = Directory.GetFiles(log_path, "*");
            }
            catch (Exception ex)
            {
                CMCPPackage p = new CMCPPackage(FunctionKey, DeveloperKey, outbox);
                p.SetJob(JobKey);   //re-use job to group all these packages together

                p.KeyVals.Add("Server", "inthealthserver");
                p.KeyVals.Add("URL", log_path);
                p.KeyVals.Add("Last Run", strLastLog);

                p.Type = CMCPPackage.PackageType.Error;
                p.NoticeCount = 1;
                p.ErrorCount = 1;
                p.Blob = ex.Message;
                p.DT = DateTime.Now;

                p.SaveToFolder();
                return;
            }



            foreach (var file in fileHolder)
            {
                if (singleday) //SINGLEDAY ONLY
                {
                    strLastLog = startrange.ToString();
                    if (startrange.Date == File.GetLastWriteTime(file).Date)
                    {
                        Console.WriteLine("Processing single day file: {0}", file);
                        ReadFile(regexObj, ref strnewlog, ref mc, file, ref mystr, ref cnt); // Call to ReadFile()
                    }
                }
                else //NOT single day
                {
                    if (File.GetLastWriteTime(file) > Convert.ToDateTime(strLastLog)) //CHECK for files to process
                    {
                        Console.WriteLine("Processing new file: {0}", file);
                        ReadFile(regexObj, ref strnewlog, ref mc, file, ref mystr, ref cnt);// Call to ReadFile()
                    }
                }
            }
            //END FOR

            CMCPPackage newPackage = new CMCPPackage(FunctionKey, DeveloperKey, outbox);
            newPackage.SetJob(JobKey);  //re-use job to group all these packages together
            newPackage.KeyVals.Add("Server", "inthealthserver");
            newPackage.KeyVals.Add("URL", log_path);
            newPackage.KeyVals.Add("Last Run", strLastLog);

            if (cnt > 0)
            {
                newPackage.Type = CMCPPackage.PackageType.Warning;
                newPackage.NoticeCount = cnt;
                newPackage.WarningCount = cnt;
                newPackage.Blob = mystr.ToString();

                Console.WriteLine("sending multiple {0}", cnt);
            }
            else
            {
                newPackage.Type = CMCPPackage.PackageType.Info;
                newPackage.NoticeCount = 1;
                newPackage.InfoCount = 1;
                newPackage.Blob = "No Data to Send";
                Console.WriteLine("sending nothing");
            }

            if (singleday)
            {
                newPackage.DT = startrange;
            }
            else
            {
                newPackage.DT = DateTime.Now;
            }
            newPackage.SaveToFolder();

            File.WriteAllText(fnLastRun, dtTodaysDate.ToString());
        }

        /// <summary>
        /// Reads all text from the file. 
        /// looks for warnings in file to create a match collection based on the regular expression
        /// foreach will then iterate through match collection to append to StringBuilder.
        /// </summary>
        /// <param name="regexObj"></param>
        /// <param name="strnewlog"></param>
        /// <param name="mc"></param>
        /// <param name="file"></param>
        private static void ReadFile(Regex regexObj, ref string strnewlog, ref MatchCollection mc, string file, ref StringBuilder mystr, ref int cnt)
        {
            string strFileName = Path.GetFileName(file);
            strnewlog = File.ReadAllText(file);
            mc = regexObj.Matches(strnewlog);
            foreach (Match m in mc)
            {
                mystr.AppendLine(strFileName + ": " + m.ToString() + "\n");
                cnt++;
            }
        }



    }
}
