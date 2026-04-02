# JiuJitsu CRUD

POC de cadastro de atletas de jiu-jitsu com foco em práticas de Clean Architecture, DDD, CQRS e mensageria assíncrona.

---

## Arquitetura

```
[React] → POST /atletas → [API .NET 9]
                               ↓
                        Publica mensagem
                               ↓
                          [RabbitMQ]
                               ↓
                    [Worker Service .NET 9]
                         ↓          ↓
                  Salva no BD   Envia email
                 [PostgreSQL]   [MailHog]
```

### Projetos

| Projeto | Função |
|---|---|
| `JiuJitsu.Domain` | Entidades, Value Objects, enums, interfaces de repositório |
| `JiuJitsu.Application` | Commands, Queries, Handlers (MediatR), DTOs, interfaces |
| `JiuJitsu.Infrastructure` | EF Core, Dapper, RabbitMQ publisher, MailKit |
| `JiuJitsu.Contracts` | DTOs de mensagem compartilhados entre API e Worker |
| `JiuJitsu.Api` | API REST .NET 9 — recebe requisições e publica no RabbitMQ |
| `JiuJitsu.Worker` | Worker Service — consome fila, salva no banco e envia email |
| `JiuJitsu.AppHost` | .NET Aspire 9 — orquestra todos os serviços |
| `JiuJitsu.ServiceDefaults` | Configurações compartilhadas do Aspire (telemetria, health checks) |
| `JiuJitsu.Tests` | Testes unitários com xUnit, NSubstitute e FluentAssertions |
| `jiujitsu-front` | Frontend React com Vite |

### Padrões técnicos

- **Clean Architecture** — separação por camadas (Domain → Application → Infrastructure → API/Worker)
- **DDD** — entidades com comportamento, Value Objects (CPF, Email), soft delete
- **CQRS com MediatR** — Commands via EF Core, Queries via Dapper
- **Mensageria assíncrona** — API publica no RabbitMQ, Worker processa em background
- **Soft delete** — atletas excluídos são marcados como inativos (`ativo = false`)

---

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para subir PostgreSQL, RabbitMQ e MailHog via Aspire)
- [Node.js 14.18+](https://nodejs.org/) (para o frontend)

---

## Como executar

### 1. Backend (API + Worker + Infraestrutura)

```bash
# Na raiz do projeto
export PATH="$HOME/.dotnet:$PATH"

cd JiuJitsuCrud
dotnet run --project JiuJitsu.AppHost
```

O Aspire sobe automaticamente:
- **PostgreSQL** — banco de dados
- **RabbitMQ** — mensageria (painel em http://localhost:15672, usuário: `guest`, senha: `guest`)
- **MailHog** — servidor de email local (UI em http://localhost:8025)
- **API** — porta exibida no dashboard do Aspire
- **Worker** — processa mensagens em background

O dashboard do Aspire ficará disponível em **https://localhost:17058** (porta pode variar).

> Na primeira execução as migrations são aplicadas automaticamente pelo Worker.

### 2. Frontend

```bash
cd jiujitsu-front

# Ajuste a porta da API conforme exibida no dashboard do Aspire
# Edite o arquivo .env.development:
# VITE_API_URL=http://localhost:<PORTA_DA_API>

npm run dev
# Acesse http://localhost:3000
```

---

## Endpoints da API

| Método | Rota | Descrição | Resposta |
|---|---|---|---|
| `GET` | `/api/atletas` | Lista atletas com filtros e paginação | 200 OK |
| `GET` | `/api/atletas/{id}` | Obtém atleta por ID | 200 OK / 404 |
| `POST` | `/api/atletas` | Enfileira criação de atleta | 202 Accepted |
| `PUT` | `/api/atletas/{id}` | Enfileira atualização de atleta | 202 Accepted / 404 |
| `DELETE` | `/api/atletas/{id}` | Enfileira exclusão (soft delete) | 202 Accepted / 404 |

**Parâmetros de listagem:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `nome` | string | Filtro parcial por nome (case-insensitive) |
| `faixa` | string | Filtro exato por faixa (ex: `Azul`) |
| `pagina` | int | Número da página (padrão: 1) |
| `tamanhoPagina` | int | Itens por página (padrão: 10) |

**Swagger:** disponível em `/swagger` quando a API estiver em execução.

---

## Testes

```bash
dotnet test JiuJitsu.Tests
```

6 testes unitários cobrindo:
- `CriarAtletaCommandHandler` — publicação da mensagem e retorno do ID
- `ExcluirAtletaCommandHandler` — validação de existência e publicação
- `ListarAtletasQueryHandler` — repasse de filtros e paginação ao repositório

---

## Migrations

As migrations são aplicadas automaticamente na inicialização. Para criar uma nova migration após alterar o Domain:

```bash
dotnet ef migrations add <NomeDaMigration> \
  --project JiuJitsu.Infrastructure \
  --startup-project JiuJitsu.Api
```

---

## Estrutura de mensagens RabbitMQ

```
Exchange: atletas.exchange (direct)
  ├── atleta.criado    → atletas.queue
  ├── atleta.atualizado → atletas.queue
  └── atleta.excluido  → atletas.queue

Em caso de falha:
  atletas.queue → atletas.dlx (fanout) → atletas.dlq
```

---

## Fluxo de uma operação de escrita

1. React envia `POST /api/atletas`
2. API valida e despacha `CriarAtletaCommand` via MediatR
3. Handler publica `AtletaMensagem` no RabbitMQ e retorna o ID gerado
4. API responde `202 Accepted` com o ID
5. Worker lê a mensagem, instancia a entidade `Atleta`, salva no PostgreSQL
6. Worker envia email de confirmação via MailHog
7. React recarrega a lista após um breve delay

> As operações de **leitura** (GET) consultam o banco diretamente via Dapper — sem passar pelo RabbitMQ.
# bjj-system-manager
# bjj-system-manager
