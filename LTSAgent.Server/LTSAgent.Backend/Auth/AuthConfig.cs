using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic;
using LTSAgent.Backend.Core;

namespace LTSAgent.Backend.Auth;

public class AuthConfig
{
    private readonly string ConfigPath = Path.Combine(AgentPaths.UserConfigDir, "AuthConfig.json");
    
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    
    public string ApiKey {get; private set; }
    
    public AnthropicClient Client { get; private set; }

    public bool IsApiKeyConfigured() => !string.IsNullOrWhiteSpace(ApiKey);
    
    public void SetApiKey(string Key)
    {
        ApiKey = Key;
        Save();
    }
    
    private void Save()
    {
        string Dir = Path.GetDirectoryName(ConfigPath);
        if (!Directory.Exists(Dir))
            Directory.CreateDirectory(Dir);

        JsonObject Root = new() { ["api_key"] = ApiKey };
        File.WriteAllText(ConfigPath, Root.ToJsonString(JsonOptions));
        UpdateClient();
    }

    public void Load()
    {
        if (!File.Exists(ConfigPath))
            return;

        string Json = File.ReadAllText(ConfigPath);
        JsonNode Root = JsonNode.Parse(Json);
        if (Root is null) 
            return;
        
        JsonNode apiKeyNode = Root["api_key"];
        if (apiKeyNode is null)
            return;

        ApiKey = apiKeyNode.GetValue<string>();
        UpdateClient();
    }
    
    private void UpdateClient()
    {
        Client = ApiKey is not null ? new AnthropicClient { ApiKey = ApiKey } : null;
    }
}