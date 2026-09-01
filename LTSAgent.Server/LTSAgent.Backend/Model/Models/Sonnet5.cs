using LTSAgent.Backend.Model.Attributes;

namespace LTSAgent.Backend.Model.Models;

/// <summary> Claude Sonnet 5.0 모델 정의 </summary>
[AgentModel(Order = 2)]
public class Sonnet5: IModel
{
    public const string ModelId = "claude-sonnet-5";
    public string Id => ModelId;
    public string DisplayName => "Sonnet 5.0";
    public string Description => "속도와 지능의 최적 균형을 갖춘 모델입니다.";
    public int MaxOutputTokens => 128_000;
    public int ContextWindow => 1_000_000;
}
