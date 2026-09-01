using LTSAgent.Backend.Chat;
using LTSAgent.Backend.Conversation;

namespace LTSAgent.Backend.Agent.Middleware;

/// <summary> 에이전트 파이프라인의 다음 단계를 실행하는 델리게이트 </summary>
public delegate IAsyncEnumerable<ChatEvent> AgentDelegate(UserInput Input, AgentSession Session, CancellationToken Ct);

/// <summary>
/// 에이전트 파이프라인 미들웨어 기본 클래스
/// 요청 전후에 로직을 삽입하거나, 요청을 가로채서 단락할 수 있음
/// </summary>
public abstract class IAgentMiddleware
{
    /// <summary> 파이프라인의 다음 단계 </summary>
    protected AgentDelegate Next { get; private set; } = null!;

    /// <summary> 다음 단계를 설정. AgentPipeline이 빌드 시 호출 </summary>
    internal void SetNext(AgentDelegate Delegate) => Next = Delegate;

    /// <summary> 미들웨어 로직을 실행 </summary>
    public abstract IAsyncEnumerable<ChatEvent> InvokeAsync(UserInput Input, AgentSession Session, CancellationToken Ct);
}