using System.Data;
using System.Data.OleDb;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Configuration;


if (args.Any() is false)
{
    Console.WriteLine("Tables not defined.. add like this: .exe table1,table2");
    return;
}

var tables = (string)args.GetValue(0)!;

var configurationRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("./appsettings.json").Build();
var databasePath = configurationRoot["DatabasePath"];
var outputPath = configurationRoot["OutputPath"];

await using (var con = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={databasePath};"))
{
    await con.OpenAsync();

    await Parallel.ForEachAsync(tables.Split(','), new ParallelOptions { MaxDegreeOfParallelism = 5 },
        async (table, token) =>
        {
            Console.WriteLine($"Exporting {table}");
            var data = (await con.QueryAsync($"select * from {table}")).ToList();
            Console.WriteLine($"Found {data.Count} rows in {table}");
            await using var fileStream = File.Create(Path.Combine(outputPath, $"{table}.json"));
            JsonSerializer.Serialize(fileStream, data);
        });

    await con.CloseAsync();
}

Console.WriteLine("All done!");
Environment.Exit(0);