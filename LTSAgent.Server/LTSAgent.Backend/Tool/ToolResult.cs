namespace LTSAgent.Backend.Tool;

/// <summary>
/// 도구 실행 결과
/// </summary>
public sealed record ToolResult(bool bIsSuccess, string Content)
{
    // 성공 결과를 생성
    public static ToolResult Success(string Content) => new(true, Content);

    // 에러 결과를 생성. "ERROR:" 접두사 없이 원문 그대로 저장
    public static ToolResult Error(string Error) => new(false, Error);
}
