using Discord.Interactions;
using System.Threading.Tasks;
using Copist.Services;
using Discord;

namespace Copist.Commands;

public class StatusModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly SqliteService _sqliteService;

    public StatusModule(SqliteService sqliteService)
    {
        _sqliteService = sqliteService;
    }
    
    [SlashCommand("status", "Get the current status of the bot.")]
    public async Task StatusAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Copist Status")
            .WithDescription("The bot is currently running and ready to assist you.")
            .WithColor(Color.Green)
            .WithCurrentTimestamp()
            .WithFooter($"Currently running : {_sqliteService.getTranscribeInstancesCount()} instances of transcription.")
            .Build();
        
        Console.WriteLine("Status command invoked by user: " + Context.User.Username);
        
        await RespondAsync(embed: embed, ephemeral: true);
    }
    
    [SlashCommand("threadtest", "Crée un thread de test.")]
    public async Task ThreadTestAsync()
    {
        // Vérifie que le canal est un texte
        if (Context.Channel is ITextChannel textChannel)
        {
            // Crée un thread public nommé "Nouveau thread"
            var thread = await textChannel.CreateThreadAsync(
                name: "Nouveau thread",
                autoArchiveDuration: ThreadArchiveDuration.OneHour,
                type: ThreadType.PublicThread
            );
            await thread.SendMessageAsync("<@270595136466059264> :wave: Transcription Started in this thread.");

            await RespondAsync($"Thread créé : {thread.Name}", ephemeral: true);
        }
        else
        {
            await RespondAsync("Cette commande doit être utilisée dans un salon texte.", ephemeral: true);
        }
    }
}