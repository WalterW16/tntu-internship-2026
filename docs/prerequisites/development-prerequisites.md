# Development Prerequisites

## Team Task Board — TNTU Internship 2026

Complete every item in this checklist **before Week 1, Day 1**. If you are blocked on any step, ask your mentor — do not skip verification.

---

## Table of Contents

1. [Target Audience](#target-audience)
2. [Hardware Requirements](#hardware-requirements)
3. [Operating System Support](#operating-system-support)
4. [Required Software](#required-software)
5. [Optional Software](#optional-software)
6. [Azure Account Setup](#azure-account-setup)
7. [GitHub Setup](#github-setup)
8. [Azure Resources Checklist](#azure-resources-checklist)
9. [Environment Variables](#environment-variables)
10. [Verification Steps](#verification-steps)
11. [Free-Tier Limitations](#free-tier-limitations)
12. [Cost Guardrails](#cost-guardrails)
13. [Troubleshooting](#troubleshooting)

---

## Target Audience

This project is designed for **3rd–4th year computer science students** with:

| Skill | Minimum level |
|-------|---------------|
| C# | Variables, classes, async/await, LINQ basics |
| HTTP | Know GET, POST, PUT, DELETE and status codes |
| Git | Clone, commit, push, pull request |
| Command line | Run basic commands in PowerShell or bash |

No prior experience with Azure, EF Core, or microservices is required — you will learn these during the internship.

**Recommended pre-reading:**

- [C# documentation](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [REST API concepts](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)

---

## Hardware Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| RAM | 8 GB | 16 GB (Cosmos DB Emulator + IDE) |
| Disk space | 10 GB free | 20 GB free |
| CPU | 4 cores | 4+ cores |
| Internet | Stable broadband | Required for Azure and GitHub |

---

## Operating System Support

| OS | Support level | Notes |
|----|---------------|-------|
| **Windows 10/11** | Primary | Full support including Cosmos DB Emulator |
| **macOS** | Supported | Use cloud Cosmos DB for local dev; no official emulator |
| **Linux** | Supported | Use cloud Cosmos DB for local dev; [Linux emulator preview](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux) available |

---

## Required Software

Install and verify each item. Links point to official documentation.

### 1. .NET 8 SDK

| | |
|---|---|
| **Purpose** | Build and run ASP.NET Core APIs |
| **Download** | [Install .NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) |
| **Docs** | [.NET 8 overview](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8) |
| **Verify** | `dotnet --version` → `8.0.x` |

### 2. IDE — choose one

#### Option A: Visual Studio 2022 (recommended for Windows)

| | |
|---|---|
| **Purpose** | Full-featured .NET IDE with debugging |
| **Download** | [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/community/) |
| **Workload** | **ASP.NET and web development** |
| **Docs** | [Visual Studio docs](https://learn.microsoft.com/en-us/visualstudio/) |

#### Option B: Visual Studio Code + C# Dev Kit

| | |
|---|---|
| **Purpose** | Lightweight cross-platform editor |
| **Download** | [VS Code](https://code.visualstudio.com/) |
| **Extensions** | C# Dev Kit, REST Client or Thunder Client |
| **Docs** | [C# in VS Code](https://code.visualstudio.com/docs/languages/csharp) |

### 3. Git

| | |
|---|---|
| **Purpose** | Version control |
| **Download** | [Git for Windows](https://git-scm.com/download/win) / [Git for macOS](https://git-scm.com/download/mac) |
| **Docs** | [Git documentation](https://git-scm.com/doc) |
| **Verify** | `git --version` |

### 4. Azure CLI

| | |
|---|---|
| **Purpose** | Create and manage Azure resources from terminal |
| **Download** | [Install Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) |
| **Docs** | [Azure CLI reference](https://learn.microsoft.com/en-us/cli/azure/) |
| **Verify** | `az --version` and `az login` |

### 5. GitHub account

| | |
|---|---|
| **Purpose** | Source control hosting and CI/CD |
| **Sign up** | [GitHub](https://github.com/join) |
| **Docs** | [GitHub Docs](https://docs.github.com/en/get-started) |

### 6. API testing tool — choose one

| Tool | Link |
|------|------|
| Postman | [Postman downloads](https://www.postman.com/downloads/) |
| Bruno (open source) | [Bruno](https://www.usebruno.com/) |
| VS Code REST Client | [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) |

---

## Optional Software

Install these when you reach the relevant week or if your mentor recommends them.

| Software | When to install | Documentation |
|----------|-----------------|---------------|
| **Cosmos DB Emulator** | Week 1 (Windows) | [Local emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator) |
| **Docker Desktop** | Week 4 (optional) | [Docker Desktop](https://docs.docker.com/desktop/) |
| **Application Insights** | Week 3 | Configured in Azure Portal — [US-019](../user-stories/US-019-application-insights.md) |
| **FluentAssertions** | Week 1 | NuGet package — [FluentAssertions](https://fluentassertions.com/) |

---

## Azure Account Setup

### Step 1: Create an Azure account

Choose one option:

| Option | Link | Benefit |
|--------|------|---------|
| **Azure for Students** | [Azure for Students](https://azure.microsoft.com/en-us/free/students/) | $100 credit, no credit card for verified students |
| **Azure free account** | [Azure free account](https://azure.microsoft.com/en-us/free/) | $200 credit for 30 days + 12 months free services |

**Docs:** [Azure account types](https://learn.microsoft.com/en-us/azure/cost-management-billing/manage/create-subscription)

### Step 2: Sign in and verify subscription

```powershell
az login
az account list --output table
az account set --subscription "<your-subscription-name>"
```

### Step 3: Enable Cosmos DB free tier

> **Important:** Free tier can only be enabled **when creating** the Cosmos DB account. It cannot be enabled later.

When creating the Cosmos DB account in Azure Portal:

1. Open **Create Azure Cosmos DB account**
2. Check **Apply Free Tier Discount**
3. Select API: **Azure Cosmos DB for NoSQL**

**Docs:** [Cosmos DB free tier](https://learn.microsoft.com/en-us/azure/cosmos-db/free-tier)

### Step 4: Register resource providers (if needed)

```powershell
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.DocumentDB
az provider register --namespace Microsoft.Insights
```

---

## GitHub Setup

### Repository access

- [ ] You have been added to the internship GitHub organization or repository
- [ ] You can clone the repo: `git clone <repository-url>`
- [ ] GitHub Actions is enabled (Settings → Actions → General → Allow all actions)

### Branch protection (mentor configures)

- [ ] `main` branch requires pull request before merge
- [ ] CI workflow must pass before merge

### Secrets for deployment (Week 3 — mentor assists)

For OIDC deployment (recommended):

| Secret / Variable | Purpose |
|-------------------|---------|
| `AZURE_CLIENT_ID` | App registration client ID |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |

For publish profile (alternative):

| Secret | Purpose |
|--------|---------|
| `AZURE_WEBAPP_PUBLISH_PROFILE_PROJECTS` | Projects.Api deployment |
| `AZURE_WEBAPP_PUBLISH_PROFILE_TASKS` | Tasks.Api deployment |

**Docs:**

- [GitHub Actions secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [Deploy to Azure from GitHub Actions (OIDC)](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure)

---

## Azure Resources Checklist

Your mentor may provision shared resources. If you provision your own, create these in Week 3:

| # | Resource | Type | SKU / tier |
|---|----------|------|------------|
| 1 | `rg-tntu-taskboard` | Resource group | — |
| 2 | `cosmos-tntu-taskboard` | Cosmos DB account | Free tier enabled |
| 3 | `TaskBoard` | Cosmos DB database | — |
| 4 | `projects` | Cosmos DB container | Partition key: `/id` |
| 5 | `tasks` | Cosmos DB container | Partition key: `/projectId` |
| 6 | `asp-tntu-taskboard` | App Service plan | F1 (Free) |
| 7 | `app-tntu-projects-api` | Web App | F1 |
| 8 | `app-tntu-tasks-api` | Web App | F1 |
| 9 | `appi-tntu-taskboard` | Application Insights | Free (required — [US-019](../user-stories/US-019-application-insights.md)) |

### Quick-create with Azure CLI (reference)

```powershell
# Variables — change to your names
$rg = "rg-tntu-taskboard"
$location = "westeurope"
$cosmos = "cosmos-tntu-taskboard"

az group create --name $rg --location $location

# Create Cosmos DB with free tier (Portal recommended for free tier checkbox)
# See: https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-manage-database-account

az appservice plan create --name asp-tntu-taskboard --resource-group $rg --sku F1
az webapp create --name app-tntu-projects-api --resource-group $rg --plan asp-tntu-taskboard --runtime "DOTNET:8"
az webapp create --name app-tntu-tasks-api --resource-group $rg --plan asp-tntu-taskboard --runtime "DOTNET:8"
```

**Docs:**

- [Create Cosmos DB account](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-manage-database-account)
- [Create App Service](https://learn.microsoft.com/en-us/azure/app-service/)

---

## Environment Variables

### Local development (`appsettings.Development.json`)

**Projects.Api:**

```json
{
  "CosmosDb": {
    "Endpoint": "https://localhost:8081",
    "Key": "<emulator-primary-key>",
    "DatabaseName": "TaskBoard",
    "ProjectsContainer": "projects"
  }
}
```

> Emulator default key: see [Connect to emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator)

**Tasks.Api:**

```json
{
  "CosmosDb": {
    "Endpoint": "https://localhost:8081",
    "Key": "<emulator-primary-key>",
    "DatabaseName": "TaskBoard",
    "TasksContainer": "tasks"
  },
  "ProjectsApi": {
    "BaseUrl": "https://localhost:7001"
  }
}
```

### Azure App Service (Configuration → Application settings)

| Setting | Service | Example value |
|---------|---------|---------------|
| `CosmosDb__Endpoint` | Both | `https://cosmos-tntu-taskboard.documents.azure.com:443/` |
| `CosmosDb__Key` | Both | Primary key from Cosmos account |
| `CosmosDb__DatabaseName` | Both | `TaskBoard` |
| `CosmosDb__ProjectsContainer` | Projects.Api | `projects` |
| `CosmosDb__TasksContainer` | Tasks.Api | `tasks` |
| `ProjectsApi__BaseUrl` | Tasks.Api | `https://app-tntu-projects-api.azurewebsites.net` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Both | Connection string from Application Insights resource |

> Use double underscore `__` for nested configuration in Azure App Service. Application Insights uses the standard `APPLICATIONINSIGHTS_CONNECTION_STRING` name (single underscores).

**Docs:** [ASP.NET Core configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

### Never commit secrets

Add to `.gitignore`:

```
appsettings.Development.json
appsettings.*.local.json
*.user
```

Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development:

```powershell
cd src/Projects.Api
dotnet user-secrets init
dotnet user-secrets set "CosmosDb:Key" "<your-key>"
```

---

## Verification Steps

Run every check and record the result. All must pass before starting development.

| # | Check | Command / action | Expected result |
|---|-------|------------------|-----------------|
| 1 | .NET SDK | `dotnet --version` | `8.0.x` |
| 2 | Git | `git --version` | Version displayed |
| 3 | Azure CLI | `az --version` | Version displayed |
| 4 | Azure login | `az login` | Login successful |
| 5 | Subscription | `az account show` | Correct subscription |
| 6 | Clone repo | `git clone <url>` | Repository cloned |
| 7 | Create API project | `dotnet new webapi -n TestApi` | Project created |
| 8 | Run API | `dotnet run` in TestApi | Listening on localhost |
| 9 | Cosmos emulator | Open Data Explorer at `https://localhost:8081/_explorer/index.html` | UI loads (Windows) |
| 10 | GitHub Actions | Push a branch, open PR | CI workflow triggers (Week 3) |

Delete the `TestApi` scaffold after verification.

---

## Free-Tier Limitations

Understand these limits to avoid surprises during development and demos.

### Azure App Service F1

| Limitation | Detail | Mitigation |
|------------|--------|------------|
| CPU quota | ~60 minutes compute per day | Develop locally; deploy for demos |
| Always On | Not available | First request after idle is slow — warm up before demo |
| Custom domain | Not supported on F1 | Use default `*.azurewebsites.net` URL |
| Scaling | No scale-out | Acceptable for internship |

**Docs:** [App Service quotas](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/azure-subscription-service-limits#app-service-limits)

### Azure Cosmos DB free tier

| Limitation | Detail | Mitigation |
|------------|--------|------------|
| Throughput | 1000 RU/s shared | Sufficient for internship workload |
| Storage | 25 GB | More than enough |
| Accounts | 1 free tier account per subscription | Share account across team with separate containers |
| Cannot enable later | Must check box at creation | Mentor creates account correctly |

**Docs:** [Cosmos DB free tier](https://learn.microsoft.com/en-us/azure/cosmos-db/free-tier)

### GitHub Actions

| Limitation | Detail |
|------------|--------|
| Free minutes | 2000 min/month for private repos (Free plan) |
| Public repos | Unlimited minutes |

**Docs:** [GitHub Actions billing](https://docs.github.com/en/billing/managing-billing-for-github-actions/about-billing-for-github-actions)

---

## Cost Guardrails

1. **Set a budget alert** in Azure Portal → Cost Management → Budgets (e.g., $5 threshold).
2. **Tag all resources** with `project=taskboard` and `owner=<your-name>`.
3. **Use only F1 and Cosmos free tier** unless mentor approves an upgrade.
4. **Delete resources** after the internship if they are no longer needed.
5. **Never share Cosmos keys** in chat, email, or committed files.

**Docs:** [Azure cost management](https://learn.microsoft.com/en-us/azure/cost-management-billing/)

---

## Troubleshooting

### `dotnet` command not found

- Restart terminal after installing .NET SDK.
- Verify installation path is in `PATH`.
- [Troubleshoot .NET installs](https://learn.microsoft.com/en-us/dotnet/core/install/troubleshoot)

### `az login` fails

- Try `az login --use-device-code` if browser does not open.
- Ensure you have an active Azure subscription.

### Cosmos DB Emulator will not start

- Run as Administrator (Windows).
- Check port 8081 is not in use: `netstat -ano | findstr 8081`
- [Emulator troubleshooting](https://learn.microsoft.com/en-us/azure/cosmos-db/troubleshoot-local-emulator)

### EF Core cannot connect to Cosmos

- Verify endpoint URL ends with `:443/` for cloud, or `https://localhost:8081` for emulator.
- Check the primary key matches the account/emulator.
- Ensure database and container names match configuration.

### GitHub Actions deployment fails

- Verify secrets are set correctly in repository settings.
- Check App Service runtime stack is **.NET 8**.
- Review workflow logs in the Actions tab.

### App Service returns 503 after deploy

- Check Application settings (Cosmos connection strings).
- View log stream: Azure Portal → App Service → Log stream.
- [App Service troubleshooting](https://learn.microsoft.com/en-us/azure/app-service/troubleshoot-diagnostic-logs)

---

## Related Documentation

- [Architecture and Tech Stack](../architecture/architecture-and-tech-stack.md)
- [System Overview](../domain/system-overview.md)
- [One-Month Schedule](../internship-plan/one-month-schedule.md)
