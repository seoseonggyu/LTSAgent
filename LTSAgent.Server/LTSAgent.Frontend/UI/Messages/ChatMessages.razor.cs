using Microsoft.AspNetCore.Components;
using LTSAgent.Backend.Chat;

namespace LTSAgent.Frontend.UI.Messages;

public partial class ChatMessages
{
    /// <summary> 표시할 메시지 목록 </summary>
    [Parameter] public List<ChatUIMessage> Messages { get; set; } = [];
    
    /// <summary> 응답 수신 시작 여부. false이면 shimmer를 숨김 </summary>
    [Parameter] public bool bIsReceiving { get; set; }
}