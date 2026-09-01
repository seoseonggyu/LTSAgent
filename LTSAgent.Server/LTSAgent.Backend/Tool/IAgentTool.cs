using System.Text.Json;
using LTSAgent.Backend.Agent;

namespace LTSAgent.Backend.Tool;

/// <summary>
/// 에이전트 도구 실행 인터페이스
/// [AgentTool] 어트리뷰트와 함께 구현하면 ToolRegistry가 자동 스캔
/// </summary>
public interface IAgentTool
{
    // 도구를 실행하고 결과를 반환
    Task<ToolResult> ExecuteAsync(string InputJson, AgentSession Session, CancellationToken Ct = default);
}

/// <summary>
/// 타입 안전한 도구 기본 클래스
/// JSON 입력을 TInput 레코드로 자동 역직렬화
/// </summary>
public abstract class AgentTool<TInput> : IAgentTool
{
    // JSON 문자열을 TInput으로 역직렬화하여 실행
    public Task<ToolResult> ExecuteAsync(string InputJson, AgentSession Session, CancellationToken Ct = default)
    {
        TInput Input = JsonSerializer.Deserialize<TInput>(InputJson);
        if (Input == null)
        {
            throw new ArgumentException($"Failed to deserialize {typeof(TInput).Name}.");
        }
        return ExecuteAsync(Input, Session, Ct);
    }
    
    // 타입 안전한 도구 실행 메서드
    protected abstract Task<ToolResult> ExecuteAsync(TInput Input, AgentSession Session, CancellationToken Ct);
}