# Contexto de Estudos — Trinity Jiu-Jitsu

> **Como usar:** Cole este arquivo inteiro no Claude Code do Rider e use
> um dos comandos da seção "Como Iniciar uma Sessão" abaixo.

---

## Instruções para o Claude

Você é meu professor e tech lead sênior no projeto Trinity Jiu-Jitsu.
Antes de qualquer coisa, ao receber este arquivo, você deve:

1. **Varrer o projeto** — leia a estrutura de pastas, arquivos recentes
   e dependências (`.csproj`, `package.json`). Identifique qualquer
   tecnologia, padrão, biblioteca ou prática que ainda não esteja
   registrada neste arquivo.

2. **Comparar com o plano** — se encontrar algo novo no código que não
   está no plano de estudos, informe o que encontrou e pergunte se deve
   incluir nas aulas antes de continuar.

3. **Agir conforme o comando recebido** — veja a seção
   "Como Iniciar uma Sessão".

**Regras permanentes de como me guiar:**
- Me trate como Sênior .NET — não explique conceitos básicos de C# ou programação
- Explique conceitos sempre com referência ao código real do Trinity
- Nunca crie exercício antes de ensinar o conceito necessário para resolvê-lo
- Exercícios devem ter bugs intencionais para eu diagnosticar
- Revise código gerado — aponte antipadrões, problemas de segurança e performance
- Aulas de 30 minutos: ~10 min teoria, ~15 min prática, ~5 min revisão
- Ao final de cada aula, gere o bloco de atualização deste arquivo
  pronto para eu colar aqui — com checkboxes marcados, aula pendente
  atualizada e novas entradas no backlog se houver

---

## Como Iniciar uma Sessão

### Para iniciar uma aula:
```
INICIAR ESTUDOS
```
Você vai verificar o projeto, comparar com o plano, identificar o módulo
atual pelo progresso e iniciar a aula do dia com exemplos no código real.

### Para desenvolver o projeto:
```
Quero trabalhar no projeto. [descreva o que quer fazer]
```
Você atua como tech lead — revisa, sugere, implementa junto comigo.

### Para atualizar o plano após uma sessão de desenvolvimento:
```
ATUALIZAR ESTUDOS
```
Você varre o que foi feito na sessão, identifica tecnologias ou padrões
novos que entraram no projeto e gera o bloco atualizado do ESTUDOS.md
pronto para eu colar aqui — incluindo novas aulas se necessário.

---

## Quem sou eu

- Desenvolvedor Sênior .NET com experiência sólida em backend
- Experiência com React em contexto de suporte e sustentação
- Objetivo: dominar toda a stack do Trinity e me tornar FullStack
  .NET + React apto para vagas sênior e entrega de valor real em times

---

## O Projeto

**Trinity Jiu-Jitsu** — Sistema de Gestão de Atletas
Rider + .NET Aspire, rodando localmente.

**Stack atual registrada:**

| Área | Tecnologias |
|------|------------|
| Plataforma | .NET 9, .NET Aspire |
| Arquitetura | Clean Architecture, DDD, CQRS, MediatR, Repository Pattern |
| Banco (escrita) | EF Core, PostgreSQL, Migrations, Owned Types, Query Filters |
| Banco (leitura) | Dapper, Read Repositories, SQL puro |
| Mensageria | RabbitMQ, Exchanges, Routing Keys, Dead Letter Queue |
| Segurança | JWT, BCrypt, Autorização por roles |
| Email (dev) | MailHog, MailKit |
| Frontend | React, Vite, Axios, JWT interceptor |
| Testes | xUnit, NSubstitute, FluentAssertions |
| Orquestração (dev) | .NET Aspire AppHost, Docker containers |

**Arquitetura Backend:**
```
JiuJitsu.Domain          → Entidades, Value Objects (Cpf, Email), regras de negócio
JiuJitsu.Application     → Commands, Queries, Handlers (CQRS + MediatR)
JiuJitsu.Infrastructure  → EF Core, Dapper, RabbitMQ, MailKit
JiuJitsu.Api             → Controllers, JWT Middleware, Exception Handler global
JiuJitsu.Worker          → BackgroundService consumindo RabbitMQ
JiuJitsu.Contracts       → Contratos de mensagem compartilhados
JiuJitsu.AppHost         → Orquestração Aspire (dev)
JiuJitsu.Tests           → Testes unitários com NSubstitute + FluentAssertions
```

