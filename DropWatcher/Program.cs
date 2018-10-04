using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using CommandLineParser.Core;

namespace MCP.Utils.DropWatcher
{
    class Program
    {
        public const string processedPath = "outbox";
        public static string MCPDropBoxPath = "";

        public class CommandObject
        {
            public string Path { get; set; }
        }

        static void Main(string[] args)
        {
            var p = new FluentCommandLineParser<CommandObject>();
            p.Setup(arg => arg.Path)
                .As('p', "path") // define the short and long option name
                .Required(); // using the standard fluent Api to declare this Option as required.

            var result = p.Parse(args);
            if (result.HasErrors == true)
            {
                Console.WriteLine("You must specify the dropbox path using -p");
                return;
            }
            MCPDropBoxPath = p.Object.Path;
            Directory.CreateDirectory(Path.Combine(MCPDropBoxPath, processedPath));

            FileSystemWatcher fsw = new FileSystemWatcher(MCPDropBoxPath, "*.json")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };
            fsw.Created += Fsw_Created;
            fsw.Changed += Fsw_Changed;

            fsw.EnableRaisingEvents = true;

            Console.WriteLine("Press \'q\' to quit.");
            while (Console.Read() != 'q') ;

        }

        static void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File: " + e.FullPath + " (onChanged) " + e.ChangeType);
            if (File.Exists(e.FullPath))
            {
                string jsonstr = File.ReadAllText(e.FullPath);

                var connectionString = "mongodb://mcpserver";
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("mcp");
                var collection = database.GetCollection<BsonDocument>("packages");

                BsonDocument bd = BsonSerializer.Deserialize<BsonDocument>(jsonstr);

                collection.InsertOneAsync(bd).Wait();

                File.Move(e.FullPath, Path.Combine(MCPDropBoxPath, processedPath, e.Name));
            }
        }

        static void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File: " + e.FullPath + " (onCreated) " + e.ChangeType);

        }

    }
}
