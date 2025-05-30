using Newtonsoft.Json;

namespace Copist.Models;

public class UserSettings
{
    public string userId { get; set; } = string.Empty;
    public string defaultLanguage { get; set; } = "en-US";
    public bool isAllowingRecording { get; set; } = false;
    public bool isCopistFollowingWhenLeading { get; set; } = false;


    public override string? ToString()
    {
        if (this == null) return null;
        
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