using AccessToJson.Models;

namespace AccessToJson;

public record DatabaseOptions
{
  public string? MailCmdPath { get; set; }
  public List<Database>? Databases { get; set; }
}