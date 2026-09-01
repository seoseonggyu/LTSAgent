using System.Net.Http.Json;
using System.Text.Json;

namespace LTSAgent.Backend.Mcp;

/// <summary>
/// HTTP를 통해 MCP 서버와 통신하는 클라이언트
/// initialize → tools/list → tools/call 흐름을 처리
/// </summary>
public class McpClient(HttpClient Http, string ServerName, string Url)
{
    /// <summary> JSON-RPC 요청 ID 카운터 </summary>
    private int NextId;

    /// <summary> initialize 결과. 연결 후 서버 정보를 있음 </summary>
    public InitializeResult ServerResult { get; private set; }

    /// <summary> 서버가 도구를 지원하는지 여부 </summary>
    public bool HasTools => ServerResult?.Capabilities.HasTools ?? false;

    /// <summary> MCP 서버에 initialize 요청을 보내고 서버 정보를 받음 </summary>
    public async Task<InitializeResult> InitializeAsync(CancellationToken Ct = default)
    {
        InitializeResult Result = await SendAsync<InitializeParams, InitializeResult>(
            "initialize",
            new InitializeParams(),
            Ct
        );

        ServerResult = Result;

        return Result;
    }

    /// <summary> 서버에서 사용 가능한 도구 목록을 가져옴 </summary>
    public async Task<List<McpToolDefinition>> ListToolsAsync(CancellationToken Ct = default)
    {
        ToolsListResult Result = await SendAsync<object, ToolsListResult>(
            "tools/list",
            new { },
            Ct
        );

        return Result.Tools;
    }

    /// <summary> MCP 서버의 도구를 실행 </summary>
    public async Task<ToolCallResult> CallToolAsync(string ToolName, JsonElement? Arguments,
        CancellationToken Ct = default)
    {
        return await SendAsync<ToolCallParams, ToolCallResult>(
            "tools/call",
            new ToolCallParams { Name = ToolName, Arguments = Arguments },
            Ct
        );
    }

    /// <summary> JSON-RPC 요청을 보내고 응답의 result를 역직렬화하여 반환 </summary>
    private async Task<TResult> SendAsync<TParams, TResult>(string Method, TParams Params, CancellationToken Ct)
    {
        JsonRpcRequest Request = new()
        {
            Id = Interlocked.Increment(ref NextId),
            Method = Method,
            Params = Params
        };

        HttpResponseMessage HttpResponse = await Http.PostAsJsonAsync(Url, Request, Ct);
        HttpResponse.EnsureSuccessStatusCode();

        JsonRpcResponse RpcResponse =
            await HttpResponse.Content.ReadFromJsonAsync<JsonRpcResponse>(cancellationToken: Ct);

        if (RpcResponse is null)
            throw new InvalidOperationException($"[{ServerName}] 빈 응답을 받았습니다.");

        if (!RpcResponse.IsSuccess)
            throw new InvalidOperationException(
                $"[{ServerName}] {RpcResponse.Error!.Message} (code: {RpcResponse.Error.Code})");

        return RpcResponse.Result!.Value.Deserialize<TResult>()
               ?? throw new InvalidOperationException($"[{ServerName}] result 역직렬화에 실패했습니다.");
    }
}