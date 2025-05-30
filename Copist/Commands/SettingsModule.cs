using Copist.Services;
using Discord;
using Discord.Interactions;

namespace Copist.Commands;

public class SettingsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly SqliteService _sqliteService;

    public SettingsModule(SqliteService sqliteService)
    {
        _sqliteService = sqliteService;
    }

    public enum SettingsType
    {
        User,
        Guild
    }

    [SlashCommand("settings", "Get the current options of the bot.")]
    public async Task GetOptionsAsync(SettingsType type)
    {
        try
        {
            string? settings;
            switch (type)
            {
                case SettingsType.Guild:
                    var guildSettings = _sqliteService.GetGuildSettings(Context.Guild.Id.ToString()).ToString();
                    settings = guildSettings?.ToString() ?? null;
                    if (settings == null)
                    {
                        await RespondAsync("No settings found is database for this guild.");
                        return;
                    }
                    break; 
                
                case SettingsType.User:
                    var userSettings = _sqliteService.GetUserSettings(Context.User.Id.ToString());
                    settings = userSettings?.ToString() ?? null;
                    if (settings == null)
                    {
                        await RespondAsync("No settings found is database for this user.");
                        return;
                    }
                    
                    break;
                
                default:
                    settings = null;
                    break;
            }

            if (settings == null)
            {
                await RespondAsync("No options found for the specified type.", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle($"{type} Options")
                .WithDescription($"Current options for {type}:\n {settings}")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .Build();

            await RespondAsync(embed: embed, ephemeral: true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}