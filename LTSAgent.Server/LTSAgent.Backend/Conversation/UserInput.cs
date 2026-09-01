namespace LTSAgent.Backend.Conversation;

/// <summary> 사용자 입력 메시지. 텍스트와 첨부 이미지를 포함 </summary>
public sealed record UserInput(string Text, string ImageMediaType = null, string ImageBase64 = null)
{
    /// <summary> 자동 형변환 ex) UserInput input = "안녕하세요"; UserInput input = new UserInput("안녕하세요"); </summary>
    public static implicit operator UserInput(string Text) => new(Text);
    
    /// <summary> 첨부 이미지가 있는지 여부 </summary>
    public bool bHasImage => ImageBase64 is not null;
}