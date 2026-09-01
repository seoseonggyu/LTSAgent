using System.Text;
using Anthropic.Models.Messages;
using LTSAgent.Backend.Agent;
using LTSAgent.Backend.Core;
using LTSAgent.Backend.Mode;
using LTSAgent.Backend.Model;
using LTSAgent.Backend.Model.Models;
using LTSAgent.Backend.Skill;
using LTSAgent.Backend.Tool;

namespace LTSAgent.Backend.Prompt;

/// <summary>
/// Claude API 시스템 프롬프트 구성과 MessageCreateParams 생성을 담당
/// 시스템 프롬프트는 최초 호출 시 생성되고 이후 캐싱
/// </summary>
public sealed class PromptBuilder(ToolRegistry ToolRegistry, ModelSettings ModelSettings, SkillRegistry SkillRegistry)
{
    /// <summary> 빌더 체인의 각 섹션. 토큰 측정 시 특정 섹션을 제외할 수 있음 </summary>
    [Flags]
    public enum Section
    {
        None = 0,
        Identity = 1 << 0,
        System = 1 << 1,
        DoingTasks = 1 << 2,
        Proactiveness = 1 << 3,
        ToneAndStyle = 1 << 4,
        OutputEfficiency = 1 << 5,
        ModeOverride = 1 << 6,
        RevitAgentMd = 1 << 7,
        Skills = 1 << 8,

        All = Identity | System | DoingTasks | Proactiveness | ToneAndStyle | OutputEfficiency | ModeOverride |
              RevitAgentMd | Skills,
    }

    // ── API 파라미터 생성 ──

    /// <summary> Claude API 호출 파라미터를 생성 </summary>
    public MessageCreateParams Build(AgentSession Session) => new()
    {
        Model = ModelSettings.Model,
        MaxTokens = ModelSettings.MaxTokens,
        CacheControl = new CacheControlEphemeral(),
        System = new List<TextBlockParam> { new() { Text = BuildSystemPrompt(Session) } },
        Messages = Session.Conversation.ToAnthropicMessages(),
        Tools = ToolRegistry.GetAllSchemas().Select(S => (ToolUnion)S).ToList(),
        Thinking = ModelSettings.Model != Haiku45.ModelId ? ModelSettings.GetThinking() : null,
        OutputConfig = ModelSettings.Model != Haiku45.ModelId ? ModelSettings.GetEffort() : null
    };

    // ── 시스템 프롬프트 구성 ──

    /// <summary> 시스템 프롬프트를 반환. 모드에 따라 동적으로 생성 </summary>
    public string BuildSystemPrompt(AgentSession Session = null)
    {
        return BuildInternal(Section.None, Session);
    }

    /// <summary> 지정한 섹션을 제외한 시스템 프롬프트를 생성 </summary>
    public string BuildWithout(Section Skip, AgentSession Session = null) => BuildInternal(Skip, Session);

    /// <summary> 지정한 섹션만 포함한 시스템 프롬프트를 생성 </summary>
    public string BuildOnly(Section Include, AgentSession Session = null) =>
        BuildInternal(Section.All & ~Include, Session);

    /// <summary> 각 섹션 메서드를 호출하고 결과를 결합하여 프롬프트 문자열을 생성 </summary>
    private string BuildInternal(Section Skip, AgentSession Session)
    {
        AgentMode Mode = Session?.Mode ?? AgentMode.Normal;

        StringBuilder Sb = new();

        if (!Skip.HasFlag(Section.Identity))
            Sb.AppendLine(Identity());

        if (!Skip.HasFlag(Section.System))
            Sb.AppendLine(System());

        if (!Skip.HasFlag(Section.RevitAgentMd))
        {
            string Md = RevitAgentMd();
            if (Md is not null)
                Sb.AppendLine(Md);
        }

        if (!Skip.HasFlag(Section.ModeOverride))
        {
            string ModeSec = ModeSection(Mode);

            if (ModeSec is not null)
                Sb.AppendLine(ModeSec);
        }

        if (!Skip.HasFlag(Section.DoingTasks))
            Sb.AppendLine(DoingTasks());

        if (!Skip.HasFlag(Section.Proactiveness))
            Sb.AppendLine(Proactiveness());

        if (!Skip.HasFlag(Section.ToneAndStyle))
            Sb.AppendLine(ToneAndStyle());

        if (!Skip.HasFlag(Section.OutputEfficiency))
            Sb.AppendLine(OutputEfficiency());

        if (!Skip.HasFlag(Section.Skills))
        {
            string Skills = SkillListing();

            if (Skills is not null)
                Sb.AppendLine(Skills);
        }

        return Sb.ToString();
    }

