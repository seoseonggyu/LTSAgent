namespace LTSAgent.Backend.Tool.Attributes;

/// <summary>
/// ToolRegistry가 자동 스캔하는 도구 마커 어트리뷰트
/// Claude API에 전달할 도구 이름과 설명을 지정
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AgentToolAttribute(string name, string description) : Attribute
{
    // Claude API에 전달할 도구 이름
    public string Name { get; } = name;

    // Claude에게 보여줄 도구 설명
    public string Description { get; } = description;
}