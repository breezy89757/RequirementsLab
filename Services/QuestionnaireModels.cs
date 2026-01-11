using System.Text.Json.Serialization;

namespace RequirementsLab.Models;

public class QuestionnaireData
{
    public string Title { get; set; } = "Questions";
    public List<QuestionItem> Questions { get; set; } = new();
}

public class QuestionItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = "";
    public string Type { get; set; } = "text"; // checkbox, radio, text
    public List<string> Options { get; set; } = new();
    
    // User Answer State
    [JsonIgnore]
    public string AnswerText { get; set; } = "";
    [JsonIgnore]
    public HashSet<string> SelectedOptions { get; set; } = new();
}
