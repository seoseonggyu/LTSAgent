using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using LTSAgent.Backend.Conversation;
using LTSAgent.Frontend.Infrastructure;

namespace LTSAgent.Frontend.UI.Input;

public partial class ChatInput : JsComponentBase
{
    /// <summary>메시지 전송 콜백입니다.</summary>
    [Parameter] public EventCallback<UserInput> OnSend { get; set; }

    /// <summary>textarea 요소 참조입니다.</summary>
    private ElementReference TextAreaRef;
    
    /// <summary>.NET에서 JS가 호출할 수 있는 참조입니다.</summary>
    private DotNetObjectReference<ChatInput> DotNetRef;

    /// <summary>모드 스위처 컴포넌트 참조입니다.</summary>
    private ModeSwitcher ModeSwitcherRef = null!;
    
    /// <summary>커맨드 팝업 참조입니다.</summary>
    private CommandPopup CmdPopup = null!;
    
    /// <summary>textarea 바인딩 값입니다. 변경 시 커맨드 팝업을 갱신합니다.</summary>
    private string InputText
    {
        get;
        set
        {
            field = value;
            CmdPopup.Update(value);
        }
    } = "";

    /// <summary>JS 모듈 로드 후 키 바인딩을 설정합니다.</summary>
    protected override async Task OnModuleLoaded()
    {
        DotNetRef = DotNetObjectReference.Create(this);
        await Module.InvokeVoidAsync("setupKeyBindings", TextAreaRef, DotNetRef);
    }
    
    /// <summary>Shift+Tab 시 JS에서 호출됩니다.</summary>
    [JSInvokable]
    public void CycleMode() => ModeSwitcherRef.CycleMode();

    /// <summary>팝업에서 방향키로 항목을 탐색합니다.</summary>
    [JSInvokable]
    public async Task PopupNavigate(int Direction) => await CmdPopup.Navigate(Direction);

    /// <summary>팝업에서 현재 선택된 항목을 적용합니다.</summary>
    [JSInvokable]
    public void PopupSelect()
    {
        string Result = CmdPopup.Select();

        if (Result is not null)
        {
            InputText = Result;
            StateHasChanged();
        }
    }

    /// <summary>팝업을 닫습니다.</summary>
    [JSInvokable]
    public void PopupClose() => CmdPopup.Close();

    /// <summary>폼 제출 시 메시지를 전송합니다.</summary>
    private async Task HandleSubmit()
    {
        string Trimmed = InputText.Trim();

        if (string.IsNullOrEmpty(Trimmed))
            return;

        InputText = "";
        await OnSend.InvokeAsync(Trimmed);
    }
}