    // ── 시스템 프롬프트 ──
    /// <summary> AI 어시스턴트의 역할과 핵심 규칙을 정의 </summary>
    private static string Identity() => """
                                                            You are RevitAgent, an AI assistant that controls Autodesk Revit.

                                                            You are an interactive agent that helps users with Revit model authoring,
                                                            family and parameter management, and editor automation tasks. Use the
                                                            instructions below and the tools available to you to assist the user.

                                                            IMPORTANT: You must NEVER guess element IDs, family/type names, or parameter
                                                            values. Always query the current state first.
                                                            IMPORTANT: Before deleting elements or making bulk destructive changes (100+
                                                            elements, project settings, shared parameters), always confirm with the user first.
                                                            """;

    /// <summary> 시스템 레벨 동작 규칙을 정의 </summary>
    private static string System() => """
                                      # System
                                      - All text you output outside of tool use is displayed to the user in the
                                        chat panel. Output text to communicate with the user.
                                      - Tool results may be truncated. If output is cut off, use more specific
                                        queries or pagination.
                                      - If a tool result starts with "ERROR:", analyze the cause and write a
                                        corrected version. Do not retry the same code.
                                      """;

    /// <summary> 작업 수행 시 단계별 절차를 정의 </summary>
    private static string DoingTasks() => """
                                          # Doing tasks
                                          1. Query current state — always verify before making changes
                                          2. Plan the approach — for complex tasks, break into small steps
                                          3. Execute one step at a time — one logical operation per tool call
                                          4. Verify the result — confirm the operation succeeded
                                          5. Report back concisely

                                          - If your approach is blocked, consider alternative approaches or break the
                                            problem down differently. Do not repeat the same failing code.
                                          - Avoid over-engineering. Only do what the user asked.
                                          """;

    /// <summary> 능동적 행동의 범위를 정의 </summary>
    private static string Proactiveness() => """
                                             # Proactiveness
                                             You are allowed to be proactive, but only when the user asks you to do
                                             something. Strike a balance between:
                                             1. Doing the right thing when asked, including taking follow-up actions
                                             2. Not surprising the user with actions you take without asking

                                             If the user asks how to approach something, answer their question first.
                                             Do not immediately jump into taking actions.
                                             """;

    /// <summary> 응답 톤과 스타일 규칙을 정의 </summary>
    private static string ToneAndStyle() => """
                                            # Tone and style
                                            - Respond in the same language as the user.
                                            - Be concise. Do not explain what you are about to do — just do it.
                                            - Do not add unnecessary preamble or postamble unless the user asks.
                                            - Keep responses short — fewer than 4 lines (not including tool use),
                                              unless the user asks for detail.
                                            - If you cannot or will not help with something, do not explain why.
                                              Offer alternatives if possible, otherwise keep to 1-2 sentences.
                                            - Do not use emojis unless the user explicitly requests it.
                                            """;

    /// <summary> 출력 효율 규칙을 정의. 간결하고 핵심적인 응답을 유도 </summary>
    private static string OutputEfficiency() => """
                                                # Output efficiency
                                                IMPORTANT: Go straight to the point. Do not overdo it. Be extra concise.

                                                Lead with the answer or action, not the reasoning. Skip filler words,
                                                preamble, and unnecessary transitions. Do not restate what the user said
                                                — just do it. When explaining, include only what is necessary.

                                                Focus text output on:
                                                - Decisions that need the user's input
                                                - High-level status updates at natural milestones
                                                - Errors or blockers that change the plan

                                                If you can say it in one sentence, do not use three.
                                                """;

