namespace LTSAgent.Backend.Agent;

/// <summary>
/// 에이전트 세션
/// </summary>
public sealed class AgentSession
{
    // 이 세션의 대화 히스토리
    public Conversation.Conversation Conversation { get; } = new();
}