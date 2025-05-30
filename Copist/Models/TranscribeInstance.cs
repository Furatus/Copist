namespace Copist.Models;

public class TranscribeInstance
{
    public Guid InstanceId { get; set; } = Guid.NewGuid();
    public string GuildId { get; set; } = string.Empty;
    public string VoiceChannelId { get; set; } = string.Empty;
    public string TranscriptionChannelId { get; set; } = string.Empty;
    public List<string>? LanguagesTranscriptionList { get; set; } = null;
    public List<string>? TranscribedUsersList { get; set; } = null;
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
}