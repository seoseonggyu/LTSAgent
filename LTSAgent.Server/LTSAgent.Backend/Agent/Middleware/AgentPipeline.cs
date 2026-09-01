using LTSAgent.Backend.Chat;
using LTSAgent.Backend.Conversation;

namespace LTSAgent.Backend.Agent.Middleware;

/// <summary>
/// 미들웨어 체인을 조립하여 실행하는 에이전트 파이프라인
/// 미들웨어는 Use() 호출 순서대로 실행
/// </summary>
public sealed class AgentPipeline
{
    private readonly List<IAgentMiddleware> Middlewares = [];
    private AgentDelegate Pipeline;
    
    /// <summary> 미들웨어를 파이프라인에 추가 </summary>
    public AgentPipeline Use(IAgentMiddleware Middleware)
    {
        Middlewares.Add(Middleware);
        return this;
    }
    
    /// <summary> 파이프라인의 최종 단계(에이전트 루프)를 설정하고 빌드 </summary>
    public AgentPipeline Run(AgentDelegate Terminal)
    {
        AgentDelegate Current = Terminal;

        // 역순으로 체이닝. 마지막 미들웨어의 Next가 Terminal을 가리킴
        for (int I = Middlewares.Count - 1; I >= 0; I--)
        {
            Middlewares[I].SetNext(Current);
            Current = Middlewares[I].InvokeAsync;
        }

        Pipeline = Current;
        return this;
    }
    
    /// <summary> 파이프라인을 실행 </summary>
    public IAsyncEnumerable<ChatEvent> RunAsync(UserInput Input, AgentSession Session, CancellationToken Ct)
        => (Pipeline ?? throw new InvalidOperationException("Run()으로 파이프라인을 빌드해야 합니다."))(Input, Session, Ct);
}