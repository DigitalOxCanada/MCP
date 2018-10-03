using System;
using System.Net;
using System.Net.NetworkInformation;
using MCP.Lib;

namespace MCP.Agent.Template
{
    class Program
    {
        public static readonly Guid DeveloperKey = new Guid("11111111-1111-1111-1111-111111111111");    //key of the developer responsible for this agent
        public static readonly Guid FunctionKey = new Guid("a2cad9b8-bb58-4057-b729-5c11245b8071");      //agent identity key

        static void Main(string[] args)
        {
            string outbox = @"\\mcpserver\MCPDropBox\";   //drop box for json files

            Guid JobKey = Guid.NewGuid();   //use for grouping packages into job chunks

            string ip = "10.10.10.150";
            CMCPPackage p = new CMCPPackage(FunctionKey, DeveloperKey, outbox);
            p.SetJob(JobKey);

            Ping x = new Ping();
            PingReply reply = x.Send(IPAddress.Parse(ip));

            p.KeyVals.Add("GHC Server", ip);
            if (reply.Status == IPStatus.Success)
            {
                p.Type = CMCPPackage.PackageType.Info;
                p.NoticeCount = 1;
                p.InfoCount = 1;
                p.Blob = "Connection OK to " + ip;
                p.DT = DateTime.Now;

                p.SaveToFolder();
                Console.WriteLine("Connection OK");
            }
            else
            {
                Console.WriteLine("Connection not OK");
                p.Type = CMCPPackage.PackageType.Error;
                p.NoticeCount = 1;
                p.ErrorCount = 1;
                p.Blob = "Can't connect to " + ip;
                p.DT = DateTime.Now;

                p.SaveToFolder();
            }

        }
    }
}
