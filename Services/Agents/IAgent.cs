using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace RequirementsLab.Services.Agents;

public interface IAgent
{
    string Name { get; }
    string Role { get; } // e.g., "PM", "SA", "PG"
    string Description { get; } // For the selector to decide who speaks next

    // The core "Brain" function
    Task<string> RespondAsync(IEnumerable<ChatMessage> history, CancellationToken ct = default);

    // Streaming support
    IAsyncEnumerable<string> RespondStreamAsync(IEnumerable<ChatMessage> history, CancellationToken ct = default);
}
