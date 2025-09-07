# 🏦 FCG Pagamentos

Microserviço responsável pelo processamento e gerenciamento de transações de pagamento do sistema **FIAP Cloud Games**.  
Suporta **PIX, cartão de crédito/débito e boleto bancário**.

---

## 🚀 Como rodar o projeto

### ✅ Pré-requisitos
- **.NET 8 SDK**
- **PostgreSQL** (via Supabase)
- **EF Core CLI**:
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

### 🇧🇷 **Tipos de Pagamento**

#### **PIX (Pagamento Instantâneo)**
- **Dados Necessários**: Chave PIX, Nome do beneficiário
- 
#### **Cartão de Crédito**
- **Dados Necessários**: Número, Nome do titular, Validade, CVV, Parcelas

#### **Cartão de Débito**
- **Dados Necessários**: Número, Nome do titular, Validade, CVV
  
#### **Boleto Bancário**
- **Dados Necessários**: CPF/CNPJ, Nome, Endereço completo, CEP, Cidade, Estado

## 🗄️ Modelo de Dados

### 📊 **Tabela**

#### **Transações**
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
  

Microserviço de Pagamentos | FIAP Cloud Games 2025
