# 🏦 FCG Pagamentos - Microserviço de Pagamentos Brasileiro

Microserviço responsável pelo processamento e status de transações de pagamento do sistema FCG (FIAP Cloud Games), otimizado para o mercado brasileiro com suporte a PIX, cartões de crédito/débito e boleto bancário.

## 🚀 Como Iniciar o Projeto FCG Pagamentos

### ✅ Pré-requisitos
- **.NET 8 SDK** instalado
- **PostgreSQL** (via Supabase)
- **EF Core CLI** instalado globalmente:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 📦 Restauração de Pacotes
Após clonar o repositório, navegue até a pasta do projeto e execute:

```bash
cd FGC_PAGAMENTOS
dotnet restore
```

### 🛠️ Configuração do Banco de Dados

#### 🔄 **Supabase + PostgreSQL**
O projeto utiliza **Supabase** como provedor de PostgreSQL em nuvem:

**Connection String:**
```
Host=db.elcvczlnnzbgcpsbowkg.supabase.co
Port=5432
Database=postgres
Username=postgres
Password=Fiap@1234
```

#### 🗄️ **Aplicando Migrations**
Para aplicar as migrations no Supabase:

```bash
# Navegar para a API
cd FCG.Pagamentos.API

# Aplicar migrations
dotnet ef database update --project ../FCG.Pagamentos.Infrastructure --startup-project .
```

#### 📝 **Criando Novas Migrations**
Para gerar uma nova migration:

```bash
cd FCG.Pagamentos.API
dotnet ef migrations add NomeDaMigration --project ../FCG.Pagamentos.Infrastructure --startup-project .
```

### 🔄 Executando a Aplicação

Com o banco configurado, execute a API:

```bash
cd FCG.Pagamentos.API
dotnet run
```

A API será iniciada em:
- **HTTPS**: https://localhost:61824
- **HTTP**: http://localhost:61825
- **Swagger**: http://localhost:61825/swagger

## 🏗️ Arquitetura

### 📂 Estrutura do Projeto
```
FGC_PAGAMENTOS/
├── FCG.Pagamentos.API/           # Camada de API e Controllers
│   ├── Controllers/              # Controllers REST
│   │   └── TransacoesController.cs
│   ├── Program.cs               # Configuração da aplicação
│   └── appsettings.json        # Configurações
├── FCG.Pagamentos.Application/   # Regras de negócio e serviços
│   ├── Pagamentos/
│   │   ├── Interfaces/          # Interfaces dos serviços
│   │   ├── Services/            # Implementação dos serviços
│   │   └── ViewModels/          # DTOs e ViewModels
│   └── ServiceCollectionExtensions.cs
├── FCG.Pagamentos.Domain/        # Entidades e interfaces
│   └── Pagamentos/
│       ├── Entities/            # Entidades do domínio
│       └── Interfaces/          # Interfaces dos repositórios
└── FCG.Pagamentos.Infrastructure/ # EF Core + Repositórios
    ├── Pagamentos/
    │   ├── Repositories/        # Implementação dos repositórios
    │   └── Context/             # DbContext Factory
    ├── Migrations/              # Scripts de migração EF Core
    ├── PagamentosDbContext.cs   # Contexto do EF Core
    └── ServiceCollectionExtensions.cs
```

### 🔧 Tecnologias Utilizadas
- **.NET 8** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados (via Supabase)
- **Serilog** - Logging estruturado
- **Swagger/OpenAPI** - Documentação da API

## 📡 Endpoints da API

### 💳 **Transações de Pagamento Simplificadas**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/transacoes` | Criar e processar transação (unificado) |
| `GET` | `/api/transacoes/{id}` | Obter transação por ID |
| `GET` | `/api/transacoes/usuario/{usuarioId}` | Obter transações por usuário |
| `GET` | `/api/transacoes/jogo/{jogoId}` | Obter transações por jogo |
| `POST` | `/api/transacoes/buscar` | Buscar com filtros (data, jogo) |
| `PUT` | `/api/transacoes/{id}` | Atualizar status e observações |

### 🇧🇷 **Tipos de Pagamento Suportados**

#### **PIX (Pagamento Instantâneo)**
- **Taxa de Sucesso**: 90%
- **Dados Necessários**: Chave PIX, Nome do beneficiário
- **Processamento**: Instantâneo
- **Código de Autorização**: Formato `PIX{6 dígitos}`

