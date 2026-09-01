using Microsoft.AspNetCore.Components;
using LTSAgent.Backend.Model;

namespace LTSAgent.Frontend.UI.Input;

public partial class ThinkingToggle
{
    /// <summary> 모델 설정 서비스 </summary>
    [Inject] private ModelSettings Settings { get; set; } = null!;
    
    /// <summary> 토글 상태를 전환 </summary>
    private void Toggle() {  Settings.bThinkingEnabled = !Settings.bThinkingEnabled; }
    
    /// <summary> 라벨 색상. 활성화 시 흰색, 비활성화 시 회색</summary>
    private string LabelColorClass => Settings.bThinkingEnabled ? "text-white" : "text-[#666]";

    /// <summary> 트랙 CSS 클래스 </summary>
    private string TrackClass => Settings.bThinkingEnabled ? "think-on" : "think-off";
}