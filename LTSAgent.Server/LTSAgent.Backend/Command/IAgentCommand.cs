using LTSAgent.Backend.Agent;
using LTSAgent.Backend.Chat;

namespace LTSAgent.Backend.Command;

/// <summary>
/// 에이전트 커맨드 실행 인터페이스
/// [AgentCommand] 어트리뷰트와 함께 구현하면 CommandRegistry가 자동 스캔
/// </summary>
public interface IAgentCommand
{
    /// <summary>커맨드를 실행하고 결과 ChatEvent를 스트리밍합니다.</summary>
    IAsyncEnumerable<ChatEvent> ExecuteAsync(string[] Args, AgentSession Session);
}