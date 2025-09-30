# 🔧 FCG Pagamentos - Azure Functions

Este projeto contém duas Azure Functions que complementam o microserviço de pagamentos FCG:

## 🚀 Azure Functions Implementadas

### 1. **PaymentProcessorFunction** (HTTP Trigger)
- **Rota**: `POST /api/payments/process`
- **Tipo**: HTTP Trigger
- **Função**: Processa pagamentos utilizando a mesma lógica da API REST

#### Exemplo de Requisição:
```json
{
  "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
  "jogoId": "987fcdeb-51a2-43d1-9f12-123456789abc",
  "valor": 29.99,
  "tipoPagamento": 0,
  "observacoes": "Compra do jogo XYZ"
}
```

#### Exemplo de Resposta (200 OK):
```json
{
  "status": "Aprovada",
  "message": "Pagamento processado com sucesso",
  "transacaoId": "456e7890-e89b-12d3-a456-426614174111",
  "codigoAutorizacao": "PIX123456",
  "processedAt": "2025-09-30T12:00:00Z"
}
```

### 2. **NotificationFunction** (Queue Trigger)
- **Fila**: `notification-queue`
- **Tipo**: Queue Trigger
- **Função**: Processa notificações de forma assíncrona

#### Exemplo de Mensagem para a Fila:
```json
{
  "transacaoId": "456e7890-e89b-12d3-a456-426614174111",
  "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
  "tipoNotificacao": "pagamento_aprovado",
  "conteudo": "Seu pagamento foi aprovado com sucesso!",
  "dadosAdicionais": {
    "valor": 29.99,
    "jogo": "Game XYZ"
  }
}
```

## ⚙️ Configuração Local

### 1. **Pré-requisitos**
```bash
# Instalar Azure Functions Core Tools
npm install -g azure-functions-core-tools@4 --unsafe-perm true

# OU via Chocolatey (Windows)
choco install azure-functions-core-tools-4

# OU baixar direto do site da Microsoft
```

### 2. **Configurar local.settings.json**
Edite o arquivo `local.settings.json` e configure as connection strings:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DefaultConnection": "Host=db.elcvczlnnzbgcpsbowkg.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Fiap@1234",
    "NotificationQueueConnection": "UseDevelopmentStorage=true"
  }
}
```

### 3. **Executar Localmente**
```bash
# Navegar para a pasta das Functions
cd FCG.Pagamentos.Functions

# Instalar Azurite (emulador do Azure Storage) - apenas uma vez
npm install -g azurite

# Executar Azurite em outro terminal
azurite --silent --location c:\azurite --debug c:\azurite\debug.log

# Executar as Functions
func start
```

### 4. **Testar as Functions Localmente**

#### PaymentProcessorFunction:
```bash
curl -X POST http://localhost:7071/api/payments/process \
  -H "Content-Type: application/json" \
  -d '{
    "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
    "jogoId": "987fcdeb-51a2-43d1-9f12-123456789abc",
    "valor": 29.99,
    "tipoPagamento": 0,
    "observacoes": "Teste local"
  }'
```

#### NotificationFunction:
```bash
# Enviar mensagem para a fila usando Azure Storage Explorer
# Ou via código C#:

using Azure.Storage.Queues;

var client = new QueueClient("UseDevelopmentStorage=true", "notification-queue");
await client.CreateIfNotExistsAsync();

var message = """
{
  "transacaoId": "456e7890-e89b-12d3-a456-426614174111",
  "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
  "tipoNotificacao": "pagamento_aprovado",
  "conteudo": "Teste de notificação local"
}
""";

await client.SendMessageAsync(message);
```

## ☁️ Deploy no Azure

### 1. **Criar Resources no Azure**
```bash
# Criar Resource Group
az group create --name rg-fcg-pagamentos --location eastus2

# Criar Storage Account
az storage account create \
  --name stfcgpagamentos \
  --resource-group rg-fcg-pagamentos \
  --location eastus2 \
  --sku Standard_LRS

# Criar Function App
az functionapp create \
  --name func-fcg-pagamentos \
  --resource-group rg-fcg-pagamentos \
  --storage-account stfcgpagamentos \
  --runtime dotnet-isolated \
  --runtime-version 8 \
  --functions-version 4
```

### 2. **Configurar Application Settings**
```bash
# Configurar Connection String do PostgreSQL
az functionapp config appsettings set \
  --name func-fcg-pagamentos \
  --resource-group rg-fcg-pagamentos \
  --settings DefaultConnection="Host=db.elcvczlnnzbgcpsbowkg.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Fiap@1234"

# Configurar Connection String para Queue
az functionapp config appsettings set \
  --name func-fcg-pagamentos \
  --resource-group rg-fcg-pagamentos \
  --settings NotificationQueueConnection="<CONNECTION_STRING_DO_STORAGE_ACCOUNT>"
```

### 3. **Fazer Deploy**
```bash
# Via Azure Functions Core Tools
func azure functionapp publish func-fcg-pagamentos

# OU via Visual Studio (Publish)
# OU via Azure DevOps/GitHub Actions
```

### 4. **Criar a Fila no Storage Account**
```bash
# Via Azure CLI
az storage queue create \
  --name notification-queue \
  --account-name stfcgpagamentos

# OU via Azure Storage Explorer
# OU programaticamente via código
```

## 🔗 Integração com o Sistema Existente

### Enviar Mensagens para Fila de Notificação
Adicione este código ao seu serviço de transações:

```csharp
using Azure.Storage.Queues;
using System.Text.Json;

public class NotificationService
{
    private readonly QueueClient _queueClient;
    
    public NotificationService(IConfiguration config)
    {
        _queueClient = new QueueClient(
            config.GetConnectionString("NotificationQueueConnection"), 
            "notification-queue"
        );
    }
    
    public async Task EnviarNotificacaoAsync(Guid transacaoId, Guid usuarioId, string tipo, string conteudo)
    {
        var notification = new
        {
            TransacaoId = transacaoId,
            UsuarioId = usuarioId,
            TipoNotificacao = tipo,
            Conteudo = conteudo,
            CriadoEm = DateTime.UtcNow
        };
        
        var json = JsonSerializer.Serialize(notification);
        await _queueClient.SendMessageAsync(json);
    }
}
```

## 📊 Monitoramento

### Logs e Application Insights
- Os logs são automaticamente enviados para o Application Insights
- Use o portal do Azure para monitorar execuções
- Configure alertas para falhas ou high latency

### Métricas Importantes
- **PaymentProcessorFunction**: Tempo de resposta, taxa de erro, throughput
- **NotificationFunction**: Tempo de processamento da fila, mensagens processadas/falhadas

## 🐛 Troubleshooting

### Problemas Comuns

1. **Connection String inválida**
   - Verificar se as settings estão configuradas corretamente
   - Testar conexão com o PostgreSQL

2. **Fila não encontrada**
   - Criar a fila `notification-queue` no Storage Account
   - Verificar nome da fila e connection string

3. **Dependências não resolvidas**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Azurite não iniciando**
   ```bash
   # Limpar dados do Azurite
   azurite --silent --location c:\azurite --debug c:\azurite\debug.log --loose
   ```

## 📝 Tipos de Notificação Suportados

- `pagamento_aprovado`: Notificação de pagamento bem-sucedido
- `pagamento_recusado`: Notificação de pagamento recusado  
- `pagamento_pendente`: Notificação de pagamento em análise
- `sistema_manutencao`: Notificação de manutenção programada

---

**FCG Pagamentos Functions** - Processamento Serverless | FIAP Cloud Games 2025
