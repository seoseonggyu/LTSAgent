using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using LTSAgent.Backend.Chat;
using LTSAgent.Backend.Security;
using LTSAgent.Frontend.Infrastructure;
using LTSAgent.Frontend.UI.Messages.ToolRenderers;

namespace LTSAgent.Frontend.UI.Messages;

public partial class PermissionDialog : JsComponentBase
{
    /// <summary>표시할 권한 요청</summary>
    [Parameter] public ChatEvent.ToolPermissionRequest Request { get; set; } = null!;

    /// <summary>사용자의 판정 결과 콜백</summary>
    [Parameter] public EventCallback<ToolPermission> OnDecision { get; set; }

    /// <summary>JS에서 C# 메서드를 호출하기 위한 .NET 객체 참조</summary>
    private DotNetObjectReference<PermissionDialog> DotNetRef;

    protected override async Task OnModuleLoaded()
    {
        DotNetRef = DotNetObjectReference.Create(this);
        await Module.InvokeVoidAsync("setup", DotNetRef);
    }

    /// <summary>JS에서 1/2/3 키 입력 시 호출</summary>
    [JSInvokable]
    public async Task HandlePermissionKey(string Key)
    {
        ToolPermission? Decision = Key switch
        {
            "1" => ToolPermission.Allow,
            "2" => ToolPermission.AlwaysAllow,
            "3" => ToolPermission.Deny,
            _ => null
        };

        if (Decision is { } Permission)
            await OnDecision.InvokeAsync(Permission);
    }

    /// <summary>도구 입력에서 사용자가 읽을 수 있는 요약을 추출</summary>
    private string Summary => Request.ToolName switch
    {
        "web_search" => WebSearchBlock.GetPermissionSummary(Request.InputJson),
        "web_fetch"  => WebFetchBlock.GetPermissionSummary(Request.InputJson),
        _ when CodeBlock.IsCodeTool(Request.ToolName) => CodeBlock.GetPermissionSummary(Request.InputJson),
        _ => ""
    };
}