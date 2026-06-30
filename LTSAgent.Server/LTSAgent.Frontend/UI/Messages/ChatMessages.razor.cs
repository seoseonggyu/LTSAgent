using Microsoft.AspNetCore.Components;
using LTSAgent.Backend.Chat;

namespace LTSAgent.Frontend.UI.Messages;

public partial class ChatMessages
{
    // 표시할 메시지 목록
    [Parameter] public List<ChatUIMessage> Messages { get; set; } = [];
    
    // 응답 수신 시작 여부. false?이면 shimmer를 숨김
    [Parameter] public bool bIsReceiving { get; set; }
}