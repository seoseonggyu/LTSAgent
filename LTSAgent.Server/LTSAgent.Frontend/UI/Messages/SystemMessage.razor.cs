using Microsoft.AspNetCore.Components;
using LTSAgent.Backend.Chat;

namespace LTSAgent.Frontend.UI.Messages;

public partial class SystemMessage
{
    /// <summary> 표시할 시스템 메시지 </summary>
    [Parameter] public ChatUIMessage.System Message { get; set; } = null!;
}