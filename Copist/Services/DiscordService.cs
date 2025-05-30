using Discord;
using Discord.WebSocket;

namespace Copist.Services;

public class DiscordService
{
    public static IVoiceChannel? LocateUserVoiceChannel(SocketGuild guild, ulong userId)
    {
        var user = guild.GetUser(userId);
        return user?.VoiceChannel;
    }
}