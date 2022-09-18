using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using AccessToJson;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

string outputPath;
string mailCmdPath = null!;

try
{
    var configurationRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("./appsettings.json")
        .Build();

    var databasePath = configurationRoot["DatabasePath"];
    var medarbejderDatabasePath = configurationRoot["MedarbejderDatabasePath"];
    outputPath = configurationRoot["OutputPath"];
    mailCmdPath = configurationRoot["MailCmdPath"];

    ArgumentNullException.ThrowIfNull(databasePath, "databasePath is not set in appSettings");
    ArgumentNullException.ThrowIfNull(medarbejderDatabasePath, "medarbejderDatabasePath is not set in appSettings");
    ArgumentNullException.ThrowIfNull(outputPath, "outputPath is not set in appSettings");
    ArgumentNullException.ThrowIfNull(mailCmdPath, "mailCmdPath is not set in appSettings");

    new DirectoryInfo(outputPath).Create();

    await using var con = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={databasePath};");
    await using var medCon =
        new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={medarbejderDatabasePath};");

    var conOpenTask = con.OpenAsync();
    var medConOpenTask = medCon.OpenAsync();
    await conOpenTask;
    await medConOpenTask;

    var tasks = new List<Task>
    {
        ReadModelToFile<TabBatch>(con),
        ReadModelToFile<TabFejl>(con),
        ReadModelToFile<TabRulle>(con),
        ReadModelToFile<TblLevering>(con),
        ReadModelToFile<TabMedarbejder>(medCon)
    };
    await Task.WhenAll(tasks);

    var conCloseTask = con.CloseAsync();
    var medConCloseTask = medCon.CloseAsync();
    await conCloseTask;
    await medConCloseTask;

    Console.WriteLine("All done!");
    Environment.Exit(0);
}
catch (Exception e)
{
    Console.WriteLine(e);
    try
    {
        Console.WriteLine("Sending email");
        Process.Start(mailCmdPath);
        Console.WriteLine("email sent");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        Environment.Exit(0);
    }
    Environment.Exit(0);
}

async Task ReadModelToFile<T>(IDbConnection oleDbConnection)
{
    var dataTask = oleDbConnection.QueryAsync<T>($"select * from {typeof(T).Name}");

    Console.WriteLine($"Exporting {typeof(T).Name}");
    var data = (await dataTask).ToList();
    Console.WriteLine($"Found {data.Count} rows in {typeof(T).Name}");

    var filePath = Path.Combine(outputPath, $"{typeof(T).Name}.json");
    await using var file = File.CreateText(filePath);
    var serializer = new JsonSerializer
    {
        DateFormatString = "dd-MM-yyyy HH:mm:ss", Converters = { new StringJsonConverter() }
    };
    serializer.Serialize(file, new Dictionary<string, List<T>> { { typeof(T).Name, data } });
}