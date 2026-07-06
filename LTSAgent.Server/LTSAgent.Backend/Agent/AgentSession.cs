using LTSAgent.Backend.Conversation;
using LTSAgent.Backend.Chat;
using LTSAgent.Backend.Mode;
using LTSAgent.Backend.Security;

namespace LTSAgent.Backend.Agent;

/// <summary>
/// 에이전트 세션
/// </summary>
public sealed class AgentSession(AgentLoop Loop)
{
    /// <summary> 이 세션의 대화 히스토리 </summary>
    public Conversation.Conversation Conversation { get; } = new();
    
    /// <summary>이 세션의 도구 실행 권한 엔진</summary>
    public PermissionEngine PermissionEngine { get; } = new();
    
    /// <summary> 사용자 메시지를 처리 </summary>
    public IAsyncEnumerable<ChatEvent> ProcessMessage(UserInput Input) => Loop.RunAsync(Input, this);
    
    /// <summary>현재 에이전트 모드 </summary>
    public AgentMode Mode { get; set; } = AgentMode.Normal;
}