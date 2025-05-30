using Copist.Services;
using Discord;
using Discord.Audio;
using Discord.Interactions;

namespace Copist.Commands;

public class ConnectVoiceChat : InteractionModuleBase<SocketInteractionContext>
{
    private static IAudioClient? _staticAudioClient;
    private readonly SqliteService _sqliteService;

    public ConnectVoiceChat(SqliteService sqliteService)
    {
        _sqliteService = sqliteService;
    }

    [SlashCommand("connect", "Connect to a voice channel for transcription.", runMode: RunMode.Async)]
    public async Task ConnectAsync()
    {
        try
        {
            var voiceChannel = DiscordService.LocateUserVoiceChannel(Context.Guild, Context.User.Id);

            if (voiceChannel == null)
            {
                await RespondAsync("You are not connected to a voice channel.", ephemeral: true);
                return;
            }

            if (_staticAudioClient != null && _staticAudioClient.ConnectionState == ConnectionState.Connected)
            {
                await RespondAsync("Already connected to a voice channel.", ephemeral: true);
                return;
            }

            Console.WriteLine("Connecting to voice channel: " + voiceChannel.Name);

            var staticAudioClient = await voiceChannel.ConnectAsync();

            //_ = KeepAliveAudioHandler(_staticAudioClient);

            await RespondAsync($"Connected to voice channel: {voiceChannel.Name}", ephemeral: true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task KeepAliveAudioHandler(IAudioClient client)
    {
        Console.WriteLine("Starting audio stream handler...");

        client.Disconnected += (exception) =>
        {
            Console.WriteLine($"Disconnected ! Reason : {exception?.Message ?? "uknown"}");
            return Task.CompletedTask;
        };
        using var audioStream = client.CreatePCMStream(AudioApplication.Voice);
        try
        {
            while (true)
            {
                await audioStream.WriteAsync(new byte[1920], 0, 1920);
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur dans le stream audio : {ex.Message}");
        }
    }
}