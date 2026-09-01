namespace LTSAgent.Backend.Mode;

/// <summary> 에이전트 실행 모드 </summary>
public enum AgentMode
{
    /// <summary> 일반적인 상태 </summary>
    Normal,
    /// <summary> 모든 도구가 자동 승인 </summary>
    Edit,
    /// <summary> 모든 도구가 차단되며, 계획적인 분석을 수행 </summary>
    Plan
}