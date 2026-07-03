namespace LTSAgent.Backend.Security;

/// <summary>
/// 도구 실행 권한 판정 결과
/// </summary>
public enum ToolPermission
{
    /// <summary>실행을 허용</summary>
    Allow,
    /// <summary>실행을 거부</summary>
    Deny,
    /// <summary>사용자에게 확인을 요청</summary>
    Ask,
    /// <summary>이 도구를 항상 허용하도록 권한 엔진에 등록</summary>
    AlwaysAllow
}