using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using LTSAgent.Backend.Chat;
using static LTSAgent.Frontend.UI.Messages.ToolBlock;

namespace LTSAgent.Frontend.UI.Messages.ToolRenderers;

/// <summary>
/// web_search 도구의 콘텐츠 렌더러.
/// 검색 결과를 제목, URL, 게시일 목록으로 표시.
/// </summary>
public partial class WebSearchBlock : ComponentBase
{
    /// <summary> 표시할 Tool 메시지 </summary>
    [Parameter] public ChatUIMessage.Tool Message { get; set; } = default!;

    /// <summary> 이 도구의 summary 바 메타데이터 </summary>
    public static ToolMeta GetInfo(ChatUIMessage.Tool Msg)
        => new("language", "Web Search", "font-mono",
            ChatUIMessage.Tool.GetInputField(Msg.Input, "query", "web_search"));

    /// <summary> 권한 다이얼로그에 표시할 요약 </summary>
    public static string GetPermissionSummary(string InputJson) => ChatUIMessage.Tool.GetInputField(InputJson, "query");

    /// <summary> 검색 결과 항목 </summary>
    private sealed record SearchResult(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("page_age")] string PageAge)
    {
        // URL에서 추출한 도메인
        public string Domain => Uri.TryCreate(Url, UriKind.Absolute, out Uri Parsed)
            ? Parsed.Host
            : Url;
    }

    /// <summary> Content JSON을 파싱한 검색 결과 목록 </summary>
    private List<SearchResult> Results => ParseResults();

    /// <summary> Content JSON 배열을 SearchResult 목록으로 파싱 </summary>
    private List<SearchResult> ParseResults()
    {
        if (string.IsNullOrEmpty(Message.Content))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<SearchResult>>(Message.Content) ?? [];
        }
        catch
        {
            return [];
        }
    }
}