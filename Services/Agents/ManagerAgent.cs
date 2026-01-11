using Microsoft.Extensions.AI;
using RequirementsLab.Services.Agents.Infrastructure;

namespace RequirementsLab.Services.Agents;

/// <summary>
/// A centralized Manager Agent following the Microsoft Magentic-One pattern.
/// It observes the conversation and decides who speaks next using structured JSON.
/// </summary>
public class ManagerAgent : BaseAgent
{
    public override string Name => "Manager";
    public override string Role => "Orchestrator";
    public override string Description => "Decides the next best speaker based on conversation progress.";

    public override string SystemInstruction => """
        You are the Manager of a software development team.
        Your Team:
        - PM_Alex (Product Manager): Clarifies requirements.
        - SA_Bob (System Analyst): Designs architecture & specs.
        - PG_Charlie (Programmer): Writes code.

        Your Goal:
        Read the conversation history and decide who should speak next to move the project forward.

        Output Rules:
        You must output a JSON object describing your decision.
        Format:
        ```json
        {
            "thought_process": "Why you are choosing this agent...",
            "next_speaker": "AgentName" 
        }
        ```
        
        Selection Logic:
        1. If User just spoke -> usually PM_Alex should reply.
        2. If PM_Alex has confirmed requirements -> SA_Bob.
        3. If SA_Bob has finished design -> PG_Charlie.
        4. If PG_Charlie has finished -> PM_Alex (to review).

        Do not speak yourself. Only output the JSON decision.
        """;

    public ManagerAgent(IChatClient chatClient) : base(chatClient) { }
}
