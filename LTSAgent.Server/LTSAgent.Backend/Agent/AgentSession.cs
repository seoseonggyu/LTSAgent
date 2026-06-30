using LTSAgent.Backend.Conversation;
using LTSAgent.Backend.Chat;

namespace LTSAgent.Backend.Agent;

/// <summary>
/// 에이전트 세션
/// </summary>
public sealed class AgentSession(AgentLoop Loop)
{
    // 이 세션의 대화 히스토리
    public Conversation.Conversation Conversation { get; } = new();
    
    /// <summary>
    /// 사용자 메시지를 처리합니다.
    /// </summary>
    public IAsyncEnumerable<ChatEvent> ProcessMessage(UserInput Input) => Loop.RunAsync(Input, this);
}