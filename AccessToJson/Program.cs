#pragma warning disable CA1416

using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics;
using AccessToJson;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
  .AddJsonFile("./appsettings.json")
  .Build();

var databaseOptions = configuration.Get<DatabaseOptions>();
ArgumentNullException.ThrowIfNull(databaseOptions?.MailCmdPath, "mailCmdPath is not set in appSettings");

try
{
  var tasks = new List<Task>();
  var connections = new List<OleDbConnection>();

  ArgumentNullException.ThrowIfNull(databaseOptions.Databases, "Databases is not set in appSettings");
  foreach (var database in databaseOptions.Databases)
  {
    ArgumentNullException.ThrowIfNull(database.ConnectionString,
      "ConnectionString is not set for database in appSettings");
    ArgumentNullException.ThrowIfNull(database.OutputPath, "OutputPath is not set for database in appSettings");

    if (Directory.Exists(database.OutputPath) is false) Directory.CreateDirectory(database.OutputPath);

    var connection = new OleDbConnection(database.ConnectionString);
    connections.Add(connection);

    await connection.OpenAsync();

    ArgumentNullException.ThrowIfNull(database.Tables, "Tables is not set for database in appSettings");
    tasks.AddRange(database.Tables.Select(tableName => ReadModelToFile(connection, tableName, database.OutputPath)));
  }

  Console.WriteLine("Waiting for tasks to complete");
  await Task.WhenAll(tasks);

  Console.WriteLine("Closing connections");
  await Task.WhenAll(connections.Select(connection => connection.CloseAsync()));

  Console.WriteLine("All done!");
}
catch (Exception e)
{
  Console.WriteLine(e);
  try
  {
    Console.WriteLine("Sending email");
    var processInfo = new ProcessStartInfo(databaseOptions.MailCmdPath)
    {
      WorkingDirectory = new FileInfo(databaseOptions.MailCmdPath).DirectoryName
    };
    var processStart = Process.Start(processInfo);
    await processStart!.WaitForExitAsync();
    Console.WriteLine("Email sent");
  }
  catch (Exception ex)
  {
    Console.WriteLine(ex);
  }
}

async Task ReadModelToFile(DbConnection oleDbConnection, string tableName, string outputPath)
{
  var databaseName = GetDatabaseName(oleDbConnection);

  var dataTask = oleDbConnection.QueryAsync<dynamic>($"select * from {tableName}");

  Console.WriteLine($"Exporting {databaseName}_{tableName}");
  var data = (await dataTask).ToList();
  Console.WriteLine($"Found {data.Count} rows in {databaseName}_{tableName}");

  var filePath = Path.Combine(outputPath, $"{databaseName}_{tableName}.json");
  await using var file = File.CreateText(filePath);
  var serializer = new JsonSerializer
  {
    DateFormatString = "dd-MM-yyyy HH:mm:ss", Converters = { new StringJsonConverter() }
  };
  serializer.Serialize(file, new Dictionary<string, List<dynamic>> { { tableName, data } });
}

string GetDatabaseName(DbConnection dbConnection)
{
  var dataSource = dbConnection.DataSource;
  var databaseName = dataSource.Split('\\').Last().Split('.').First();
  return databaseName;
}

#pragma warning restore CA1416