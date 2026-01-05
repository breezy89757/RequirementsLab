namespace RequirementsLab.Models;

/// <summary>
/// PM 輸入的需求評估表單
/// </summary>
public class RequirementInput
{
    /// <summary>User 想達成什麼目標</summary>
    public string Goal { get; set; } = "";

    /// <summary>現在怎麼做 (現行流程)</summary>
    public string CurrentProcess { get; set; } = "";

    /// <summary>使用者會輸入什麼 (Input 類型)</summary>
    public string InputType { get; set; } = "";

    /// <summary>期望得到什麼輸出</summary>
    public string ExpectedOutput { get; set; } = "";

    /// <summary>資料來源 (PDF, DB, API...)</summary>
    public string DataSource { get; set; } = "";

    /// <summary>成功標準</summary>
    public string SuccessCriteria { get; set; } = "";
}

/// <summary>
/// LLM 產出的解決方案選項
/// </summary>
public class SolutionOption
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Complexity { get; set; } = "Medium"; // Low/Medium/High
    public string EstimatedCost { get; set; } = "";
    public string Risks { get; set; } = "";
    public int FeasibilityScore { get; set; } = 0; // 0-100
    public string Recommendation { get; set; } = "";
    public string PocTemplateType { get; set; } = "chat"; // chat, rag, form
}

/// <summary>
/// 完整分析結果
/// </summary>
public class AnalysisResult
{
    public RequirementInput Input { get; set; } = new();
    public List<SolutionOption> Solutions { get; set; } = new();
    public string Summary { get; set; } = "";
}
