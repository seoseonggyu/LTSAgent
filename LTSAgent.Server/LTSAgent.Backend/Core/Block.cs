namespace LTSAgent.Backend.Core;

/// <summary>
/// LLM으로부터 받는 콘텐츠 블록
/// </summary>
public abstract record Block
{
    // 텍스트 응답 블록
    public sealed record Text(string Content) : Block;

    // 사고 과정(Extended Thinking) 블록
    public sealed record Thinking(string Content, string Signature) : Block;
    
    // 도구 호출 블록
    public sealed record ToolUse(string Id, string Name, string InputJson) : Block;
}