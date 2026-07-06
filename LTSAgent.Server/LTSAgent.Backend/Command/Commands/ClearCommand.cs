using LTSAgent.Backend.Agent;
using LTSAgent.Backend.Chat;
using LTSAgent.Backend.Command.Attributes;

namespace LTSAgent.Backend.Command.Commands;

/// <summary>
/// 대화 내역을 초기화하는 슬래시 커맨드
/// </summary>
[AgentCommand("/clear", "대화 내역을 초기화합니다", icon: "restart_alt")]
public class ClearCommand : IAgentCommand
{
    /// <summary>
    /// 대화 히스토리와 UI 메시지를 모두 초기화
    /// </summary>
    public async IAsyncEnumerable<ChatEvent> ExecuteAsync(string[] Args, AgentSession Session)
    {
        Session.Conversation.Clear();

        yield return new ChatEvent.Command("clear", "");
    }
}