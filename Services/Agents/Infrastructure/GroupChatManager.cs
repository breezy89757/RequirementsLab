using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace RequirementsLab.Services.Agents.Infrastructure;

/// <summary>
/// Manages a conversation between multiple agents.
/// Implements the "Group Chat" pattern.
/// </summary>
public class GroupChatManager
{
    private readonly List<IAgent> _agents = new();
    private readonly List<ChatMessage> _history = new();
    
    // State
    public IReadOnlyList<ChatMessage> History => _history;
    public IAgent? CurrentSpeaker { get; private set; }

    public void AddAgent(IAgent agent)
    {
        _agents.Add(agent);
    }

    public void AddUserMessage(string message)
    {
        _history.Add(new ChatMessage(ChatRole.User, message));
    }

    /// <summary>
    /// Selects the next speaker. 
    /// Currently implements a simple Round-Robin or Sequential flow based on setup.
    /// Future: Use LLM to select based on _agents.Description.
    /// </summary>
    public IAgent? SelectNextSpeaker()
    {
        if (_agents.Count == 0) return null;
        
        var lastMsg = _history.LastOrDefault();

        // 1. If start of conversation (or last was User), always PM (Index 0)
        if (lastMsg == null || lastMsg.Role == ChatRole.User)
        {
            return _agents.FirstOrDefault(a => a.Name.Contains("PM")) ?? _agents.First();
        }

        // 2. Check for Explicit Handoff (e.g., "NEXT: SA_Bob")
        var content = lastMsg.Text ?? "";
        
        // Strategy A: Explicit Token (Highest Priority for reliability)
        if (content.Contains("NEXT: "))
        {
            var targetName = content.Split("NEXT: ").Last().Trim().Split(' ', '\n').First(); 
            var targetAgent = FindAgentByName(targetName);
            if (targetAgent != null) return targetAgent;
        }

        // Strategy B: Magentic JSON Routing (Dynamic Intelligent Selection)
        // Look for JSON pattern: { "next_speaker": "SA_Bob" } or similar
        try 
        {
            if (content.Trim().EndsWith("}") || content.Contains("```json"))
            {
                // Simple regex or string search to avoid heavy JSON parsing dependency for now
                // We look for "next_speaker": "NAME"
                var jsonFragment = content;
                if (jsonFragment.Contains("\"next_speaker\""))
                {
                    // Extract value manually to be robust
                    var parts = jsonFragment.Split(new[] { "\"next_speaker\"" }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        var valuePart = parts[1].Trim().TrimStart(':').Trim();
                        var targetName = valuePart.Trim('"', '\'', ' ', ',', '\n', '\r');
                        
                        var targetAgent = FindAgentByName(targetName);
                        if (targetAgent != null) return targetAgent;
                    }
                }
            }
        }
        catch { /* Ignore JSON parse errors */ }

        // 3. Default Chain Rules (Fallback if no explicit handoff)
        // If SA talks, PG usually follows? Or just stop?
        // Let's implement the specific chain: PM -> [Wait User] | SA -> PG -> [Stop]
        
        var lastSender = lastMsg.AuthorName;
        
        if (lastSender != null && lastSender.Contains("SA_")) // SA finished -> PG
        {
            return _agents.FirstOrDefault(a => a.Name.Contains("PG"));
        }
        
        // If PM finished and didn't trigger Handoff -> Stop (Wait for User)
        if (lastSender != null && lastSender.Contains("PM_"))
        {
            return null; 
        }

        // If PG finished -> Stop
        if (lastSender != null && lastSender.Contains("PG_"))
        {
            return null;
        }

        return null;
    }

    public async Task<string> StepAsync(CancellationToken ct = default)
    {
        var speaker = SelectNextSpeaker();
        if (speaker == null) return null; // End of turn

        CurrentSpeaker = speaker;
        
        // Execute the agent
        // Note: In a real system, we would stream this.
        var response = await speaker.RespondAsync(_history, ct);

        // Add to history
        _history.Add(new ChatMessage(ChatRole.Assistant, response) { AuthorName = speaker.Name });

        return response;
    }

    public async IAsyncEnumerable<string> StepStreamAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var speaker = SelectNextSpeaker();
        if (speaker == null) yield break;

        CurrentSpeaker = speaker;
        // Yield a special marker or just start streaming. 
        // For simplicity, we assume the UI knows CurrentSpeaker changed via a separate check or we just stream content.
        
        var fullResponse = "";
        
        await foreach (var token in speaker.RespondStreamAsync(_history, ct))
        {
            fullResponse += token;
            yield return token;
        }

        // Add to history
        _history.Add(new ChatMessage(ChatRole.Assistant, fullResponse) { AuthorName = speaker.Name });
    }

    private IAgent? FindAgentByName(string name)
    {
        // Remove common artifacts
        name = name.Replace("_", "").Replace(" ", ""); 
        
        return _agents.FirstOrDefault(a => 
            a.Name.Replace("_", "").Equals(name, StringComparison.OrdinalIgnoreCase) ||
            a.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
            name.Contains(a.Name, StringComparison.OrdinalIgnoreCase));
    }
}
