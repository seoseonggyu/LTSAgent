using System.Runtime.CompilerServices;
using LTSAgent.Backend.Chat;
using LTSAgent.Backend.Command;
using LTSAgent.Backend.Conversation;

namespace LTSAgent.Backend.Agent.Middleware;

/// <summary>
/// 슬래시 입력을 가로채서 커맨드 또는 스킬을 실행하는 미들웨어
/// 커맨드는 파이프라인을 단락하고, 스킬은 본문을 주입한 뒤 AgentLoop로 전달
/// </summary>
public sealed class SlashCommandMiddleware(CommandRegistry CommandRegistry) : IAgentMiddleware
{
    public override async IAsyncEnumerable<ChatEvent> InvokeAsync(UserInput Input, AgentSession Session, [EnumeratorCancellation] CancellationToken Ct)
    {
        // 1. 커맨드 우선 확인 (단락 실행)
        if (Input.Text.StartsWith('/'))
        {
            if (CommandRegistry.HasCommand(Input.Text))
            {
                await foreach (ChatEvent Evt in CommandRegistry.ExecuteAsync(Input.Text, Session).WithCancellation(Ct))
                    yield return Evt;
                
                yield break;
            }
        }
        
        // 2. /에서 처리하지 않는 로직의 경우 다음 Middleware로 이동
        await foreach (ChatEvent Evt in Next(Input, Session, Ct))
            yield return Evt;
    }
}