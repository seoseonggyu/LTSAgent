using Microsoft.AspNetCore.Components;
using LTSAgent.Backend.Chat;

namespace LTSAgent.Frontend.UI.Messages;

/// <summary>
/// Extended Thinking 블록의 코드-비하인드
/// 완료 여부에 따라 녹색/파란색 테마를 전환하고, 경과 시간을 표시
/// </summary>
public partial class ThinkingBlock : IDisposable
{
    // 표시할 Thinking 메시지
    [Parameter] public ChatUIMessage.Thinking Message { get; set; } = null!;

    // 경과 시간 갱신용 타이머
    private Timer _Timer;

    // 사고가 완료되었는지 여부
    private bool bIsCompleted => Message.bIsCompleted;

    // 컴포넌트 초기화 시 타이머를 시작
    protected override void OnInitialized()
    {
        if (!bIsCompleted)
            _Timer = new Timer(_ => InvokeAsync(StateHasChanged), null, 0, 100);
    }

    // 파라미터 변경 시 완료되면 타이머를 정지
    protected override void OnParametersSet()
    {
        if (bIsCompleted)
        {
            _Timer?.Dispose();
            _Timer = null;
        }
    }

    // 이머 리소스를 해제
    public void Dispose()
    {
        _Timer?.Dispose();
        _Timer = null;
    }
    
    // 사고 내용을 줄 단위로 분리
    private string[] SplitLines()
    {
        if (string.IsNullOrWhiteSpace(Message.Content))
            return [""];

        return Message.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    // ── 완료: 녹색 테마, 진행 중: 파란색 테마 ──

    // 외곽선 색상 클래스
    private string BorderClass => bIsCompleted ? "border-[#4ba96c]" : "border-[#0070e0]";

    // 그림자 효과 클래스
    private string ShadowClass => bIsCompleted
        ? "shadow-[0_0_12px_rgba(75,169,108,0.15)]"
        : "shadow-[0_0_12px_rgba(0,112,224,0.15)]";

    // 아이콘 색상 클래스
    private string IconColorClass => bIsCompleted ? "text-[#4ba96c]" : "text-[#0070e0]";

    // 배경 색상 클래스
    private string BgClass => bIsCompleted ? "bg-[#1a2e1a]" : "bg-[#0f1a2e]";

    // 정보 바 외곽선 클래스
    private string BorderBarClass => bIsCompleted ? "border-[#2a5a2a]" : "border-[#1a3a5a]";

    // 텍스트 색상 클래스
    private string TextColorClass => bIsCompleted ? "text-[#4ba96c]" : "text-[#5a9fd6]";

    // 타이머 색상 클래스
    private string TimerColorClass => bIsCompleted ? "text-[#4ba96c]/70" : "text-[#5a9fd6]/70";

    // 경과 시간 표시 문자열. 완료 시 고정, 진행 중이면 실시간 갱신
    private string ElapsedDisplay => bIsCompleted
        ? $"{Message.ElapsedSeconds:F1}s"
        : $"{(DateTime.Now - Message.StartTime).TotalSeconds:F1}s";
}