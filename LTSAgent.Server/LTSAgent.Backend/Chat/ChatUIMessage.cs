using System.Text.Json;

namespace LTSAgent.Backend.Chat;

/// <summary> UI에 표시되는 채팅 메시지 </summary>
public abstract record ChatUIMessage
{
    /// <summary> 메시지 본문 </summary>
    public abstract string Content { get; init; }
    
    /// <summary> 메시지 생성 시간 </summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;
    
    /// <summary> Content 뒤에 텍스트를 이어붙인 새 인스턴스를 반환 </summary>
    public ChatUIMessage Append(string Text) => this with { Content = Content + Text };
    
    /// <summary> 사용자 메시지 </summary>
    public sealed record User(string Content, string ImageMediaType = null, string ImageBase64 = null) : ChatUIMessage;
    
    /// <summary> 어시스턴트(AI) 응답 </summary>
    public sealed record Assistant(string Content) : ChatUIMessage;
    
    /// <summary> 사고 과정(Extended Thinking) 메시지 </summary>
    public sealed record Thinking(string Content) : ChatUIMessage
    {
        // 사고 시작 시간. UI에서 실시간 경과 시간을 계산
        public DateTime StartTime { get; init; }

        // 사고 과정에 소요된 최종 시간(초). 완료 후 확정
        public double ElapsedSeconds { get; init; }

        // 완료 여부
        public bool bIsCompleted { get; init; }
    }
    
    /// <summary> 도구 실행 메시지 </summary>
    public sealed record Tool(string Name, string Content) : ChatUIMessage
    {
        // Claude가 발급한 tool_use ID
        public string ToolUseId { get; init; } = "";

        // 도구 입력 파라미터(JSON)
        public string Input { get; init; } = "";

        // 도구 실행 시작 시간. UI에서 실시간 경과 시간을 계산
        public DateTime StartTime { get; init; }

        // 도구 실행 소요 최종 시간(초). 완료 후 확정
        public double ElapsedSeconds { get; init; }

        // 도구 실행 완료 여부
        public bool bIsCompleted { get; init; }

        // JSON 문자열에서 지정 필드의 문자열 값을 추출
        public static string GetInputField(string Json, string FieldName, string Fallback = "")
        { 
            if (string.IsNullOrEmpty(Json))
                return Fallback;

            try
            {
                using JsonDocument Doc = JsonDocument.Parse(Json);
                return Doc.RootElement.TryGetProperty(FieldName, out JsonElement Element)
                    ? Element.GetString() ?? Fallback
                    : Fallback;
            }
            catch
            {
                return Fallback;
            }
        }
    }
    
    /// <summary> 시스템 메시지 </summary>
    public sealed record System(string Content) : ChatUIMessage;
}