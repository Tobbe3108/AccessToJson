namespace AccessToJson.Models;

public class Database
{
  public string? ConnectionString { get; set; }
  public string? OutputPath { get; set; }
  public List<string>? Tables { get; set; }
}