#### **Cartão de Crédito**
- **Taxa de Sucesso**: 80%
- **Dados Necessários**: Número, Nome do titular, Validade, CVV, Parcelas
- **Processamento**: 1-3 segundos
- **Código de Autorização**: Formato `CC{6 dígitos}`

#### **Cartão de Débito**
- **Taxa de Sucesso**: 80%
- **Dados Necessários**: Número, Nome do titular, Validade, CVV
- **Processamento**: 1-3 segundos
- **Código de Autorização**: Formato `CD{6 dígitos}`

#### **Boleto Bancário**
- **Taxa de Sucesso**: 70%
- **Dados Necessários**: CPF/CNPJ, Nome, Endereço completo, CEP, Cidade, Estado
- **Processamento**: Imediato (geração do boleto)
- **Código de Autorização**: Formato `BOL{6 dígitos}`

## 🗄️ Modelo de Dados

### 📊 **Tabelas Principais**

#### **Transacoes**
- `Id` (UUID) - Identificador único
- `UsuarioId` (UUID) - ID do usuário
- `JogoId` (UUID) - ID do jogo
- `Valor` (Decimal) - Valor da transação
- `Status` (Enum) - Status da transação (Pendente, Processando, Aprovada, Recusada, Cancelada, Falha)
- `TipoPagamento` (Enum) - Tipo de pagamento (CartaoCredito, CartaoDebito, PIX, Boleto)
- `Referencia` (String) - Referência única para consulta
- `CodigoAutorizacao` (String) - Código de autorização do processador
- `CodigoTransacao` (String) - Código da transação
- `DataProcessamento` (DateTime) - Data do processamento
- `DataConfirmacao` (DateTime) - Data da confirmação
- `Observacoes` (String) - Observações da transação
- `ErroProcessamento` (String) - Erro em caso de falha
- `DataCriacao` (DateTime) - Data de criação

## 🔄 Migração de SQL Server para PostgreSQL

### ✅ **Mudanças Implementadas**
- **Banco de Dados**: Migrado de SQL Server LocalDB para PostgreSQL (Supabase)
- **Arquitetura**: Migrado de Minimal API para Controllers
- **Dependências**: Atualizadas para suportar PostgreSQL
- **Migrations**: Recriadas para PostgreSQL

### 🔧 **Configurações Atualizadas**
- **Connection String**: Configurada para Supabase
- **DbContext**: Otimizado para PostgreSQL
- **Controllers**: Implementados seguindo padrão REST
- **Dependency Injection**: Configurado com ServiceCollectionExtensions

## 🐞 Logs e Monitoramento

### 📝 **Serilog**
- Logs estruturados com Serilog
- Arquivos de log por data em `/logs/`
- Logs de console para desenvolvimento
- Formato: `pagamentos-api-YYYY-MM-DD.txt`

### 🔍 **Swagger**
- Documentação automática da API
- Interface interativa para testes
- Disponível em `/swagger` quando em desenvolvimento

## 🚀 Deploy e Produção

### ☁️ **Supabase**
- Banco de dados PostgreSQL gerenciado
- Migrations aplicadas automaticamente na inicialização
- Conexão segura via SSL

### 🔧 **Configurações de Ambiente**
- **Development**: Usa appsettings.Development.json
- **Production**: Usa appsettings.json
- **Connection String**: Configurada via Supabase

## 📋 Status do Projeto

### ✅ **Concluído**
- [x] Migração para PostgreSQL (Supabase)
- [x] Implementação de Controllers simplificados
- [x] Configuração de Dependency Injection
- [x] Migrations aplicadas no Supabase
- [x] API funcionando com Swagger
- [x] Logs estruturados com Serilog
- [x] Suporte a pagamentos brasileiros (PIX, Cartão, Boleto)
- [x] Simplificação máxima do microserviço
- [x] Criação e processamento unificados em um único POST
- [x] Remoção de campos desnecessários (moeda, tentativas, etc.)

### 🔄 **Funcionalidades Simplificadas**
- [x] CRUD de Transações simplificado
- [x] Processamento de Pagamentos Brasileiros unificado
- [x] Validações específicas por tipo de pagamento
- [x] Códigos de autorização por tipo de pagamento
- [x] Tratamento de Erros específicos
- [x] Simulação de processamento com taxas de sucesso realistas
- [x] Busca por usuário, jogo e período
- [x] Atualização simples de status e observações

**FCG Pagamentos** - Microserviço de Pagamentos | FIAP Cloud Games 2025
