using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using RequirementsLab.Models;

namespace RequirementsLab.Services;

public class QuestionnaireService
{
    private readonly IChatClient _chatClient;

    public QuestionnaireService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<QuestionnaireData?> ParseToQuestionnaireAsync(string text)
    {
        var systemPrompt = """
            You are a Form Parser. Convert the following text into a JSON Schema for a questionnaire.
            Extract every question asked in the text.
            
            Schema:
            {
                "title": "Title of the form (e.g. 'Clarification Questions')",
                "questions": [
                    {
                        "id": "q1",
                        "text": "Question text?",
                        "type": "checkbox" | "radio" | "text",
                        "options": ["Option A", "Option B"] (only for checkbox/radio)
                    }
                ]
            }

            Rules:
            - **PREFER SELECTION OVER TYPING**: Whenever possible, convert open-ended questions into "radio" or "checkbox" by inferring common options.
            - Example: "What is the timeline?" -> type: "radio", options: ["1 Week", "2 Weeks", "1 Month", "Custom"]
            - Example: "Which database?" -> type: "radio", options: ["SQL Server", "PostgreSQL", "MongoDB", "Other"]
            - Only use "text" if there are absolutely no predictable options.
            - If text has "[ ]" -> "checkbox".
            - If text has "( )" or "select one" -> "radio".
            - Output ONLY raw JSON.
            """;

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, text)
        };

        // Use GetResponseAsync as seen in LlmServices.cs
        var response = await _chatClient.GetResponseAsync(messages);

        var json = response.Text?.Trim();
        if (string.IsNullOrEmpty(json)) return null;

        // Clean markdown
        if (json.StartsWith("```json")) json = json[7..].Trim();
        if (json.EndsWith("```")) json = json[..^3].Trim();

        try
        {
            var data = JsonSerializer.Deserialize<QuestionnaireData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (data?.Questions != null)
            {
                foreach (var q in data.Questions)
                {
                    q.Type = q.Type?.ToLowerInvariant() ?? "text";
                }
            }
            return data;
        }
        catch (JsonException)
        {
            return null; // Parse failed
        }
    }
}


