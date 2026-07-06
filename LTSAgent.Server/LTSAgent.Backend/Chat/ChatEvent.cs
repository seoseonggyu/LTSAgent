using LTSAgent.Backend.Security;

namespace LTSAgent.Backend.Chat;

/// <summary>
/// Agent에서 UI로 전달되는 스트리밍 이벤트
/// </summary>
public abstract record ChatEvent
{
    /// 사용자 메시지
    public sealed record User(string Content) : ChatEvent;
    
    // Claude의 텍스트 응답
    public sealed record Assistant(string Content) : ChatEvent;
    
    // Claude의 사고 과정(Extended Thinking) 응답
    public sealed record Thinking(string Content) : ChatEvent;
    
    // 도구 실행 시작
    public sealed record ToolStart(string ToolUseId, string Name, string Input) : ChatEvent;
    
    // 도구 실행 결과
    public sealed record ToolEnd(string ToolUseId, string Name, string Result) : ChatEvent;
    
    // 도구 실행 권한 요청. UI에서 허용/거부 다이얼로그를 표시
    public sealed record ToolPermissionRequest(string ToolUseId, string ToolName, string InputJson) : ChatEvent
    {
        /// <summary>UI에서 SetResult로 응답하면 AgentLoop의 await가 깨어남</summary>
        public TaskCompletionSource<ToolPermission> Tcs { get; } = new();
    }
    
    /// <summary>시스템 메시지 (커맨드 결과, 에러 등).</summary>
    public sealed record System(string Content) : ChatEvent;
    
    /// <summary>커맨드 메시지 </summary>
    public sealed record Command(string Name, string Argument) : ChatEvent;
    
    // 스트림 종료
    public sealed record Done : ChatEvent;
}