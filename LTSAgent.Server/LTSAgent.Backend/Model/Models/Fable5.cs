using LTSAgent.Backend.Model.Attributes;

namespace LTSAgent.Backend.Model.Models;

/// <summary> Claude Fable 5.0 모델 정의 </summary>
[AgentModel(Order = 0)]
public class Fable5: IModel
{
    public const string ModelId = "claude-fable-5";
    public string Id => ModelId;
    public string DisplayName => "Fable 5.0";
    public string Description => "장시간 실행되는 에이전트를 위한 차세대 지능을 갖춘 모델입니다.";
    public int MaxOutputTokens => 128_000;
    public int ContextWindow => 1_000_000;
}