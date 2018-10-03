using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace MCP.Utils.DropWatcher
{
    class Program
    {
        public const string processedPath = "outbox";
        public class Entity
        {
            public ObjectId Id { get; set; }

            public string Name { get; set; }
        }

        static void Main(string[] args)
        {
            //make sure out path exists
            if (!Directory.Exists(processedPath))
            {
                Directory.CreateDirectory(processedPath);
            }

            FileSystemWatcher fsw = new FileSystemWatcher(".", "*.json");
            fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            fsw.Created += fsw_Created;
            fsw.Changed += fsw_Changed;

            fsw.EnableRaisingEvents = true;

            Console.WriteLine("Press \'q\' to quit.");
            while (Console.Read() != 'q') ;

        }

        static void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File: " + e.FullPath + " (onChanged) " + e.ChangeType);
            if (File.Exists(e.FullPath))
            {
                string jsonstr = File.ReadAllText(e.FullPath);

                var connectionString = "mongodb://mcpserver";
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("MCP");
                var collection = database.GetCollection<BsonDocument>("packages");

                BsonDocument bd = BsonSerializer.Deserialize<BsonDocument>(jsonstr);

                collection.InsertOneAsync(bd).Wait();

                File.Move(e.FullPath, processedPath + "/" + e.Name);
            }
        }

        static void fsw_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File: " + e.FullPath + " (onCreated) " + e.ChangeType);

        }

    }
}
