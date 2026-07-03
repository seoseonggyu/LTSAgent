using LTSAgent.Backend.Agent;
using LTSAgent.Backend.Chat;
using LTSAgent.Backend.Security;
using Microsoft.AspNetCore.Components;

namespace LTSAgent.Frontend.Page;

public partial class Chat : IAsyncDisposable
{
    // 에이전트 실행 서비스
    [Inject] private AgentRunner AgentRunner { get; set; } = null!;
    
    // 설정 패널 표시 여부
    private bool bShowSettings;

    // 설정 패널을 토글
    private void ToggleSettings() => bShowSettings = !bShowSettings;

    // 랜 사용량 패널 표시 여부
    private bool bShowUsage;

    // 플랜 사용량 패널을 토글 // TODO: 토클 사용량
    private void ToggleUsage() => bShowUsage = !bShowUsage;
    
    // 현재 대기 중인 권한 요청
    private ChatEvent.ToolPermissionRequest PendingPermission;

    protected override void OnInitialized()
    {
        AgentRunner.OnChatEvent = OnChatEvent;
    }

    public async ValueTask DisposeAsync()
    {
        if (AgentRunner.OnChatEvent == OnChatEvent)
            AgentRunner.OnChatEvent = null;
        await ValueTask.CompletedTask;
    }
    
    /// <summary>
    /// AgentRunner의 ChatEvent를 UI 스레드에서 처리
    /// Store 수정과 렌더링이 같은 스레드에서 실행되어 스레드 안전성을 보장
    /// </summary>
    private Task OnChatEvent(ChatEvent Evt) => InvokeAsync(() =>
    {
        if (Evt is ChatEvent.ToolPermissionRequest Req)
            PendingPermission = Req;
        else
            AgentRunner.Store.Process(Evt);

        StateHasChanged();
    });
    
    /// <summary>권한 다이얼로그에서 사용자가 결정했을 때 호출</summary>
    private void HandlePermissionDecision(ToolPermission Decision)
    {
        PendingPermission?.Tcs.TrySetResult(Decision);
        PendingPermission = null;
    }
}