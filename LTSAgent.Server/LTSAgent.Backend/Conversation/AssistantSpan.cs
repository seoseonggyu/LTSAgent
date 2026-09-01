using LTSAgent.Backend.Core;

namespace LTSAgent.Backend.Conversation;

/// <summary>
/// Claude API 호출 1회의 결과 
/// 어시스턴트 응답 블록과 도구 실행 결과를 포함
/// </summary>
public sealed class AssistantSpan
{
    /// <summary> 어시스턴트 응답 블록 목록 </summary>
    public required IReadOnlyList<Block> AssistantBlocks { get; init; }
    
    /// <summary> 도구 실행 결과 레코드 </summary>
    public sealed record ToolExecution(string ToolUseId, string Name, string Output, bool bIsError);
    
    /// <summary> 도구 실행 결과 목록. 도구 호출이 없으면 비어 있음 </summary>
    public List<ToolExecution> ToolExecutions { get; } = [];
    
    /// <summary> API 호출의 입력 토큰 수 </summary>
    public long InputTokens { get; init; }
}