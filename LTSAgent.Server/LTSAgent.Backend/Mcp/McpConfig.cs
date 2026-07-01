using System.Text.Json;
using System.Text.Json.Serialization;
using LTSAgent.Backend.Core;

namespace LTSAgent.Backend.Mcp;


/// <summary>
/// settings.local.json에서 mcpServers 섹션을 로드
/// </summary>
public static class McpConfig
{
    /// <summary>
    /// {ConfigDir}/settings.local.json에서 mcpServers를 읽어 반환
    /// 파일이 없거나 섹션이 없으면 빈 딕셔너리를 반환
    /// </summary>
    public static Dictionary<string, McpServerConfig> Load()
    {
        // TODO: C:\Users\{userName} 가 아니라 각 프로젝트에 맞게 설정 필요
        string SettingsPath = Path.Combine(AgentPaths.UserConfigDir, "settings.global.json");
        
        string Json = File.ReadAllText(SettingsPath);
        using JsonDocument Doc = JsonDocument.Parse(Json);

        if (!Doc.RootElement.TryGetProperty("mcpServers", out JsonElement McpElement))
            return new();

        return McpElement.Deserialize<Dictionary<string, McpServerConfig>>() ?? new();
    }
}


/// <summary>
/// MCP 서버 하나의 설정
/// </summary>
public sealed record McpServerConfig
(
    [property: JsonPropertyName("url")]
    string Url
);