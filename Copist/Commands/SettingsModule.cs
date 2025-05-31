using Copist.Models;
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
                    var guildSettings = _sqliteService.GetGuildSettings(Context.Guild.Id.ToString());
                    settings = guildSettings?.ToString() ?? null;
                    if (settings == null)
                    {
                        await RespondAsync("No settings found in database for this guild.");
                        return;
                    }
                    break; 
                
                case SettingsType.User:
                    var userSettings = _sqliteService.GetUserSettings(Context.User.Id.ToString());
                    settings = userSettings?.ToString() ?? null;
                    if (settings == null)
                    {
                        await RespondAsync("No settings found in database for this user.");
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
    
    [SlashCommand("setuseroption", "Set a new value to a single user setting option.")]
    public async Task SetUserOptionAsync(
        [Summary("option", "Option to set")] UserSettings.SettingsType property, 
        [Summary("valeur", "Value to attribute")] string value)
    {
        try
        {
            // Vérifie si l'utilisateur existe déjà dans la base de données
            var userSettings = _sqliteService.GetUserSettings(Context.User.Id.ToString());
            if (userSettings == null)
            {
                // Crée un nouvel utilisateur avec les paramètres par défaut
                userSettings = new UserSettings { userId = Context.User.Id.ToString() };
                _sqliteService.SaveUserSettings(userSettings);
            }

            _sqliteService.SaveSingleUserSetting(Context.User.Id.ToString(), property, value);
        
            await RespondAsync($"Updated {property} with success !", ephemeral: true);
        }
        catch (ArgumentException ex)
        {
            await RespondAsync($"Error: {ex.Message}", ephemeral: true);
        }
        catch (Exception e)
        {
            Console.Write(e);
            await RespondAsync("We ran into an issue when updating your settings.", ephemeral: true);
        }
    }
}