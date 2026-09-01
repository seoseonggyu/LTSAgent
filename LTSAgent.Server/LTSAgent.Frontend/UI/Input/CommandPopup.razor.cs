using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using LTSAgent.Backend.Command;
using LTSAgent.Backend.Skill;
using LTSAgent.Frontend.Infrastructure;

namespace LTSAgent.Frontend.UI.Input;

public partial class CommandPopup : JsComponentBase
{
    /// <summary> 슬래시 커맨드를 등록·실행하는 레지스트리 </summary>
    [Inject] private CommandRegistry CommandRegistry { get; set; } = null!;
    
    /// <summary> 스킬 목록 </summary>
    [Inject] private SkillRegistry SkillRegistry { get; set; } = null!;

    /// <summary> 등록된 슬래시 커맨드 목록 </summary>
    private IReadOnlyList<CommandRegistry.CommandEntry> Commands => CommandRegistry.GetAll();    
    
    /// <summary> 사용자 전용 스킬 목록 </summary>
     private IReadOnlyList<SkillDefinition> Skills => SkillRegistry.GetUserInvocableSkills();

    /// <summary> 팝업 표시 여부 </summary>
    public bool bShowPopup { get; private set; }

    /// <summary> 현재 선택된 인덱스 </summary>
    private int SelectedIndex;

    /// <summary> 팝업 항목. 커맨드와 스킬을 통합 표시 </summary>
    private sealed record PopupItem(string Name, string Description, string Icon);

    /// <summary> 필터링된 항목 목록 </summary>
    private List<PopupItem> FilteredItems = [];

    /// <summary> 입력 텍스트에 따라 팝업 상태를 갱신 </summary>
    public void Update(string RawInputText)
    {
        string Text = RawInputText;

        // 공백 없는 슬래시 시작 텍스트만 후보로 처리합니다.
        if (!Text.StartsWith('/') || Text.Contains(' '))
        {
            bShowPopup = false;

            StateHasChanged();
            return;
        }

        // '/' 제거
        string Query = Text[1..];

        // 필터링 후 Command 목록
        List<PopupItem> CommandItems = Commands
            .Where(C => C.Name[1..].Contains(Query, StringComparison.OrdinalIgnoreCase))
            .Select(C => new PopupItem(C.Name[1..], C.Description, C.Icon))
            .ToList();

        List<PopupItem> SkillItems = Skills
            .Where(S => S.Name.Contains(Query, StringComparison.OrdinalIgnoreCase))
            .Select(S => new PopupItem(S.Name, S.Description, "auto_stories"))
            .ToList();

        FilteredItems = [..CommandItems, ..SkillItems];
        bShowPopup = FilteredItems.Count > 0;
        SelectedIndex = 0;

        StateHasChanged();
    }

    /// <summary> 방향키로 항목을 탐색 </summary>
    public async Task Navigate(int Direction)
    {
        if (!bShowPopup || FilteredItems.Count == 0)
            return;

        SelectedIndex = (SelectedIndex + Direction + FilteredItems.Count) % FilteredItems.Count;
        StateHasChanged();

        await Module.InvokeVoidAsync("scrollToItem", "popup-item", SelectedIndex);
    }

    /// <summary> 현재 선택된 항목을 적용. 적용된 텍스트를 반환 </summary>
    public string Select()
    {
        if (!bShowPopup || SelectedIndex < 0 || SelectedIndex >= FilteredItems.Count)
            return null;

        string Result = "/" + FilteredItems[SelectedIndex].Name + " ";
        bShowPopup = false;
        StateHasChanged();

        return Result;
    }

    /// <summary> 팝업을 닫음 </summary>
    public void Close()
    {
        bShowPopup = false;
        StateHasChanged();
    }
}