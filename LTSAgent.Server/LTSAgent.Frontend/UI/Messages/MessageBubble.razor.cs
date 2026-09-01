using Microsoft.AspNetCore.Components;
using LTSAgent.Backend.Chat;

namespace LTSAgent.Frontend.UI.Messages;

public partial class MessageBubble
{
    /// <summary> 표시할 메시지 </summary>
    [Parameter] public ChatUIMessage UIMessage { get; set; } = null!;
}