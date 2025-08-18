# 🏦 FCG Pagamentos - Microserviço de Pagamentos

Microserviço responsável pelo processamento e status de transações de pagamento do sistema FCG (FIAP Cloud Games).

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
│   │   ├── TransacoesController.cs
│   │   └── ReembolsosController.cs
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

### 💳 **Transações**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/transacoes` | Criar nova transação |
| `GET` | `/api/transacoes` | Listar todas as transações |
| `GET` | `/api/transacoes/{id}` | Obter transação por ID |
| `GET` | `/api/transacoes/usuario/{usuarioId}` | Obter transações por usuário |
| `PUT` | `/api/transacoes/{id}` | Atualizar transação |
| `DELETE` | `/api/transacoes/{id}` | Excluir transação |
| `POST` | `/api/transacoes/processar` | Processar pagamento |
| `POST` | `/api/transacoes/{id}/cancelar` | Cancelar transação |
| `POST` | `/api/transacoes/buscar` | Buscar com filtros |
| `GET` | `/api/transacoes/referencia/{referencia}` | Obter por referência |

### 💰 **Reembolsos**
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/reembolsos` | Criar novo reembolso |
| `GET` | `/api/reembolsos` | Listar todos os reembolsos |
| `GET` | `/api/reembolsos/{id}` | Obter reembolso por ID |
| `GET` | `/api/reembolsos/usuario/{usuarioId}` | Obter reembolsos por usuário |
| `GET` | `/api/reembolsos/transacao/{transacaoId}` | Obter reembolsos por transação |
| `PUT` | `/api/reembolsos/{id}/status` | Atualizar status do reembolso |
| `POST` | `/api/reembolsos/{id}/cancelar` | Cancelar reembolso |
| `POST` | `/api/reembolsos/{id}/processar` | Processar reembolso |

## 🗄️ Modelo de Dados

### 📊 **Tabelas Principais**

#### **Transacoes**
- `Id` (UUID) - Identificador único
- `UsuarioId` (UUID) - ID do usuário
- `JogoId` (UUID) - ID do jogo
- `Valor` (Decimal) - Valor da transação
- `Moeda` (String) - Moeda (ex: BRL)
- `Status` (Enum) - Status da transação
- `TipoPagamento` (Enum) - Tipo de pagamento
- `Referencia` (String) - Referência única
- `DataCriacao` (DateTime) - Data de criação
- `DataAtualizacao` (DateTime) - Data de atualização

#### **Reembolsos**
- `Id` (UUID) - Identificador único
- `TransacaoId` (UUID) - ID da transação relacionada
- `UsuarioId` (UUID) - ID do usuário
- `ValorReembolso` (Decimal) - Valor do reembolso
- `Motivo` (String) - Motivo do reembolso
- `Status` (Enum) - Status do reembolso
- `DataSolicitacao` (DateTime) - Data da solicitação

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
- [x] Implementação de Controllers
- [x] Configuração de Dependency Injection
- [x] Migrations aplicadas no Supabase
- [x] API funcionando com Swagger
- [x] Logs estruturados com Serilog

### 🔄 **Funcionalidades**
- [x] CRUD de Transações
- [x] CRUD de Reembolsos
- [x] Processamento de Pagamentos
- [x] Busca e Filtros
- [x] Validações de Entrada
- [x] Tratamento de Erros

## 🤝 Contribuição

1. Clone o repositório
2. Configure o ambiente conforme instruções acima
3. Execute as migrations
4. Teste a aplicação
5. Faça suas alterações
6. Execute os testes
7. Submeta um Pull Request

## 📞 Suporte

Para dúvidas ou problemas:
- Verifique os logs em `/logs/`
- Consulte a documentação do Swagger
- Verifique a conexão com o Supabase
- Execute `dotnet build` para verificar erros de compilação

---

**FCG Pagamentos** - Microserviço de Pagamentos | FIAP Cloud Games 2024