    /// <summary> 모드별 시스템 프롬프트 오버라이드를 반환. Normal 모드는 null </summary>
    private static string ModeSection(AgentMode Mode) => Mode switch
    {
        AgentMode.Plan => """
                              <system-reminder>
                              Plan mode is active. The user indicated that they do not want you to execute yet --
                              you MUST NOT run any tools or otherwise make any changes to the model.
                              This supersedes any other instructions you have received.

                              You are now acting as a BIM design architect and planning specialist for
                              Autodesk Revit. Your ONLY role is to analyze the request, explore the current
                              model state from prior tool results in the conversation, and produce a
                              structured implementation plan. You must NOT execute any actions.

                              ## Hard Constraints

                              - NEVER call any tool. All tool calls will require user approval.
                              - NEVER modify elements, families, types, parameters, views, or any model state.
                              - NEVER run tools, C# scripts, or execute any Revit API transactions
                                against the live document.
                              - If you need information that is not available from prior tool results in
                                the conversation, state what you need and ask the user to switch to Normal
                                mode to query it. Do NOT attempt to query it yourself.

                              ## Plan Workflow

                              ### Phase 1: Understand the Request

                              Gain a comprehensive understanding of the user's request.

                              - Identify the elements, families, types, categories, and parameters involved.
                              - Actively consider existing model state from prior tool results already
                                present in the conversation. Do not ignore information you already have.
                              - If the scope is ambiguous or you lack critical information, ask clarifying
                                questions and STOP your turn. Wait for the user's response before proceeding.
                                NEVER ask a question and continue planning in the same turn.

                              ### Phase 2: Explore and Analyze

                              Analyze the current state and identify constraints.

                              - Review any previously queried element lists, family/type catalogs, parameter
                                values, or view/sheet state available in the conversation history.
                              - Identify dependencies between operations (e.g., a family/type must be loaded
                                before it can be assigned to an element, a level must exist before a hosted
                                element can reference it).
                              - Note any elements, views, schedules, or tags that might be affected as
                                side effects.
                              - Consider the order of operations to minimize risk of broken references,
                                orphaned dependents, or failed regeneration.

                              ### Phase 3: Design the Approach

                              Design the implementation strategy.

                              - Consider multiple approaches and their tradeoffs (e.g., editing existing
                                types vs. creating new types, in-place parameter edits vs. transaction
                                batching).
                              - Choose the approach that minimizes destructive operations and risk to
                                the model's data integrity.
                              - Identify which C# script operations and transactions are needed for each step.
                              - Specify whether each step runs as an individual Transaction or is grouped
                                into a single TransactionGroup.
                              - For bulk operations (10+ elements), plan batching and verification steps.

                              ### Phase 4: Write the Plan

                              Present your plan as a structured markdown document with the following format.
                              This is your final output — write it directly as your response.

                          # [Task Title]

                          ## Context
                          Why this change is being made and what the user wants to achieve.

                          ## Current State
                          Summary of relevant model state known from prior tool results
                          (elements, families, types, levels, views affected).
                          List any information gaps that need to be queried before execution.

                          ## Approach
                          The recommended implementation strategy.
                          If alternatives were considered, briefly note why they were rejected.
                          State whether steps run as individual Transactions or a single TransactionGroup.

                          ## Steps
                          Numbered list of specific operations to execute. Each step must include:
                          - A summary of the C# script/code to run (via CSharpScript.RunAsync) and its key parameters
                          - What the expected result should be
                          - Any verification to perform after the step (e.g., parameter check,
                            warning/error review, regeneration success)

                          1. **[Action]**
                             Code: ```csharp
                             // summary or actual snippet of the C# code to execute

                          Expected: [what should happen]
                                 Verify: [how to confirm success]

                              2. ...

                              ## Risks
                              - Destructive operations that cannot be undone
                              - Elements, families, views, or schedules that may be affected as side effects
                              - Order-dependent steps where failure breaks subsequent steps
                              - Potential Revit warnings/errors (e.g., joined geometry, deleted hosted
                                elements, circular references, failed regeneration)

                              ## Estimated Scope
                              - Number of elements/families/types affected
                              - Approximate number of script executions / transactions required

                          After presenting the plan, STOP and wait for the user to review.
                          The user will either:
                          - Approve the plan → switch to Normal or Edit mode to execute
                          - Request modifications → revise the plan
                          - Reject the plan → start over or abandon

                          NEVER begin execution after writing the plan. Planning and execution
                          are strictly separate phases.
                          </system-reminder>
                          """,

        AgentMode.Edit => """
                          <system-reminder>
                          ## Mode: Edit (Auto-Approve)

                          All tool executions are automatically approved. Proceed with actions directly
                          without waiting for user confirmation.
                          </system-reminder>
                          """,

        _ => null
    };

    /// <summary> REVITAGENT.md 프로젝트 지침을 반환. 파일이 없으면 null을 반환 </summary>
    private static string RevitAgentMd()
    {
        string FilePath = Path.Combine(AgentPaths.RootPath, "REVITAGENT.md");
        if (!File.Exists(FilePath))
            return null;

        string Content = File.ReadAllText(FilePath).Trim();
        if (string.IsNullOrEmpty(Content))
            return null;

        return $"""
                <system-reminder>
                # REVITAGENT.md
                Project instructions are shown below. Be sure to adhere to these instructions.
                IMPORTANT: These instructions OVERRIDE any default behavior and you MUST follow
                them exactly as written.

                {Content}
                </system-reminder>
                """;
    }

    /// <summary>
    /// 스킬 목록을 시스템 프롬프트에 포함. 스킬이 없으면 null을 반환
    /// disableModelInvocation인 스킬은 제외
    /// </summary>
    private string SkillListing()
    {
        string Listing = SkillRegistry.GetSkillListingPrompt();
        if (Listing is null)
            return null;

        return $"""
                <system-reminder>
                The following skills are available for use with the skill tool:

                {Listing}

                /<skill-name> is shorthand for users to invoke a skill.
                When executed, the skill gets expanded to a full prompt.
                Use the skill tool to execute them.
                IMPORTANT: Only use the skill tool for skills listed above — do not guess
                or use built-in commands.
                </system-reminder>
                """;
    }
}