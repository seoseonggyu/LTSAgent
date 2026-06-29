using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic.Models.Messages;
using LTSAgent.Backend.Core;

namespace LTSAgent.Backend.Model;

/// <summary>
/// API 런타임 설정 싱글톤
/// 모델 변경 시 이 객체를 업데이트하면 즉시 반영
/// 설정은 ~/.ltsagent/ModelSettings.json에 자동 저장
/// </summary>
public sealed class ModelSettings(ModelRegistry Registry)
{
    // 설정 파일 경로
    private readonly string ConfigPath = Path.Combine(AgentPaths.UserConfigDir, "ModelSettings.json");
    
    // SON 직렬화 옵션
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    
    // 현재 선택된 모델 정의
    private IModel CurrentModel = new Models.Opus46();
    
    // 확장된 사고 활성화 여부 백킹 필드
    private bool ThinkingEnabled = true;
    
    // 사고 깊이 백킹 필드
    private Effort CurrentEffort = Effort.High;
    
    // API 모델 ID
    public string Model => CurrentModel.Id;
    
    // UI 표시 이름
    public string DisplayName => CurrentModel.DisplayName;
    
    // 모델 설명
    public string Description => CurrentModel.Description;
    
    // 최대 출력 토큰 수
    public int MaxTokens => CurrentModel.MaxOutputTokens;

    // 컨텍스트 윈도우 크기
    public int ContextWindow => CurrentModel.ContextWindow;
    
    /// <summary>
    /// 현재 설정에 맞는 ThinkingConfigParam을 반환
    /// </summary>
    public ThinkingConfigParam GetThinking() => bThinkingEnabled ? new ThinkingConfigAdaptive() : new ThinkingConfigDisabled();

    /// <summary>
    /// 현재 설정에 맞는 Effort의 OutputConfig를 반환
    /// </summary>
    public OutputConfig GetEffort() => new() { Effort = Effort };
    
    /// <summary>
    /// 모델을 변경
    /// </summary>
    public void Select(IModel ClaudeModel)
    {
        CurrentModel = ClaudeModel;
        Save();
    }
    
    // 확장된 사고(Extended Thinking) 활성화 여부
    public bool bThinkingEnabled
    {
        get => ThinkingEnabled;
        set { ThinkingEnabled = value; Save(); }
    }
    
    // Claude의 사고 깊이. thinking과 독립적으로 동작
    public Effort Effort
    {
        get => CurrentEffort;
        set { CurrentEffort = value; Save(); }
    }
    
    /// <summary>
    /// 현재 설정을 파일에 저장. 디렉토리가 없으면 생성
    /// </summary>
    private void Save()
    {
        string Dir = Path.GetDirectoryName(ConfigPath)!;
        if (!Directory.Exists(Dir))
            Directory.CreateDirectory(Dir);

        JsonObject Root = new()
        {
            ["model"] = Model,
            ["thinking_enabled"] = ThinkingEnabled,
            ["effort"] = CurrentEffort.ToString().ToLowerInvariant()
        };

        File.WriteAllText(ConfigPath, Root.ToJsonString(JsonOptions));
    }
    
    /// <summary>
    /// 설정 파일에서 로드. ModelRegistry가 초기화된 후 호출
    /// </summary>
    public void Load()
    {
        if (!File.Exists(ConfigPath))
            return;

        string Json = File.ReadAllText(ConfigPath);
        JsonNode Root = JsonNode.Parse(Json);
        if (Root is null)
            return;

        if (Root["model"]?.GetValue<string>() is { } ModelId && Registry.FindById(ModelId) is { } Found)
            CurrentModel = Found;

        if (Root["thinking_enabled"] is not null)
            ThinkingEnabled = Root["thinking_enabled"]!.GetValue<bool>();

        if (Root["effort"]?.GetValue<string>() is { } EffortStr && Enum.TryParse<Effort>(EffortStr, true, out Effort ParsedEffort))
            CurrentEffort = ParsedEffort;
    }
}