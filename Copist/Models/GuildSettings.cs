using Newtonsoft.Json;

namespace Copist.Models;

public class GuildSettings
{
    public string GuildId { get; set; } = string.Empty;
    public string DefaultServerLanguage { get; set; } = "en-US";
    public bool IsLeavingWhenLeaderLeaves { get; set; } = true;
    public string DefaultTranscriptionChannelId { get; set; } = string.Empty;
    public bool IsTextTranscribedInThread { get; set; } = true;
    
    public override string? ToString()
    {
        var properties = GetType().GetProperties();
        var result = new List<string>();
    
        foreach (var prop in properties)
        {
            var value = prop.GetValue(this);
            result.Add($"{prop.Name}: {value}\n");
        }
    
        return string.Join("", result);
    }
}