namespace LTSAgent.Backend.Command.Attributes;

/// <summary>
/// CommandRegistry가 자동 스캔하는 슬래시 커맨드 마커 어트리뷰트
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AgentCommandAttribute(string name, string description, string icon = "terminal") : Attribute
{
    /// <summary>슬래시 커맨드 이름 (예: "/clear") </summary>
    public string Name { get; } = name;

    /// <summary>사용자에게 표시할 커맨드 설명 </summary>
    public string Description { get; } = description;

    /// <summary>Material Symbols 아이콘 이름 </summary>
    public string Icon { get; } = icon;
}