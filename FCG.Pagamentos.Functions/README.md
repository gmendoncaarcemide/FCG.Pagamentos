# ⚡ FCG Pagamentos - Azure Functions

Este repositório contém a configuração e o código base das **Azure Functions** responsáveis pelo processamento e envio de notificações do microserviço de **pagamentos** do sistema **FIAP Cloud Games (FCG)**.

---

## 🧩 Estrutura do Projeto

| Arquivo | Descrição |
|----------|------------|
| `Program.cs` | Define a inicialização do host das Functions no modelo **.NET Isolated Worker**, configurando os serviços e o logger da aplicação. |
| `host.json` | Configurações globais do host de execução das Azure Functions (versão, logging e extensão de filas). |
| `local.settings.json` | Configurações e variáveis de ambiente utilizadas durante o desenvolvimento local (connection strings e runtime). |
| `README_FUNCTIONS.md` | Documentação detalhada sobre o funcionamento das Functions, rotas, exemplos de payloads e orientações de deploy. |

---

## 🏗️ Configuração Técnica

### 1. **Program.cs**

O ponto de entrada da aplicação. Define a configuração básica do host das Azure Functions usando o modelo **isolado (.NET 8)**.

```csharp
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
    })
    .Build();

host.Run();
```

📘 *Função:* Inicializa o worker, adiciona logging via console e executa as Functions hospedadas.

---

### 2. **host.json**

Define parâmetros de execução e monitoramento das Functions.

```json
{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      },
      "enableLiveMetricsFilters": true
    }
  },
  "extensions": {
    "queues": {
      "batchSize": 16,
      "maxDequeueCount": 5,
      "newBatchThreshold": 8
    }
  }
}
```

🧠 **Principais pontos:**
- **Application Insights**: coleta de logs e métricas em tempo real.  
- **Queue Extension**: define como as mensagens são processadas em lotes (`batchSize`, `maxDequeueCount`).

---

### 3. **local.settings.json**

Contém variáveis de ambiente utilizadas apenas no ambiente local.

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DefaultConnection": "Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.elcvczlnnzbgcpsbowkg;Password=Fiap@1234;Ssl Mode=Require;Trust Server Certificate=true;",
    "AzureWebJobsNotificationQueueConnection": "DefaultEndpointsProtocol=https;AccountName=stfcgfunctions;AccountKey=...;EndpointSuffix=core.windows.net"
  }
}
```

⚙️ **Uso:**
- `AzureWebJobsStorage`: usado pelo runtime para logs e filas.
- `FUNCTIONS_WORKER_RUNTIME`: define o worker isolado (`dotnet-isolated`).
- `DefaultConnection`: conexão com o banco (PostgreSQL/Supabase).
- `AzureWebJobsNotificationQueueConnection`: conexão com o Storage Account que hospeda a fila `notification-queue`.

> ⚠️ **Atenção:** Nunca publique connection strings sensíveis em repositórios públicos.

---

### 4. **README_FUNCTIONS.md**

Documentação funcional das duas Azure Functions implementadas:

#### 🧾 **PaymentProcessorFunction** (HTTP Trigger)
- **Endpoint:** `POST /api/payments/process`
- **Responsável por:** processar pagamentos (simulação da API REST principal).

#### 📬 **NotificationFunction** (Queue Trigger)
- **Trigger:** `notification-queue`
- **Responsável por:** processar mensagens de notificação enviadas de forma assíncrona.

📖 O arquivo inclui:
- Exemplos de requisições e respostas.
- Passos para rodar localmente com **Azurite**.
- Comandos para **deploy via Azure CLI**.
- Guia de **integração com a aplicação principal**.
- Dicas de **troubleshooting** e **monitoramento com Application Insights**.

---

## 🚀 Executando Localmente

1. **Instalar as ferramentas**
   ```bash
   npm install -g azure-functions-core-tools@4
   npm install -g azurite
   ```

2. **Iniciar o Azurite**
   ```bash
   azurite --silent --location ./azurite --debug ./azurite/debug.log
   ```

3. **Executar as Functions**
   ```bash
   func start
   ```

4. **Testar a rota local**
   ```bash
   curl -X POST http://localhost:7071/api/payments/process      -H "Content-Type: application/json"      -d '{"usuarioId":"123","jogoId":"456","valor":29.99,"tipoPagamento":0}'
   ```

---

## ☁️ Deploy no Azure

O processo completo de criação de recursos e publicação está descrito no `README_FUNCTIONS.md`:
- Criar `Resource Group`, `Storage Account` e `Function App`.
- Configurar variáveis de ambiente via CLI.
- Publicar com `func azure functionapp publish`.

---

## 📊 Monitoramento

- **Application Insights:** métricas e logs.
- **Azure Storage Queues:** monitorar mensagens e reprocessamentos.
- **Azure Portal:** acompanhamento das execuções e alertas.

---

## 📄 Licença
Projeto desenvolvido para **FIAP Cloud Games (FCG)** – 2025.  
Uso educacional e acadêmico. Direitos reservados aos autores originais.
