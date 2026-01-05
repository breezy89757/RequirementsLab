using Microsoft.Extensions.AI;
using RequirementsLab.Models;

namespace RequirementsLab.Services;

/// <summary>
/// 產生 SA 可用的實作規劃書 (含 Mermaid 架構圖)
/// </summary>
public class ImplementationPlanService
{
    private readonly IChatClient _chatClient;

    public ImplementationPlanService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> GeneratePlanAsync(RequirementInput input, SolutionOption solution)
    {
        var prompt = $"""
            你是一位資深的解決方案架構師 (SA)。
            請根據以下需求和選定的方案，產出一份完整的「實作規劃書」。

            ## 原始需求
            - **目標**: {input.Goal}
            - **現行流程**: {input.CurrentProcess}
            - **輸入類型**: {input.InputType}
            - **期望輸出**: {input.ExpectedOutput}
            - **資料來源**: {input.DataSource}
            - **成功標準**: {input.SuccessCriteria}

            ## 選定方案
            - **名稱**: {solution.Name}
            - **說明**: {solution.Description}
            - **複雜度**: {solution.Complexity}

            ## 輸出格式 (Markdown)

            # 實作規劃書：{solution.Name}

            ## 1. 方案摘要
            (2-3 句話說明核心概念)

            ## 2. 系統架構圖

            注意：Mermaid 語法規則：
            - 使用英文節點 ID，例如 A, B, C
            - 節點標籤放在方括號內：A[用戶介面]
            - 箭頭使用 -->
            - 不要使用中文字元作為節點 ID
            - 不要使用 <br/> 或特殊字元

            範例格式：
            ```mermaid
            flowchart LR
                A[使用者] --> B[Web UI]
                B --> C[API Server]
                C --> D[LLM Service]
                D --> E[Vector DB]
            ```

            ## 3. 資料流程
            (說明資料如何流動)

            ## 4. 技術選型
            | 元件 | 技術選擇 | 說明 |
            |------|----------|------|

            ## 5. 開發時程估算
            | 階段 | 工作內容 | 預估時間 |
            |------|----------|----------|

            ## 6. 風險分析
            - 風險：...
            - 緩解措施：...

            ## 7. 下一步行動
            - [ ] SA：...
            - [ ] PG：...

            ---
            請直接輸出 Markdown，確保 Mermaid 語法正確。
            """;

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, prompt)
        };

        var response = await _chatClient.GetResponseAsync(messages);
        return response.Text ?? "無法產生規劃書";
    }
}
