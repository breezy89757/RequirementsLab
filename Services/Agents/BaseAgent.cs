using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace RequirementsLab.Services.Agents;

public abstract class BaseAgent : IAgent
{
    private readonly IChatClient _chatClient;
    
    public abstract string Name { get; }
    public abstract string Role { get; }
    public abstract string Description { get; }
    public abstract string SystemInstruction { get; }

    protected BaseAgent(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public virtual async Task<string> RespondAsync(IEnumerable<ChatMessage> history, CancellationToken ct = default)
    {
        // 1. Construct messages with System Prompt
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, SystemInstruction)
        };
        
        // 2. Append history
        // Note: In a real "Group Chat", we need to format the history so the LLM knows WHO spoke.
        // e.g., "User: Hello", "PM: What is your goal?"
        // Since Microsoft.Extensions.AI.ChatMessage has AuthorName, we assume the LLM supports it (GPT-4 does).
        messages.AddRange(history);

        // 3. Call LLM
        var response = await _chatClient.GetResponseAsync(messages, cancellationToken: ct);
        
        return response.Text ?? "";
    }

    public virtual async IAsyncEnumerable<string> RespondStreamAsync(IEnumerable<ChatMessage> history, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        // 1. Construct messages with System Prompt
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, SystemInstruction)
        };
        messages.AddRange(history);

        // 2. Call LLM (Streaming)
        // We use explicit enumerator to handle exceptions since 'yield' cannot be in 'try-catch'
        var stream = _chatClient.GetStreamingResponseAsync(messages, cancellationToken: ct);
        var enumerator = stream.GetAsyncEnumerator(ct);
        
        bool hasNext = true;
        string? errorMessage = null;

        while (hasNext)
        {
            try
            {
                hasNext = await enumerator.MoveNextAsync();
            }
            catch (System.ClientModel.ClientResultException ex) when (ex.Message.Contains("content_filter") || ex.Status == 400)
            {
                errorMessage = "\n\n⚠️ **(Content Filtered)**: The response was blocked by Azure OpenAI's content safety policy. Please adjust your prompt.\n";
                hasNext = false;
            }
            catch (Exception ex)
            {
                errorMessage = $"\n\n🛑 **(Error)**: {ex.Message}\n";
                hasNext = false;
            }

            if (hasNext && enumerator.Current?.Text != null)
            {
                yield return enumerator.Current.Text;
            }
        }

        if (errorMessage != null)
        {
            yield return errorMessage;
        }
    }
}
