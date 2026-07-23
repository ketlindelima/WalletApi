# Wallet API

API REST para gerenciamento de uma carteira digital desenvolvida em **.NET 8** com **Entity Framework Core** e **PostgreSQL**.

O projeto permite criar contas, consultar saldo, realizar créditos, débitos, transferências entre contas e consultar o histórico de transações, atendendo aos requisitos propostos no desafio técnico.

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL 16
- Docker & Docker Compose
- Nginx (Load Balancer)
- Swagger

---

## Como executar

Suba toda a infraestrutura:

```bash
docker compose up --build --scale api=2 -d
```

Serão iniciados:

- PostgreSQL
- 2 instâncias da API
- Nginx como _load balance_

A API estará disponível em:

```
http://localhost:8080
```

Swagger:

```
http://localhost:8080/swagger
```

As migrations são executadas automaticamente na inicialização da aplicação através do EF Core (`Database.Migrate()`), dispensando execução manual de comandos.

---

## Endpoints

| Método | Endpoint | Descrição |
|---------|----------|-----------|
| POST | /accounts | Cria uma conta |
| GET | /accounts/{id}/balance | Consulta saldo |
| POST | /accounts/{id}/transactions | Crédito ou débito |
| GET | /accounts/{id}/transactions | Extrato paginado |
| POST | /transfers | Transferência entre contas |

---

## Arquitetura

O projeto foi organizado em camadas simples (arquivos principais):

```text
WalletApi
|
├───Controllers
│       AccountsController.cs
│       TransfersController.cs
│       
├───Data
│       AppDbContext.cs
│       
├───DTOs
│   ├───Requests
│   │       CreateTransactionRequest.cs
│   │       CreateTransferRequest.cs
│   │       TransactionQueryRequest.cs
│   │       
│   └───Responses
│           PagedResponse.cs
│           TransactionResponse.cs
│           
├───Migrations
│       20260722175825_InitialCreate.cs
│       20260722175825_InitialCreate.Designer.cs
│       20260722203649_AddTransactions.cs
│       20260722203649_AddTransactions.Designer.cs
│       20260723144944_IdempotencyAndTransfer.cs
│       20260723144944_IdempotencyAndTransfer.Designer.cs
│       20260723165123_AddAccountConcurrency.cs
│       20260723165123_AddAccountConcurrency.Designer.cs
│       AppDbContextModelSnapshot.cs
│       
├───Models
│       Account.cs
│       IdempotencyRecord.cs
│       Transaction.cs
│       TransactionType.cs
│       Transfer.cs
│       
├───nginx
│       nginx.
│       nginx.conf
|
├───WalletApi.Tests
|        AccountTests.cs
|        WalletApi.Tests.csproj
|
│   docker-compose.yml
│   Dockerfile
│   Program.cs
│   WalletApi.csproj

```

A lógica de negócio principal permanece nas entidades de domínio (`Models`), enquanto os controllers atuam apenas orquestradores das operações.

---

## Banco de Dados

Foi utilizado **PostgreSQL** por oferecer:

- transações ACID;
- consistência para operações financeiras;
- suporte a índices e constraints;
- excelente integração com Entity Framework Core.

Por se tratar de movimentação financeira, um banco relacional garante melhor integridade dos dados em comparação com soluções NoSQL para este cenário.

---

## Decisões de Design

### Histórico de transações

Todas as movimentações são registradas na tabela `Transactions`.

O tipo da operação é identificado através do enum:

- Credit
- Debit
- TransferIn
- TransferOut

Essa abordagem centraliza todo o histórico financeiro da conta em uma única estrutura.

---

### Transferências

Foi criada uma entidade própria `Transfer` responsável por relacionar as duas transações geradas em uma transferência:

- saída da conta de origem;
- entrada na conta de destino.

Dessa forma é possível manter rastreabilidade da operação completa.

---

### Consistência

As transferências são executadas utilizando transações do banco de dados.

Caso qualquer etapa falhe, toda a operação é revertida, impedindo situações onde o dinheiro saia de uma conta sem entrar na outra.

---

### Idempotência

Foi implementado suporte ao header:

```
Idempotency-Key
```

