using Markdig;
using Microsoft.AspNetCore.Components;
using LTSAgent.Backend.Chat;

namespace LTSAgent.Frontend.UI.Messages;

public partial class AssistantMessage
{
    /// <summary> 표시할 어시스턴트 메시지 </summary>
    [Parameter] public ChatUIMessage.Assistant Message { get; set; } = null!;
    
    /// <summary> Markdig 파이프라인 </summary>
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    
    /// <summary> Markdown 텍스트를 HTML로 변환 </summary>
    private static string RenderMarkdown(string Md) => Markdown.ToHtml(Md, Pipeline);
}