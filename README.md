# 🧪 RequirementsLab

**LLM 需求評估與實作規劃工具** — 幫助 PM 快速評估 AI 需求可行性，並產出 SA 可用的實作規劃書。

## ✨ 功能特色

- 📝 **需求輸入表單** — 結構化收集專案需求
- 🤖 **雙 LLM 架構** — LLM 1 產生方案、LLM 2 評估可行性
- 📊 **可行性評分** — 0-100 分自動評估
- 📋 **實作規劃書** — 含 Mermaid 架構圖、技術選型、時程估算

## 🚀 快速開始

### 1. 設定 Azure OpenAI

```bash
cd RequirementsLab
copy appsettings.template.json appsettings.json
```

編輯 `appsettings.json`：
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4o"
  }
}
```

### 2. 執行

```bash
dotnet run
```

瀏覽器開啟 http://localhost:5272

## 📦 技術棧

- .NET 9 / Blazor Server
- Microsoft.Extensions.AI (MAI)
- Azure OpenAI
- Markdig (Markdown 渲染)
- Mermaid.js (架構圖)

## 📁 專案結構

```
RequirementsLab/
├── Components/Pages/
│   ├── Home.razor      # 需求輸入表單
│   ├── Analysis.razor  # 方案分析結果
│   └── Plan.razor      # 實作規劃書
├── Services/
│   ├── LlmServices.cs              # LLM 1 + LLM 2
│   └── ImplementationPlanService.cs # 規劃書生成
├── Models/
│   └── RequirementModels.cs        # 資料模型
└── wwwroot/app.css                 # 主題樣式
```

## 📸 流程

```
首頁 (填表) → 分析結果 (3方案+分數) → 實作規劃書 (Mermaid架構圖)
```

## 📄 授權

MIT License
