using LTSAgent.Backend.Core;

namespace LTSAgent.Backend.Conversation;

/// <summary>
/// Claude API 호출 1회의 결과
/// 어시스턴트 응답 블록과 도구 실행 결과를 포함
/// </summary>
public sealed class AssistantSpan
{
    // 어시스턴트 응답 블록 목록
    public required IReadOnlyList<Block> AssistantBlocks { get; init; }
    
    // 도구 실행 결과 레코드
    public sealed record ToolExecution(string ToolUseId, string Name, string Output, bool bIsError);
    
    // 도구 실행 결과 목록. 도구 호출이 없으면 비어 있음
    public List<ToolExecution> ToolExecutions { get; } = [];
}