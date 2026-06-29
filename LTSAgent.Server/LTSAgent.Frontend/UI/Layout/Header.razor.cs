using Microsoft.AspNetCore.Components;

namespace LTSAgent.Frontend.UI.Layout;

public partial class Header
{
    /// <summary>설정 버튼 클릭 콜백입니다.</summary>
    [Parameter] public EventCallback OnSettingsClick { get; set; }
}