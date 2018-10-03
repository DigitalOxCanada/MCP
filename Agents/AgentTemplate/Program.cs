using System;
using MCP.Lib;

namespace MCP.Agent.Template
{
    class Program
    {
        public static readonly Guid DeveloperKey = new Guid("11111111-1111-1111-1111-111111111111");    //key of the developer responsible for this agent
        public static readonly Guid FunctionKey = new Guid("$guid2$");      //agent identity key

        static void Main(string[] args)
        {
            string outbox = @"\\mcpserver\MCPDropBox\";   //drop box for json files

            Guid JobKey = Guid.NewGuid();   //use for grouping packages into job chunks

            CMCPPackage p = new CMCPPackage(FunctionKey, DeveloperKey, outbox);
            p.SetJob(JobKey);   //re-use job to group all these packages together

            p.KeyVals.Add("Failure Message", "something here");

            p.Type = CMCPPackage.PackageType.Info;
            p.NoticeCount = 1;
            p.Blob = "some info here";
            p.DT = DateTime.Now;

            p.SaveToFolder();

        }
    }
}
