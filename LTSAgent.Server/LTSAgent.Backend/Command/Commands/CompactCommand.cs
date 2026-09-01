using System.Text;
using System.Text.RegularExpressions;
using Anthropic.Models.Messages;
using LTSAgent.Backend.Agent;
using LTSAgent.Backend.Auth;
using LTSAgent.Backend.Chat;
using LTSAgent.Backend.Command.Attributes;
using LTSAgent.Backend.Model.Models;

namespace LTSAgent.Backend.Command.Commands;

/// <summary> 대화 히스토리를 Claude API로 요약하여 컨텍스트 윈도우를 확보하는 슬래시 커맨드 </summary>
[AgentCommand("/compact", "컨텍스트를 요약하여 압축합니다. 지시사항을 추가할 수 있습니다", icon: "compress")]
public partial class CompactCommand(AuthConfig Auth) : IAgentCommand
{
    /// <summary> 요약 결과에서 summary 태그 내용을 추출하는 정규식 </summary>
    [GeneratedRegex(@"<summary>(.*?)</summary>", RegexOptions.Singleline)]
    private static partial Regex SummaryTagRegex();

    public async IAsyncEnumerable<ChatEvent> ExecuteAsync(string[] Args, AgentSession Session)
    {
        yield return new ChatEvent.System("컨텍스트 윈도우를 요약 하고 있습니다.");

        string CustomInstruction = Args.Length > 0 ? string.Join(' ', Args) : null;
        string Summary = await SummarizeAsync(Session.Conversation, CustomInstruction);

        if (Summary is null)
        {
            yield return new ChatEvent.System("요약에 실패했습니다.");
            yield break;
        }

        Session.Conversation.Compact(Summary);

        yield return new ChatEvent.Command("clear", "");
    }

    /// <summary> 대화 히스토리를 원본 메시지 구조 그대로 Claude API에 보내 요약 </summary>
    private async Task<string> SummarizeAsync(Conversation.Conversation Conversation, string CustomInstruction)
    {
        if (Auth.Client is null)
            return null;

        string Suffix = string.IsNullOrEmpty(CustomInstruction)
            ? ""
            : $"\n\nAdditional Instructions:\n{CustomInstruction}";

        // 원본 메시지 구조를 그대로 사용하고, 마지막에 요약 요청을 추가
        List<MessageParam> Messages = Conversation.ToAnthropicMessages();
        Messages.Add(new MessageParam
        {
            Role = Role.User,
            Content = SummaryPrompt + Suffix
        });

        // Opus 4.8모델에게 요약 요청
        Message Response = await Auth.Client.Messages.Create(new MessageCreateParams
        {
            Model = Opus48.ModelId,
            MaxTokens = 40000,
            System = new List<TextBlockParam>
            {
                new() { Text = "You are a helpful AI assistant tasked with summarizing conversations." }
            },
            Messages = Messages,
            Thinking = new ThinkingConfigAdaptive(),
            OutputConfig = new OutputConfig()
            {
                Effort = Effort.High
            }
        });

        // 결과 텍스트 가져옴
        string ResponseText = ExtractText(Response.Content);

        if (string.IsNullOrWhiteSpace(ResponseText))
            return null;

        // Summary 안의 내용만 가져옴
        Match Match = SummaryTagRegex().Match(ResponseText);
        return Match.Success ? Match.Groups[1].Value.Trim() : ResponseText.Trim();
    }

    /// <summary> 응답 ContentBlock 목록에서 텍스트를 추출 </summary>
    private static string ExtractText(IReadOnlyList<ContentBlock> Content)
    {
        StringBuilder Sb = new();

        foreach (ContentBlock ContentBlock in Content)
        {
            if (ContentBlock.TryPickText(out TextBlock TextBlock))
                Sb.Append(TextBlock.Text);
        }

        return Sb.ToString();
    }

    /// <summary> 요약 요청 프롬프트 </summary>
    private const string SummaryPrompt = """
                                         You have been working on a task in a Revit project but have not yet completed it.
                                         Write a continuation summary that will allow you (or another instance of yourself)
                                         to resume work efficiently in a future context window where the conversation history
                                         will be replaced with this summary.

                                         Before providing your final summary, wrap your analysis in <analysis> tags to organize
                                         your thoughts. Chronologically analyze each part of the conversation, identifying:
                                         - The user's explicit requests and intents
                                         - Key decisions, technical concepts and patterns
                                         - Actions performed in Revit via the Revit API
                                         - Errors encountered and how they were fixed
                                         - User feedback, especially corrections or "do it differently" instructions

                                         Then provide your summary in <summary></summary> tags with these sections:

                                         1. Primary Request and Intent — The user's core request, success criteria, and constraints
                                         2. Key Technical Concepts — Revit API namespaces/classes used (e.g. Document, Transaction,
                                            FilteredElementCollector, ElementId, Family/FamilySymbol, Parameter, ExternalCommand/
                                            ExternalApplication, IExternalEventHandler, UIDocument), .NET/C# patterns, and any
                                            add-in framework or build setup (e.g. .addin manifest, target Revit version/API version)
                                            discussed
                                         3. Work Performed — Actions performed in Revit via the API
                                            (element creation/modification via Transaction, family/type loading or swapping,
                                            parameter reads/writes, geometry/location changes, view/sheet creation or configuration,
                                            schedule creation, event handler or command registration, etc.)
                                         4. Errors and Fixes — Errors encountered (e.g. exceptions, failed Transactions, API version
                                            mismatches, regeneration/warning issues), how they were fixed, and user corrections.
                                            Note what approaches failed and should not be tried again
                                         5. All User Messages — List ALL non-tool-result user messages to preserve intent and feedback
                                         6. Pending Tasks — Remaining tasks explicitly requested by the user
                                         7. Current Work — What was being worked on immediately before this summary, in detail
                                            (including relevant file/class names and method signatures if applicable)
                                         8. Next Step — The immediate next action, with direct quotes from the most recent conversation.
                                            Only include if directly in line with the user's most recent request

                                         Be thorough — include specific details that would prevent duplicate work or repeated mistakes.

                                         IMPORTANT: Do NOT use any tools. You MUST respond with ONLY the <analysis> and <summary>
                                         blocks as your text output. If Additional Instructions are provided below, prioritize them
                                         when creating the summary.
                                         """;
}