As chaves processadas são armazenadas na tabela `IdempotencyRecords`.

Um índice único garante que a mesma operação não seja processada duas vezes, mesmo em cenários com múltiplas instâncias da aplicação.

---

### Concorrência

Como o desafio exige execução em múltiplas instâncias atrás de um _load balance_, foi implementado controle de concorrência otimista utilizando **RowVersion** do Entity Framework Core.

Essa estratégia evita inconsistências quando duas requisições tentam modificar simultaneamente o saldo da mesma conta.

---

### Paginação

O endpoint de extrato suporta paginação utilizando:

- `page`
- `pageSize`

Foi definido limite máximo de registros por página para evitar consultas excessivamente grandes.

---

## Escalabilidade

A aplicação foi preparada para execução horizontal.

O ambiente Docker executa:

- duas instâncias da API;
- um banco PostgreSQL compartilhado;
- Nginx realizando o balanceamento de carga.

---

## Testes

Foram implementados testes automatizados utilizando básicos **xUnit** e **FluentAssertions**, com foco nas regras de negócio do domínio.

Os testes validam principalmente:

- criação de conta;
- saldo inicial;
- operações de crédito;
- operações de débito;
- bloqueio de débito sem saldo suficiente;
- transferências entre contas;
- validação de valores inválidos.

O objetivo é garantir que as principais regras financeiras permaneçam protegidas contra alterações futuras.

Como esse projeto não possui uma solution, para executar os testes:

```bash
cd WalletApi.Tests
```
```bash
dotnet test
```

O foco dos testes é garantir o comportamento correto das regras críticas do domínio.
Após o teste o resultado obtido foi:

```bash
Aprovado!  – Com falha:     0, Aprovado:     7, Ignorado:     0, Total:     7, Duração: 23 ms - WalletApi.Tests.dll (net8.0)
```
---
## Status Codes

A API utiliza os seguintes códigos HTTP para representar o resultado das operações:

| Status Code | Descrição |
|--------------|-----------|
| **200 OK** | Operação realizada com sucesso (consulta de saldo, extrato, crédito, débito, transferência ou reenvio de uma requisição idempotente já processada). |
| **201 Created** | Conta criada com sucesso. |
| **400 Bad Request** | Requisição inválida, como tipo de transação não suportado, valor inválido ou tentativa de transferir para a mesma conta. |
| **404 Not Found** | Conta não encontrada. |
| **409 Conflict** | Conflito de concorrência detectado durante a atualização do saldo. O cliente pode tentar novamente a operação. |
| **500 Internal Server Error** | Erro interno inesperado da aplicação. |

### Exemplos de erros tratados

- Débito com saldo insuficiente.
- Transferência entre a mesma conta.
- Conta inexistente.
- Tipo de transação inválido.
- Valor de transação menor ou igual a zero.
- Requisição duplicada utilizando a mesma `Idempotency-Key` (retorna o resultado da operação já processada).
- Conflitos de concorrência quando duas operações tentam alterar simultaneamente o saldo da mesma conta.

---
## Decisões Técnicas

Dentre algumas decisões tomadas:

- Utilizar paginação para consulta de extrato, permitindo configurações;
- Criar uma única tabela de transações e uma tabela de transferência para relacionar as duas contas;
- Solicitar uma _Idempotency_Key_ para garantir a unicidade da transação;
- Utilizar dotnet ef _Row Version_ para problema de concorrência;
- Utilização de DTOs para modelos de Requests e Responses.

---

## Melhorias Futuras

Algumas melhorias que poderiam ser adicionadas com mais tempo:

- autenticação e autorização;
- observabilidade (logs estruturados);
- cache para consultas;
- documentação mais detalhada;
- pipeline de CI/CD;
- Mais testes de integração cobrindo cenários completos da API, incluindo persistência, idempotência e execução distribuída.

---

## Principais Desafios

Durante o desenvolvimento, os principais desafios foram:

- garantir consistência nas transferências;
- implementar idempotência para evitar processamento duplicado após timeouts de rede;
- preparar a aplicação para execução em múltiplas instâncias;
- tratar concorrência de saldo utilizando controle de versão das entidades.
