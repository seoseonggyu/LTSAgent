using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using LTSAgent.Backend.Conversation;
using LTSAgent.Frontend.Infrastructure;

namespace LTSAgent.Frontend.UI.Input;

public partial class ChatInput : JsComponentBase
{
    /// <summary> 메시지 전송 콜백 </summary>
    [Parameter] public EventCallback<UserInput> OnSend { get; set; }

    /// <summary> 현재 컨텍스트 토큰 수. TokenMeter에 전달 </summary>
    [Parameter] public long ContextTokens { get; set; }

    /// <summary> textarea 요소 참조 </summary>
    private ElementReference TextAreaRef;
    
    /// <summary> .NET에서 JS가 호출할 수 있는 참조 </summary>
    private DotNetObjectReference<ChatInput> DotNetRef;

    /// <summary> 모드 스위처 컴포넌트 참조 </summary>
    private ModeSwitcher ModeSwitcherRef = null!;
    
    /// <summary> 커맨드 팝업 참조 </summary>
    private CommandPopup CmdPopup = null!;

    /// <summary> 멘션 팝업 참조 </summary>
    private MentionPopup MenPopup = null!;

    /// <summary> 이미지 첨부 컴포넌트 참조. 첫 렌더 전에는 null</summary>
    private ImagePicker? ImagePickerRef;
    
    /// <summary> textarea 바인딩 값 . 변경 시 팝업들을 갱신 </summary>
    private string InputText
    {
        get;
        set
        {
            field = value;
            CmdPopup.Update(value);
            MenPopup.Update(value);
        }
    } = "";

    /// <summary> JS 모듈 로드 후 키 바인딩을 설정 </summary>
    protected override async Task OnModuleLoaded()
    {
        DotNetRef = DotNetObjectReference.Create(this);
        await Module.InvokeVoidAsync("setupKeyBindings", TextAreaRef, DotNetRef);
    }
    
    /// <summary> Shift+Tab 시 JS에서 호출 </summary>
    [JSInvokable]
    public void CycleMode() => ModeSwitcherRef.CycleMode();

    /// <summary> 팝업에서 방향키로 항목을 탐색 </summary>
    [JSInvokable]
    public async Task PopupNavigate(int Direction) => await CmdPopup.Navigate(Direction);

    /// <summary> 팝업에서 현재 선택된 항목을 적용 </summary>
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

    /// <summary> 팝업을 닫음 </summary>
    [JSInvokable]
    public void PopupClose() => CmdPopup.Close();

    // ── 멘션 팝업 ──────────────────────────────────────────────

    /// <summary> 멘션 팝업에서 방향키로 항목을 탐색 </summary>
    [JSInvokable]
    public async Task MentionNavigate(int Direction) => await MenPopup.Navigate(Direction);

    /// <summary> 멘션 팝업에서 Enter로 최종 선택 </summary>
    [JSInvokable]
    public void MentionSelect()
    {
        string Path = MenPopup.Select();
        if (Path is null) 
            return;

        int AtIndex = InputText.LastIndexOf('@');
        if (AtIndex < 0) 
            return;

        InputText = InputText[..AtIndex] + Path + " ";
        
        StateHasChanged();
    }

    /// <summary> 멘션 팝업에서 Tab으로 드릴다운/선택 </summary>
    [JSInvokable]
    public void MentionTab()
    {
        string Result = MenPopup.Tab();
        if (Result is null) 
            return;

        int AtIndex = InputText.LastIndexOf('@');
        if (AtIndex < 0) 
            return;

        InputText = Result.EndsWith('/')
            ? InputText[..AtIndex] + "@" + Result
            : InputText[..AtIndex] + Result + " ";
        
        StateHasChanged();
    }

    /// <summary> 멘션 팝업에서 ← 키로 상위 이동 </summary>
    [JSInvokable]
    public void MentionGoBack()
    {
        string Result = MenPopup.GoBack();
        if (Result is null) 
            return;

        int AtIndex = InputText.LastIndexOf('@');
        if (AtIndex < 0) 
            return;

        InputText = InputText[..AtIndex] + "@" + Result;
        StateHasChanged();
    }

    /// <summary> 멘션 팝업을 닫는다 </summary>
    [JSInvokable]
    public void MentionClose() => MenPopup.Close();

    /// <summary> 폼 제출 시 메시지를 전송 </summary>
    private async Task HandleSubmit()
    {
        string Trimmed = InputText.Trim();
        bool bHasImage = ImagePickerRef?.ImageBase64 is not null;

        if (string.IsNullOrEmpty(Trimmed) && !bHasImage)
            return;

        UserInput Input = new(Trimmed, ImagePickerRef?.ImageMediaType, ImagePickerRef?.ImageBase64);

        InputText = "";
        
        if (ImagePickerRef is not null)
            await ImagePickerRef.Clear();
        
        await OnSend.InvokeAsync(Input);
    }
}