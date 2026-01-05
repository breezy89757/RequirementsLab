using System.ClientModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using RequirementsLab.Models;

namespace RequirementsLab.Services;

/// <summary>
/// LLM 1: 根據需求產生解決方案選項
/// </summary>
public class SolutionGeneratorService
{
    private readonly IChatClient _chatClient;

    public SolutionGeneratorService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<List<SolutionOption>> GenerateSolutionsAsync(RequirementInput input)
    {
        var prompt = $"""
            你是一位資深的 AI 解決方案架構師。
            請根據以下需求，提出 3 種可行的 LLM 解決方案。

            ## 需求資訊
            - **目標**: {input.Goal}
            - **現行流程**: {input.CurrentProcess}
            - **輸入類型**: {input.InputType}
            - **期望輸出**: {input.ExpectedOutput}
            - **資料來源**: {input.DataSource}
            - **成功標準**: {input.SuccessCriteria}

            ## 輸出格式 (JSON Array)
            請回傳一個 JSON 陣列，包含 3 個方案，每個方案包含：
            - name: 方案名稱 (例如: "RAG + Azure OpenAI")
            - description: 簡短說明 (50 字內)
            - complexity: "Low" | "Medium" | "High"
            - estimatedCost: 成本估算說明
            - risks: 主要風險
            - pocTemplateType: "chat" | "rag" | "form"

            只回傳 JSON，不要有其他文字。
            """;

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, prompt)
        };

        var response = await _chatClient.GetResponseAsync(messages);
        var json = response.Text ?? "[]";

        // 清理 JSON (移除 markdown code block 如果有)
        json = json.Replace("```json", "").Replace("```", "").Trim();

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<SolutionOption>>(json, options) ?? new();
        }
        catch
        {
            return new List<SolutionOption>
            {
                new SolutionOption { Name = "解析失敗", Description = json }
            };
        }
    }
}

/// <summary>
/// LLM 2: 評估方案可行性 (PromptAgent 模式)
/// </summary>
public class FeasibilityEvaluatorService
{
    private readonly IChatClient _chatClient;

    public FeasibilityEvaluatorService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<SolutionOption> EvaluateAsync(RequirementInput input, SolutionOption solution)
    {
        var prompt = $"""
            你是一位嚴格的技術評審。請評估以下 LLM 解決方案的可行性。

            ## 原始需求
            - 目標: {input.Goal}
            - 成功標準: {input.SuccessCriteria}

            ## 待評估方案
            - 名稱: {solution.Name}
            - 說明: {solution.Description}
            - 複雜度: {solution.Complexity}
            - 風險: {solution.Risks}

            ## 評估標準
            1. 技術可行性 (能否用現有技術實現)
            2. 資料可得性 (資料來源是否可取得)
            3. 成本合理性 (成本是否符合預期效益)
            4. 時程可控性 (能否在合理時間內完成)

            ## 輸出格式 (JSON)
            回傳一個 JSON 物件，包含：
            - feasibilityScore: 0 到 100 的整數分數
            - recommendation: 簡短建議 (50 字內)

            只回傳 JSON。
            """;

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, prompt)
        };

        var response = await _chatClient.GetResponseAsync(messages);
        var json = response.Text ?? "{}";
        json = json.Replace("```json", "").Replace("```", "").Trim();

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<EvalResult>(json, options);
            solution.FeasibilityScore = result?.FeasibilityScore ?? 0;
            solution.Recommendation = result?.Recommendation ?? "";
        }
        catch
        {
            solution.FeasibilityScore = 50;
            solution.Recommendation = "評估失敗，請人工審查";
        }

        return solution;
    }

    private class EvalResult
    {
        public int FeasibilityScore { get; set; }
        public string Recommendation { get; set; } = "";
    }
}
