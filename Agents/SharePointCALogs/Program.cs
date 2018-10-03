using System;
using System.Text;
using System.Text.RegularExpressions;
using MCP.Lib;
using Microsoft.SharePoint.Client;

namespace MCP.Agent.Template
{
    class Program
    {
        public static readonly Guid DeveloperKey = new Guid("11111111-1111-1111-1111-111111111111");    //key of the developer responsible for this agent
        public static readonly Guid FunctionKey = new Guid("01021458-8b6f-4ef4-a0b3-7b0f6ee81c5d");      //agent identity key

        static void Main(string[] args)
        {
            string outbox = @"\\mcpserver\MCPDropBox\";

            Guid JobKey = Guid.NewGuid();

            ClientContext clientContext = new ClientContext("http://spserver:28765/");
            Regex regex = new Regex("4(.)-(.)Success");
            List list = clientContext.Web.Lists.GetByTitle("Review problems and solutions");
            clientContext.Load(list);
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = "<View>All Reports</View>";
            ListItemCollection listItems = list.GetItems(camlQuery);
            clientContext.Load(listItems);
            clientContext.ExecuteQuery();

            foreach (ListItem listItem in listItems)
            {
                StringBuilder sbuilder = new StringBuilder();
                Match m = regex.Match(listItem["HealthReportSeverity"].ToString());
                if (!m.Success)
                {
                    sbuilder.AppendLine(listItem["Title"].ToString());
                    sbuilder.AppendLine("Description: " + listItem["HealthReportExplanation"].ToString());
                    sbuilder.AppendLine("Remedy: " + listItem["HealthReportRemedy"].ToString());

                    CMCPPackage p = new CMCPPackage(FunctionKey, DeveloperKey, outbox);
                    p.SetJob(JobKey);   //re-use job to group all these packages together

                    p.KeyVals.Add("Severity", listItem["HealthReportSeverity"].ToString());
                    p.KeyVals.Add("Failing Services", listItem["HealthReportServices"].ToString());
                    try
                    {
                        p.KeyVals.Add("Failing Servers", listItem["HealthReportServers"].ToString());
                    }
                    catch
                    {

                    }
                    p.KeyVals.Add("Failure Message Date", listItem["Modified"].ToString());

                    switch (listItem["HealthReportSeverity"].ToString())
                    {
                        case "1 - Error":
                            p.Type = CMCPPackage.PackageType.Error;
                            p.ErrorCount = 1;
                            break;
                        case "2 - Warning":
                            p.Type = CMCPPackage.PackageType.Warning;
                            p.WarningCount = 1;
                            break;
                        default:
                            p.Type = CMCPPackage.PackageType.Info;
                            p.InfoCount = 1;
                            break;
                    }
                    p.NoticeCount = 1;
                    p.Blob = sbuilder.ToString();
                    p.DT = DateTime.UtcNow;

                    p.SaveToFolder();
                }

            }

        }
    }
}