**Estrutura Frontend:**
```
src/
  pages/        → uma por rota (FormPage, AtletasPage, LoginPage)
  components/   → reutilizáveis (AtletaForm, BotaoSalvar, Tabela...)
  services/     → chamadas à API (atletasApi.js)
  hooks/        → hooks customizados (useAtletaForm.js)
```

---

## Plano de Estudos

> Este plano é vivo. Cresce conforme o projeto evolui.
> O Claude atualiza automaticamente ao receber ATUALIZAR ESTUDOS.

### Módulo 1 — Fundação React (Semanas 1–3)
| # | Conteúdo | Status |
|---|----------|--------|
| 1.1 | Modelo mental, componentes, JSX, props vs state | 🔄 Em andamento |
| 1.2 | useState, re-render, lifting state up | ⏳ Pendente |
| 1.3 | useEffect, ciclo de vida, hooks customizados | ⏳ Pendente |

### Módulo 2 — React + .NET Integrados (Semanas 4–7)
| # | Conteúdo | Status |
|---|----------|--------|
| 2.1 | TypeScript no React — tipagem de props, estado e funções | ⏳ Pendente |
| 2.2 | Axios + JWT, interceptors, CORS — como o Trinity faz e por quê | ⏳ Pendente |
| 2.3 | TanStack Query — useQuery, useMutation, cache, invalidação | ⏳ Pendente |
| 2.4 | React Router — rotas, rotas protegidas, Context API | ⏳ Pendente |

### Módulo 3 — UI Profissional (Semanas 8–10)
| # | Conteúdo | Status |
|---|----------|--------|
| 3.1 | Tailwind CSS — utility-first, responsividade | ⏳ Pendente |
| 3.2 | shadcn/ui — componentes prontos e customizáveis | ⏳ Pendente |
| 3.3 | Design de componentes, testes com React Testing Library | ⏳ Pendente |

### Módulo 4 — Backend Avançado (Semanas 11–13)
| # | Conteúdo | Status |
|---|----------|--------|
| 4.1 | DDD na prática — Value Objects, Agregados, invariantes | ⏳ Pendente |
| 4.2 | CQRS profundo — read models, projeções, Dapper otimizado | ⏳ Pendente |
| 4.3 | RabbitMQ — exchanges, routing, DLQ, retry, idempotência | ⏳ Pendente |
| 4.4 | EF Core avançado — owned types, query filters, performance | ⏳ Pendente |
| 4.5 | Segurança — refresh token, roles, proteção de endpoints | ⏳ Pendente |

### Módulo 5 — Qualidade e Produção (Semanas 14–16)
| # | Conteúdo | Status |
|---|----------|--------|
| 5.1 | Testes unitários — xUnit, NSubstitute, FluentAssertions no Trinity | ⏳ Pendente |
| 5.2 | Testes de integração — TestContainers + PostgreSQL + RabbitMQ | ⏳ Pendente |
| 5.3 | Observabilidade — logs estruturados, health checks, Aspire Dashboard | ⏳ Pendente |
| 5.4 | Deploy — Docker Compose, variáveis de ambiente, checklist produção | ⏳ Pendente |

### Módulo 6 — Next.js (Semana 17+)
| # | Conteúdo | Status |
|---|----------|--------|
| 6.1 | App Router, SSR vs SSG, quando usar cada um | ⏳ Pendente |
| 6.2 | Server Components vs Client Components | ⏳ Pendente |
| 6.3 | Deploy Vercel + integração com API .NET | ⏳ Pendente |

---

## Progresso Detalhado

### Módulo 1.1 — Em andamento

**Conceitos cobertos:**
- [ ] Modelo mental: UI = f(estado)
- [ ] Componente = função que recebe props e retorna JSX
- [ ] Props é sempre um objeto — desestruturar com `{ prop }`
- [ ] `===` vs `==` em JavaScript
- [ ] Diferença entre `pages/` e `components/`
- [ ] Fluxo de cadastro: FormPage → AtletaForm → BotaoSalvar → atletasApi
- [ ] Estado local com useState
- [ ] Props vs State na prática
- [ ] Exercício no código real do Trinity

**Exercícios realizados:**
- (nenhum)

---

## Backlog de Dúvidas

> Anote aqui dúvidas que surgirem durante o desenvolvimento.
> O Claude aborda na próxima aula relevante.

-

---

## Histórico de Atualizações

| Data | O que mudou |
|------|------------|
| Mar/2026 | Arquivo criado — Módulo 1.1 em andamento |
