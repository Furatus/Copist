using System.Data.SQLite;
using System.Reflection;
using Copist.Models;
using Copist.Services;
using Discord.Net;
using Discord;
using Dapper;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

public class Program
{
    private DiscordSocketClient _client;
    private CommandService _commands;
    private IServiceProvider _services;
    public static async Task Main(string[] args)
    {
        /*try {
            Console.WriteLine("Attempting to create an instance of TranscribeInstance On Database...");

            var sqliteService = new SqliteService();
            
            var instanceId = Guid.NewGuid();

            var transcribeInstance = new TranscribeInstance
            {
                InstanceId = instanceId,
                GuildId = "123456789012345678",
                VoiceChannelId = "234567890123456789",
                TranscriptionChannelId = "345678901234567890",
                LanguagesTranscriptionList = new List<string> { "en-US", "fr-FR" },
                TranscribedUsersList = new List<string> { "User1", "User2" }
            };

            sqliteService.NewTranscribeInstance(transcribeInstance);
            
            Console.WriteLine("Success.");
            
            Console.WriteLine("Trying to add a user to the TranscribeInstance...");
            
            sqliteService.AddUserToTranscribeInstance(instanceId, "User3");
            Console.WriteLine("User added successfully.");
            
            Console.WriteLine("Trying to retrieve the TranscribeInstance...");
            
            var retrievedInstance = sqliteService.GetTranscribeInstance(instanceId);
            
            if (retrievedInstance != null) {
                Console.WriteLine($"Retrieved TranscribeInstance: {JsonConvert.SerializeObject(retrievedInstance, Formatting.Indented)}");
            } else {
                Console.WriteLine("TranscribeInstance not found.");
            }
            
        }
        catch (Exception ex) {
            Console.WriteLine("Failed to do operations on TranscribeInstance:");
            Console.WriteLine(ex);
        }*/
        
        DotNetEnv.Env.Load("./.env");
        var program = new Program();
        await program.RunBotAsync();
    }

    public async Task RunBotAsync()
    {
        _client = new DiscordSocketClient();
        _commands = new CommandService();
        _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton<SqliteService>()
            .BuildServiceProvider();
        
        string? token = Environment.GetEnvironmentVariable("discordToken");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Missing Discord Bot Token, check your .env file.");
            return;
        }
        
        string? guildIdStr = Environment.GetEnvironmentVariable("TestGuildId");
        if (!ulong.TryParse(guildIdStr, out var testGuildId))
        {
            Console.WriteLine("misssing or invalid Test Guild Id.");
            return;
        }
        var interactionService = new InteractionService(_client);

        _client.Ready += async () =>
        {
            await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await interactionService.RegisterCommandsGloballyAsync();
            await interactionService.RegisterCommandsToGuildAsync(testGuildId);
        };
        
        _client.InteractionCreated += async (interaction) =>
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await interactionService.ExecuteCommandAsync(ctx, _services);
        };

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        
        Console.WriteLine("Copist is running.");
        await Task.Delay(-1);
        
        
    }
}