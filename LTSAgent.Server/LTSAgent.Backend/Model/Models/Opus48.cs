using LTSAgent.Backend.Model.Attributes;

namespace LTSAgent.Backend.Model.Models;

/// <summary> Claude Opus 4.8 모델 정의 </summary>
[AgentModel(Order = 1)]
public class Opus48: IModel
{
    public const string ModelId = "claude-opus-4-8";
    public string Id => ModelId;
    public string DisplayName => "Opus 4.8";
    public string Description => "복잡한 에이전틱 코딩과 엔터프라이즈 업무에 최적화된 모델입니다.";
    public int MaxOutputTokens => 128_000;
    public int ContextWindow => 1_000_000;
}