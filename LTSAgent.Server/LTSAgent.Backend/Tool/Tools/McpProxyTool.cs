using System.Text.Json;
using LTSAgent.Backend.Agent;
using LTSAgent.Backend.Mcp;

namespace LTSAgent.Backend.Tool.Tools;

/// <summary>
/// MCP 서버의 도구를 IAgentTool로 래핑하는 프록시
/// 실행 시 McpClient를 통해 MCP 서버에 tools/call을 전달
/// </summary>
public sealed class McpProxyTool(McpClient Client, string OriginalName) : IAgentTool
{
    /// <summary>
    /// MCP 서버에 tools/call 요청을 보내고 결과를 반환
    /// </summary>
    public async Task<ToolResult> ExecuteAsync(string InputJson, AgentSession Session, CancellationToken Ct = default)
    {
        // JSON 문자열 → JsonElement로 변환
        JsonElement? Arguments = string.IsNullOrWhiteSpace(InputJson)
            ? null
            : JsonDocument.Parse(InputJson).RootElement;

        // MCP 서버에 도구 실행 요청
        ToolCallResult Result = await Client.CallToolAsync(OriginalName, Arguments, Ct);

        // MCP 응답에서 텍스트 추출
        string Text = string.Join("\n", Result.Content
            .Where(C => C is { Type: "text", Text: not null })
            .Select(C => C.Text!));

        return Result.IsError ? ToolResult.Error(Text) : ToolResult.Success(Text);
    }
}