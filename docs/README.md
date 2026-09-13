# 🗺️ Mapa da documentação — Soromaps

Ponto de entrada único de `/docs`. Se você é humano ou agente de IA chegando
agora no repo, comece aqui — não pelas subpastas direto.

O Soromaps é uma plataforma interativa para descobrir, avaliar e compartilhar
experiências em estabelecimentos locais de Sorocaba, com geolocalização +
gamificação + rede social. TCC da FATEC Sorocaba.

## Pastas

| Pasta | O que guarda | Quando ir lá |
|---|---|---|
| [`wiki/`](./wiki/00-home.md) | O sistema **como ele está**: arquitetura, banco, endpoints, autenticação, frontend, setup, deploy e backlog. | **Ponto de partida de verdade.** Não guarda especificação antiga — o que o TCC projetou está no `archive/`. |
| [`adr/`](./adr/README.md) | Por que cada decisão de implementação foi tomada, um arquivo por módulo, organizado por área (`admin/`, `user/`). | Vai mexer num módulo que já existe e quer saber por que foi feito assim antes de mudar. |
| [`todo/`](./todo/README.md) | Brainstorm do que **ainda não existe**, um `.md` por módulo de produto, por área. | Vai começar um módulo novo ou ver o que falta num que já roda parcial. |
| [`propostas/`](./propostas/README.md) | Spec técnico escrito **antes** de a mudança existir no código, aguardando aprovação/implementação. | Vai revisar ou implementar uma mudança de modelo/API já desenhada mas não construída. |
| [`archive/`](./archive/README.md) | Material histórico — nunca é fonte de verdade, nunca é apagado. | Curiosidade sobre uma decisão antiga ou export original de diagrama. |

## Por onde começar

- **Chegou agora no projeto?** [`wiki/00-home.md`](./wiki/00-home.md) → segue
  o "Como usar" de lá (visão geral → arquitetura atual → setup local).
- **Vai mexer em módulo que já roda sobre mock (admin/\*, `/places`)?**
  [`adr/README.md`](./adr/README.md) primeiro — a decisão já foi tomada,
  não reinventa.
- **Vai começar um módulo que ainda não existe?**
  [`todo/README.md`](./todo/README.md) — índice de status por área.
- **Vai tomar uma decisão de arquitetura nova?** Registra no `/CLAUDE.md`
  (raiz do repo) — é o log vivo cronológico do projeto inteiro, decisões de
  infra/stack incluídas.

## Pra agente de IA lendo pra codar

- "Por que isso foi feito assim?" → `adr/<área>/`
- "O que falta fazer nesse módulo?" → `todo/<área>/`
- "Qual o estado real do banco/API/deploy?" → `wiki/`
- "O que ainda é mock e o que já é dado real?" → [`wiki/09-backlog.md`](./wiki/09-backlog.md)
- "O que o TCC pedia originalmente?" → `archive/wiki-trilha-projetada/` — **histórico, não fonte de verdade**
- Contexto cronológico geral e decisões de stack/infra → `/CLAUDE.md`

## Convenção que atravessa as pastas

Nenhum `.md` deveria repetir texto de outro — quando um conteúdo se aplica
aos dois lugares, um linka pro outro em vez de copiar. `todo/*.md` de um
módulo que já saiu do mock tem uma linha só apontando pro ADR correspondente,
nunca a decisão duplicada nos dois arquivos.
