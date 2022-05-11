using System.Data;
using System.Data.OleDb;
using AccessToJson;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

var configurationRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("./appsettings.json").Build();
var databasePath = configurationRoot["DatabasePath"];
var outputPath = configurationRoot["OutputPath"];
new DirectoryInfo(outputPath).Create();

await using var con = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={databasePath};");
await con.OpenAsync();
var tasks = new List<Task>
{
    ReadModelToFile<TabBatch>(con),
    ReadModelToFile<TabFejl>(con),
    ReadModelToFile<TabRulle>(con)
};
await Task.WhenAll(tasks);
await con.CloseAsync();

Console.WriteLine("All done!");
Environment.Exit(0);

async Task ReadModelToFile<T>(IDbConnection oleDbConnection)
{
    Console.WriteLine($"Exporting {typeof(T).Name}");
    var data = (await oleDbConnection.QueryAsync<T>($"select * from {typeof(T).Name}")).ToList();
    Console.WriteLine($"Found {data.Count} rows in {typeof(T).Name}");

    var filePath = Path.Combine(outputPath, $"{typeof(T).Name}.json");
    await using var file = File.CreateText(filePath);
    var serializer = new JsonSerializer
    {
        DateFormatString = "dd-MM-yyyy HH:mm:ss",
        Converters = { new StringJsonConverter() }
    };
    serializer.Serialize(file, new Dictionary<string, List<T>> { { typeof(T).Name, data } });
}