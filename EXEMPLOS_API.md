# 🇧🇷 Exemplos de Uso da API - Pagamentos Brasileiros Simplificados

## 📋 Pré-requisitos

- API rodando em `https://localhost:44371`
- Swagger disponível em `https://localhost:44371/swagger`

## 💳 Exemplo 1: Pagamento com PIX (Criação e Processamento Unificados)

```http
POST /api/transacoes
Content-Type: application/json

{
  "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
  "jogoId": "987fcdeb-51a2-43d1-b789-123456789abc",
  "valor": 99.90,
  "tipoPagamento": 3,
  "dadosPIX": {
    "chavePIX": "usuario@email.com",
    "nomeBeneficiario": "João Silva"
  },
  "observacoes": "Compra do jogo FIFA 2024"
}
```

**⚠️ Importante**: Para PIX, você só precisa enviar `dadosPIX`. Os outros campos (`dadosCartao`, `dadosBoleto`) podem ser omitidos ou enviados vazios.

## 💳 Exemplo 2: Pagamento com Cartão de Crédito

```http
POST /api/transacoes
Content-Type: application/json

{
  "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
  "jogoId": "987fcdeb-51a2-43d1-b789-123456789abc",
  "valor": 199.90,
  "tipoPagamento": 1,
  "dadosCartao": {
    "numeroCartao": "4111111111111111",
    "nomeTitular": "João Silva",
    "dataValidade": "12/25",
    "cvv": "123",
    "parcelas": 3
  },
  "observacoes": "Pagamento em 3x sem juros"
}
```

**⚠️ Importante**: Para Cartão, você só precisa enviar `dadosCartao`. Os outros campos (`dadosPIX`, `dadosBoleto`) podem ser omitidos ou enviados vazios.

## 💳 Exemplo 3: Pagamento com Cartão de Débito

```http
POST /api/transacoes
Content-Type: application/json

{
  "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
  "jogoId": "987fcdeb-51a2-43d1-b789-123456789abc",
  "valor": 49.90,
  "tipoPagamento": 2,
  "dadosCartao": {
    "numeroCartao": "5555555555554444",
    "nomeTitular": "Maria Santos",
    "dataValidade": "06/26",
    "cvv": "456"
  },
  "observacoes": "Pagamento à vista"
}
```

**⚠️ Importante**: Para Cartão de Débito, você só precisa enviar `dadosCartao`. Os outros campos (`dadosPIX`, `dadosBoleto`) podem ser omitidos ou enviados vazios.

## 💳 Exemplo 4: Pagamento com Boleto

```http
POST /api/transacoes
Content-Type: application/json

{
  "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
  "jogoId": "987fcdeb-51a2-43d1-b789-123456789abc",
  "valor": 299.90,
  "tipoPagamento": 4,
  "dadosBoleto": {
    "cpfCnpj": "123.456.789-00",
    "nomePagador": "Pedro Oliveira",
    "endereco": "Rua das Flores, 123",
    "cep": "01234-567",
    "cidade": "São Paulo",
    "estado": "SP"
  },
  "observacoes": "Boleto para pagamento em 3 dias"
}
```

**⚠️ Importante**: Para Boleto, você só precisa enviar `dadosBoleto`. Os outros campos (`dadosCartao`, `dadosPIX`) podem ser omitidos ou enviados vazios.

## 🔍 Consultar Transações

### Por ID
```http
GET /api/transacoes/456e7890-e89b-12d3-a456-426614174001
```

### Por Usuário
```http
GET /api/transacoes/usuario/123e4567-e89b-12d3-a456-426614174000
```

### Por Jogo
```http
GET /api/transacoes/jogo/987fcdeb-51a2-43d1-b789-123456789abc
```

### Busca com Filtros
```http
POST /api/transacoes/buscar
Content-Type: application/json

{
  "jogoId": "987fcdeb-51a2-43d1-b789-123456789abc",
  "dataInicio": "2024-09-01T00:00:00Z",
  "dataFim": "2024-09-30T23:59:59Z",
  "pagina": 1,
  "tamanhoPagina": 20
}
```

## ✏️ Atualizar Transação

```http
PUT /api/transacoes/456e7890-e89b-12d3-a456-426614174001
Content-Type: application/json

{
  "status": 3,
  "observacoes": "Pagamento confirmado pelo cliente"
}
```

## 📊 Respostas da API

### Transação Criada (201)
```json
{
  "id": "456e7890-e89b-12d3-a456-426614174001",
  "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
  "jogoId": "987fcdeb-51a2-43d1-b789-123456789abc",
  "valor": 99.90,
  "moeda": "BRL",
  "status": 1,
  "tipoPagamento": 3,
  "referencia": "ABC123DEF456",
  "dataCriacao": "2024-09-07T18:48:39.123Z"
}
```

### Pagamento Processado com Sucesso (200)
```json
{
  "id": "456e7890-e89b-12d3-a456-426614174001",
  "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
  "jogoId": "987fcdeb-51a2-43d1-b789-123456789abc",
  "valor": 99.90,
  "moeda": "BRL",
  "status": 3,
  "tipoPagamento": 3,
  "codigoAutorizacao": "PIX123456",
  "codigoTransacao": "TXN789ABC123",
  "dataProcessamento": "2024-09-07T18:48:40.123Z",
  "dataConfirmacao": "2024-09-07T18:48:40.456Z",
  "observacoes": "PIX processado - Chave: usuario@email.com",
  "referencia": "ABC123DEF456"
}
```

### Pagamento Recusado (200)
```json
{
  "id": "456e7890-e89b-12d3-a456-426614174002",
  "usuarioId": "123e4567-e89b-12d3-a456-426614174000",
  "jogoId": "987fcdeb-51a2-43d1-b789-123456789abc",
  "valor": 199.90,
  "moeda": "BRL",
  "status": 4,
  "tipoPagamento": 1,
  "dataProcessamento": "2024-09-07T18:48:40.123Z",
  "erroProcessamento": "Cartão de crédito recusado - Verifique os dados ou limite",
  "referencia": "ABC123DEF456"
}
```

## 🚨 Códigos de Status

| Código | Status | Descrição |
|--------|--------|-----------|
| 1 | Pendente | Transação criada, aguardando processamento |
| 2 | Processando | Pagamento sendo processado |
| 3 | Aprovada | Pagamento aprovado com sucesso |
| 4 | Recusada | Pagamento recusado |
| 5 | Cancelada | Transação cancelada |
| 6 | Falha | Erro no processamento |

## 🚨 Códigos de Tipo de Pagamento

| Código | Tipo | Descrição |
|--------|------|-----------|
| 1 | CartaoCredito | Cartão de Crédito |
| 2 | CartaoDebito | Cartão de Débito |
| 3 | PIX | PIX |
| 4 | Boleto | Boleto Bancário |

---

**FCG Pagamentos** - Microserviço de Pagamentos | FIAP Cloud Games 2024
