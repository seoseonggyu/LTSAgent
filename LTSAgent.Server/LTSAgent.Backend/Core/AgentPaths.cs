namespace LTSAgent.Backend.Core;

/// <summary>
/// 프로젝트 경로를 제공하는 정적 클래스
/// </summary>
public static class AgentPaths
{
    // ~User/.ltsagent 사용자 설정 디렉터리 경로
    public static readonly string UserConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ltsagent");
}
