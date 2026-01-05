using RequirementsLab.Models;

namespace RequirementsLab.Services;

/// <summary>
/// 產生可運行的 POC HTML 檔案
/// </summary>
public class PocTemplateService
{
    public string GeneratePocHtml(RequirementInput input, SolutionOption solution)
    {
        var systemPrompt = GenerateSystemPrompt(input, solution);
        
        return $$"""
            <!DOCTYPE html>
            <html lang="zh-TW">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>POC: {{solution.Name}}</title>
                <style>
                    * { box-sizing: border-box; margin: 0; padding: 0; }
                    body { 
                        font-family: 'Segoe UI', sans-serif; 
                        background: linear-gradient(135deg, #1e3a5f 0%, #0f172a 100%);
                        min-height: 100vh;
                        padding: 2rem;
                        color: #e2e8f0;
                    }
                    .container { max-width: 800px; margin: 0 auto; }
                    h1 { margin-bottom: 0.5rem; }
                    .subtitle { color: #94a3b8; margin-bottom: 2rem; }
                    .config-panel {
                        background: rgba(30, 41, 59, 0.8);
                        padding: 1rem;
                        border-radius: 8px;
                        margin-bottom: 1rem;
                        border: 1px solid #334155;
                    }
                    .config-panel input {
                        width: 100%;
                        padding: 0.5rem;
                        background: #0f172a;
                        border: 1px solid #475569;
                        border-radius: 4px;
                        color: #e2e8f0;
                        margin-top: 0.5rem;
                    }
                    .chat-container {
                        background: rgba(30, 41, 59, 0.8);
                        border-radius: 12px;
                        padding: 1.5rem;
                        border: 1px solid #334155;
                    }
                    #messages {
                        height: 300px;
                        overflow-y: auto;
                        margin-bottom: 1rem;
                        padding: 1rem;
                        background: #0f172a;
                        border-radius: 8px;
                    }
                    .message { margin-bottom: 1rem; padding: 0.75rem; border-radius: 8px; }
                    .user { background: #3b82f6; margin-left: 20%; }
                    .assistant { background: #475569; margin-right: 20%; }
                    .input-row { display: flex; gap: 0.5rem; }
                    #userInput { 
                        flex: 1; 
                        padding: 0.75rem; 
                        border-radius: 8px; 
                        border: 1px solid #475569;
                        background: #0f172a;
                        color: #e2e8f0;
                    }
                    button {
                        padding: 0.75rem 1.5rem;
                        background: #3b82f6;
                        color: white;
                        border: none;
                        border-radius: 8px;
                        cursor: pointer;
                        font-weight: 600;
                    }
                    button:hover { background: #2563eb; }
                    button:disabled { background: #475569; cursor: not-allowed; }
                    .loading { opacity: 0.7; }
                </style>
            </head>
            <body>
                <div class="container">
                    <h1>🧪 {{solution.Name}}</h1>
                    <p class="subtitle">{{input.Goal}}</p>

                    <div class="config-panel">
                        <label>🔑 Azure OpenAI Endpoint</label>
                        <input type="text" id="endpoint" placeholder="https://YOUR_RESOURCE.openai.azure.com/openai/deployments/YOUR_DEPLOYMENT/chat/completions?api-version=2024-08-01-preview">
                        <label style="margin-top: 0.5rem; display: block;">🔐 API Key</label>
                        <input type="password" id="apiKey" placeholder="Your API Key">
                    </div>

                    <div class="chat-container">
                        <div id="messages"></div>
                        <div class="input-row">
                            <input type="text" id="userInput" placeholder="輸入您的問題..." onkeypress="if(event.key==='Enter')sendMessage()">
                            <button onclick="sendMessage()" id="sendBtn">發送</button>
                        </div>
                    </div>
                </div>

                <script>
                    const SYSTEM_PROMPT = `{{systemPrompt}}`;
                    let conversationHistory = [{ role: "system", content: SYSTEM_PROMPT }];

                    async function sendMessage() {
                        const input = document.getElementById('userInput');
                        const messages = document.getElementById('messages');
                        const sendBtn = document.getElementById('sendBtn');
                        const endpoint = document.getElementById('endpoint').value;
                        const apiKey = document.getElementById('apiKey').value;

                        if (!input.value.trim() || !endpoint || !apiKey) {
                            alert('請填寫 Endpoint、API Key 和訊息');
                            return;
                        }

                        // Add user message
                        conversationHistory.push({ role: "user", content: input.value });
                        messages.innerHTML += `<div class="message user">${input.value}</div>`;
                        input.value = '';
                        sendBtn.disabled = true;
                        sendBtn.textContent = '思考中...';

                        try {
                            const response = await fetch(endpoint, {
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/json',
                                    'api-key': apiKey
                                },
                                body: JSON.stringify({ messages: conversationHistory })
                            });

                            const data = await response.json();
                            const assistantMessage = data.choices?.[0]?.message?.content || '無回應';
                            
                            conversationHistory.push({ role: "assistant", content: assistantMessage });
                            messages.innerHTML += `<div class="message assistant">${assistantMessage}</div>`;
                            messages.scrollTop = messages.scrollHeight;
                        } catch (error) {
                            messages.innerHTML += `<div class="message assistant" style="background:#dc2626;">錯誤: ${error.message}</div>`;
                        } finally {
                            sendBtn.disabled = false;
                            sendBtn.textContent = '發送';
                        }
                    }
                </script>
            </body>
            </html>
            """;
    }

    private string GenerateSystemPrompt(RequirementInput input, SolutionOption solution)
    {
        return $"""
            你是一個專門協助使用者「{input.Goal}」的 AI 助手。

            ## 背景
            - 使用者原本的做法: {input.CurrentProcess}
            - 資料來源: {input.DataSource}

            ## 你的任務
            - 回答使用者關於「{input.InputType}」的問題
            - 提供「{input.ExpectedOutput}」格式的回應
            - 確保達成「{input.SuccessCriteria}」的標準

            ## 回應原則
            - 使用繁體中文
            - 簡潔明瞭
            - 如果不確定，請誠實說明
            """.Replace("\"", "\\\"").Replace("\n", "\\n");
    }
}
