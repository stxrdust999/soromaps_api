# 🏠 Wiki do Soromaps

O Soromaps é uma plataforma interativa para descobrir, avaliar e compartilhar
experiências em lugares de Sorocaba. TCC da FATEC Sorocaba.

Esta wiki descreve **o sistema como ele está**: o que roda, sobre o que roda e
o que está quebrado. Ela não guarda especificação antiga — o que o TCC projetou
no papel está em
[`archive/wiki-trilha-projetada/`](../archive/wiki-trilha-projetada/README.md),
e o caminho de uma coisa até a outra está em [`/CLAUDE.md`](../../CLAUDE.md) e
em [`docs/adr/`](../adr/README.md).

---

## 📚 Páginas

| Página | Conteúdo |
|---|---|
| [01 — Visão geral](./01-visao-geral.md) | O que o produto é hoje, os três pilares e o que foi cortado do escopo |
| [02 — Arquitetura](./02-arquitetura.md) | Dois repos, Next.js 16 + ASP.NET Core 10 + Supabase + MapLibre, e os dois caminhos até a API |
| [03 — Banco](./03-banco.md) | As duas tabelas reais (`tbUsuario`, `markers`), ausência de migrations e o que falta |
| [04 — Endpoints da API](./04-api-endpoints.md) | Catálogo de rotas, payloads e quem consome cada uma |
| [05 — Autenticação e sessão](./05-autenticacao-e-sessao.md) | Login, cookie HS256 assinado pelo Next, middleware — e os riscos conhecidos |
| [06 — Frontend web](./06-frontend-web.md) | App Router, mapa de rotas, padrão de listagem, modais como rota, organização de pastas |
| [07 — Ambiente e setup](./07-ambiente-e-setup.md) | Como rodar web + API local, variáveis de ambiente, problemas comuns |
| [08 — Deploy](./08-deploy.md) | Vercel + Azure + Supabase, variáveis de produção e o defeito conhecido do mapa |
| [09 — Backlog e dívida](./09-backlog.md) | Tudo que se sabe que está errado, priorizado — a página que envelhece mais rápido |

---

## 🧭 Como usar

- **Chegou agora?** [01](./01-visao-geral.md) → [02](./02-arquitetura.md) → [07](./07-ambiente-e-setup.md). Em meia hora dá pra rodar tudo local.
- **Vai mexer no banco ou na API?** [09](./09-backlog.md) primeiro, depois [03](./03-banco.md) e [04](./04-api-endpoints.md).
- **Vai mexer em login?** [05](./05-autenticacao-e-sessao.md) — inclusive a seção de riscos.
- **Vai construir tela?** [06](./06-frontend-web.md) e o [ADR](../adr/README.md) da área.
- **Vai publicar ou mexer em produção?** [08](./08-deploy.md) — inclui o defeito que hoje derruba o mapa em produção.

---

## 🧱 O que esta wiki não cobre

| Assunto | Onde está |
|---|---|
| Por que uma tela ficou do jeito que ficou | [`docs/adr/`](../adr/README.md), um arquivo por módulo |
| O que ainda não foi construído | [`docs/todo/`](../todo/README.md), com status por módulo |
| Spec técnico aguardando implementação | [`docs/propostas/`](../propostas/README.md) |
| Requisitos, casos de uso e o modelo de 20 tabelas que as telas exigem | `docs/diagramas/` — pasta local, fora do controle de versão |
| Decisões de stack e infra, em ordem cronológica | [`/CLAUDE.md`](../../CLAUDE.md) |
| Material histórico | [`docs/archive/`](../archive/README.md) |

---

## 🔁 Como manter honesta

Toda entrega que muda comportamento atualiza a página correspondente **na mesma
entrega**, não depois. A regra vale em dobro para [09 — Backlog](./09-backlog.md):
item concluído sai de lá e do [`/CLAUDE.md`](../../CLAUDE.md) junto com o código
que o resolveu.
