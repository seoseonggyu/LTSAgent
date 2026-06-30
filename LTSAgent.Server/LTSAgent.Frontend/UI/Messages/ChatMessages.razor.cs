using Microsoft.AspNetCore.Components;
using LTSAgent.Backend.Chat;

namespace LTSAgent.Frontend.UI.Messages;

public partial class ChatMessages
{
    // 표시할 메시지 목록
    [Parameter] public List<ChatUIMessage> Messages { get; set; } = [];
}