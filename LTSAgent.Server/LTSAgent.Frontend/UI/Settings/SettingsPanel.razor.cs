using Microsoft.AspNetCore.Components;
using LTSAgent.Backend.Auth;

namespace LTSAgent.Frontend.UI.Settings;

public partial class SettingsPanel
{
    /// <summary> 인증 설정 </summary>
    [Inject] private AuthConfig Auth { get; set; } = null!;

    /// <summary> 패널 표시 여부 </summary>
    [Parameter] public bool bIsVisible { get; set; }

    /// <summary> 패널 닫기 콜백 </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    /// <summary> API Key 입력값 </summary>
    private string ApiKeyInput = "";

    /// <summary> 상태 메시지 </summary>
    private string StatusMessage = "";

    /// <summary> 상태 메시지 CSS 클래스 </summary>
    private string StatusCss = "";

    /// <summary> API Key를 저장 </summary>
    private void SaveApiKey()
    {
        if (string.IsNullOrWhiteSpace(ApiKeyInput))
        {
            StatusMessage = "API Key를 입력해주세요.";
            StatusCss = "text-[#e05e5e]";
            return;
        }

        Auth.SetApiKey(ApiKeyInput.Trim());
        ApiKeyInput = "";
        StatusMessage = "API Key가 저장되었습니다.";
        StatusCss = "text-[#4ba96c]";
    }
}