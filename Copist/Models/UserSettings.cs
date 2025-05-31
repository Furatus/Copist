using System.ComponentModel;
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
        var properties = GetType().GetProperties();
        var result = new List<string>();
    
        foreach (var prop in properties)
        {
            var value = prop.GetValue(this);
            result.Add($"{prop.Name}: {value}\n");
        }
    
        return string.Join("", result);
    }
    
    public enum SettingsType
    {
        [Description("string")]
        DefaultLanguage,
        
        [Description("bool")]
        IsAllowingRecording,
        
        [Description("bool")]
        IsCopistFollowingWhenLeading
    }
    
    public static string GetExpectedTypeForProperty(SettingsType type)
    {
        var memberInfo = typeof(SettingsType).GetMember(type.ToString());
        var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        
        if (attributes.Length > 0)
        {
            return ((DescriptionAttribute)attributes[0]).Description;
        }
        
        return "string";
    }
}