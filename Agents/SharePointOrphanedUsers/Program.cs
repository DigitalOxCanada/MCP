using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text;
using MCP.Lib;
using Microsoft.SharePoint.Client;

namespace MCP.Agent.Template
{
    class Program
    {
        public static readonly Guid DeveloperKey = new Guid("11111111-1111-1111-1111-111111111111");    //key of the developer responsible for this agent
        public static readonly Guid FunctionKey = new Guid("d7d477de-3a59-4282-8dbf-d84601e3c6fc");      //agent identity key
        static bool foundsome = false;
        public static string outbox = @"\\mcpserver\MCPDropBox\"; //drop box for json files

        static void Main(string[] args)
        {
            DoWork();

        }

        private static void DoWork()
        {
            ClientContext clientContext = new ClientContext("http://intranet");
            //context.Credentials = new NetworkCredential(this.UserName, this.Password, this.Domain);
            GroupCollection groupCollection = clientContext.Web.SiteGroups;
            clientContext.Load(groupCollection, groups => groups.Include(group => group.Users));
            clientContext.ExecuteQuery();

            CMCPPackage p = new CMCPPackage(FunctionKey, DeveloperKey, outbox);

            List<string> ignoreUsersList = new List<string>() { @"NT AUTHORITY\authenticated users",
                @"NT AUTHORITY\LOCAL SERVICE",
                @"SHAREPOINT\system",
                @"DOMAIN1\spfarm",
                @"DOMAIN1\spups",
                @"DOMAIN1\sptest",
                @"DOMAIN1\administrator",
                @"DOMAIN1\domain admins",
                @"DOMAIN1\domain users"
            };

            StringBuilder sbuilder = new StringBuilder();
            int cnt = 0;
            foreach (Group oGroup in groupCollection)
            {
                UserCollection userColl = oGroup.Users;
                foreach (User user in userColl)
                {
                    if (!ignoreUsersList.Contains(user.LoginName))
                    {
                        string s = IsUserInAD(user.LoginName);
                        if (s != "")
                        {
                            cnt++;
                            sbuilder.AppendLine(s);
                        }
                    }
                }
            }

            if (!foundsome)
            {
                CMCPPackage newPackage = new CMCPPackage(FunctionKey, DeveloperKey, outbox);
                newPackage.KeyVals.Add("Server", "intranet");
                newPackage.Blob = "No issues found";
                newPackage.Type = CMCPPackage.PackageType.Info;
                newPackage.DT = DateTime.Now;
                newPackage.NoticeCount = 0;
                newPackage.InfoCount = 0;
                newPackage.SaveToFolder();
            }
            else
            {
                CMCPPackage newPackage = new CMCPPackage(FunctionKey, DeveloperKey, outbox);
                newPackage.KeyVals.Add("Server", "intranet");
                newPackage.Blob = sbuilder.ToString();
                newPackage.Type = CMCPPackage.PackageType.Warning;
                newPackage.DT = DateTime.Now;
                newPackage.NoticeCount = cnt;
                newPackage.WarningCount = cnt;
                newPackage.SaveToFolder();
            }

        }

        static string IsUserInAD(string loginName)
        {
            StringBuilder sbuilder = new StringBuilder();

            using (PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, "DOMAIN1.COM"))
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(domainContext, loginName.ToString()))
                {
                    //Try to find SharePoint user in AD
                    bool userExists = (user != null);
                    if (!userExists)
                    {
                        sbuilder.Append((string)loginName + " does not exist in Active Directory");
                        foundsome = true;
                    }

                    try
                    {
                        DirectorySearcher search = new DirectorySearcher();
                        search.Filter = string.Format("(SAMAccountName={0})", ExtractUserName(loginName));
                        search.PropertiesToLoad.Add("CN");
                        SearchResult result = search.FindOne();
                        DirectoryEntry de = new DirectoryEntry(result.Path);
                        int isDisabled = (int)(de.Properties["userAccountControl"].Value);

                        if (Convert.ToBoolean(isDisabled & 0x0002))
                        {
                            sbuilder.Append((string)loginName + " is disabled in Active Directory");
                            foundsome = true;
                        }
                    }
                    catch { }
                }
            }
            return sbuilder.ToString();
        }

        private static string ExtractUserName(string path)
        {
            string[] userPath = path.Split(new char[] { '\\' });
            return userPath[userPath.Length - 1]; ;
        }
    }
}
