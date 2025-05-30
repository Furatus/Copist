using System.Data.SQLite;
using Copist.Models;
using Dapper;
using Newtonsoft.Json;

namespace Copist.Services;

public class SqliteService
{
    private readonly string _connectionString = Environment.GetEnvironmentVariable("SQLiteConnectionString") ?? "Data Source=./Database/Copist.db;";
    public SqliteService()
    {
        InitializeDatabase();
    }
    
    private SQLiteConnection getConnection()
    {
        return new SQLiteConnection(_connectionString);
    }
    
    private void InitializeDatabase()
    {
        if (!Directory.Exists("./Database")) Directory.CreateDirectory("./Database");
        
        if (!File.Exists("./Database/Copist.db"))
        {
            SQLiteConnection.CreateFile("./Database/Copist.db");
        }
        
        using var connection = getConnection();
        connection.Open();

        // Create UserSettings table if it doesn't exist
        var createUserSettingsTable = @"
            CREATE TABLE IF NOT EXISTS UserSettings (
                userId TEXT PRIMARY KEY,
                defaultLanguage TEXT DEFAULT 'en-US',
                isAllowingRecording BOOLEAN DEFAULT 0,
                isCopistFollowingWhenLeading BOOLEAN DEFAULT 0
            )";
        connection.Execute(createUserSettingsTable);

        // Create GuildSettings table if it doesn't exist
        var createGuildSettingsTable = @"
            CREATE TABLE IF NOT EXISTS GuildSettings (
                guildId TEXT PRIMARY KEY,
                defaultServerLanguage TEXT DEFAULT 'en-US',
                isLeavingWhenLeaderLeaves BOOLEAN DEFAULT 1,
                defaultTranscriptionChannelId TEXT DEFAULT '',
                isTextTranscribedInThread BOOLEAN DEFAULT 1
            )";
        connection.Execute(createGuildSettingsTable);
        
        // Create TranscribeInstances table if it doesn't exist
        var createTranscribeInstancesTable = @"
            CREATE TABLE IF NOT EXISTS TranscribeInstances (
                InstanceId TEXT PRIMARY KEY,
                GuildId TEXT NOT NULL,
                VoiceChannelId TEXT NOT NULL,
                TranscriptionChannelId TEXT NOT NULL,
                LanguagesTranscriptionList TEXT,
                TranscribedUsersList TEXT,
                StartTime DATETIME DEFAULT CURRENT_TIMESTAMP
            )";
        connection.Execute(createTranscribeInstancesTable);
        
        connection.Close();
    }
    public UserSettings? GetUserSettings(string userId)
    {
        using var connection = getConnection();
        connection.Open();
        
        var getSettingsSql = "SELECT * FROM UserSettings WHERE userId = @userId";
        
        var row = connection.QueryFirstOrDefault(getSettingsSql, new { userId });
        if (row == null)
        {
            connection.Close();
            return null;
        }
        
        var userSettings = new UserSettings
        {
            userId = row.userId,
            defaultLanguage = row.defaultLanguage,
            isAllowingRecording = row.isAllowingRecording,
            isCopistFollowingWhenLeading = row.isCopistFollowingWhenLeading
        };
        
        connection.Close();
        return userSettings;
    }

    public GuildSettings GetGuildSettings(string guildId)
    {
        using var connection = getConnection();
        connection.Open();

        var sql = "SELECT * FROM GuildSettings WHERE guildId = @guildId";
        
        connection.Close();
        return connection.QueryFirstOrDefault<GuildSettings>(sql, new { guildId });
    }
    
    public void SaveUserSettings(UserSettings userSettings)
    {
        using var connection = getConnection();
        connection.Open();
        
        var sql = @"
            INSERT OR REPLACE INTO UserSettings (userId, defaultLanguage, isAllowingRecording, isCopistFollowingWhenLeading)
            VALUES (@userId, @defaultLanguage, @isAllowingRecording, @isCopistFollowingWhenLeading)";
        
        connection.Execute(sql, userSettings);
        connection.Close();
    }
    
    public void SaveGuildSettings(GuildSettings guildSettings)
    {
        using var connection = getConnection();
        connection.Open();
        
        var sql = @"
            INSERT OR REPLACE INTO GuildSettings (guildId, defaultServerLanguage, isLeavingWhenLeaderLeaves, defaultTranscriptionChannelId, isTextTranscribedInThread)
            VALUES (@guildId, @defaultServerLanguage, @isLeavingWhenLeaderLeaves, @defaultTranscriptionChannelId, @isTextTranscribedInThread)";
        
        connection.Execute(sql, guildSettings);
        connection.Close();
    }
    
    public void NewTranscribeInstance(TranscribeInstance transcribeInstance)
    {
        using var connection = getConnection();
        connection.Open();
        
        var sql = @"
            INSERT INTO TranscribeInstances (InstanceId, GuildId, VoiceChannelId, TranscriptionChannelId, LanguagesTranscriptionList, TranscribedUsersList, StartTime)
            VALUES (@InstanceId, @GuildId, @VoiceChannelId, @TranscriptionChannelId, @LanguagesTranscriptionList, @TranscribedUsersList, @StartTime)";
        
        var serializedTranscribeInstance = new {
            InstanceId = transcribeInstance.InstanceId.ToString(),
            GuildId = transcribeInstance.GuildId,
            VoiceChannelId = transcribeInstance.VoiceChannelId,
            TranscriptionChannelId = transcribeInstance.TranscriptionChannelId,
            LanguagesTranscriptionList = JsonConvert.SerializeObject(transcribeInstance.LanguagesTranscriptionList),
            TranscribedUsersList = JsonConvert.SerializeObject(transcribeInstance.TranscribedUsersList),
            StartTime = transcribeInstance.StartTime
        };
        
        connection.Execute(sql, serializedTranscribeInstance);
        
        connection.Close();
    }

    public void AddUserToTranscribeInstance(Guid instanceId, string userId)
    {
        using var connection = getConnection();
        connection.Open();

        var getExistingUsersListSql = @"SELECT TranscribedUsersList FROM TranscribeInstances WHERE InstanceId = @InstanceId";

        var row = connection.QueryFirstOrDefault(getExistingUsersListSql, new { InstanceId = instanceId });

        string existingUsersListJson = row?.TranscribedUsersList ?? "[]";

        var users = JsonConvert.DeserializeObject<List<string>>(existingUsersListJson);

        if (users != null && !users.Contains(userId))
            users.Add(userId);

        string updatedUsersListJson = JsonConvert.SerializeObject(users);

        var updateUserListSql = @"UPDATE TranscribeInstances SET TranscribedUsersList = @TranscribedUsersList WHERE InstanceId = @InstanceId;";

        connection.Execute(updateUserListSql,
            new { TranscribedUsersList = updatedUsersListJson, InstanceId = instanceId });
        connection.Close();
    }
    
    public TranscribeInstance? GetTranscribeInstance(Guid instanceId)
    {
        using var connection = getConnection();
        connection.Open();

        var getInstanceSql = "SELECT * FROM TranscribeInstances WHERE InstanceId = @InstanceId";
        
        var row = connection.QueryFirstOrDefault(getInstanceSql, new { InstanceId = instanceId.ToString() });
        if (row == null)
        {
            connection.Close();
            return null;
        }
        
        Console.WriteLine(row.InstanceId.ToString());
        
        var instance = new TranscribeInstance
        {
            InstanceId = Guid.Parse(row.InstanceId.ToString()),
            GuildId = row.GuildId,
            VoiceChannelId = row.VoiceChannelId,
            TranscriptionChannelId = row.TranscriptionChannelId,
            LanguagesTranscriptionList = JsonConvert.DeserializeObject<List<string>>(row.LanguagesTranscriptionList ?? "[]"),
            TranscribedUsersList = JsonConvert.DeserializeObject<List<string>>(row.TranscribedUsersList ?? "[]"),
            StartTime = row.StartTime
        };
        connection.Close();
        
        return instance;
    }
    
    public int getTranscribeInstancesCount()
    {
        using var connection = getConnection();
        connection.Open();

        var sql = "SELECT COUNT(*) FROM TranscribeInstances";
        
        var count = connection.ExecuteScalar<int>(sql);
        connection.Close();
        
        return count;
    }
}