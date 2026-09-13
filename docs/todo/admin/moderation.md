# 🛂 Moderação de pontos

> Área: admin · Rota: `/admin/moderation` · Status: 🟡 parcial — tela pronta, dados em mock

## Ideia

Fila de aprovação do conteúdo que entra no mapa: ponto criado por usuário nasce `pendente` e só aparece publicamente depois do aceite do admin. A tela é a fila — dados do ponto, quem criou, mini-mapa da localização, aprovar/rejeitar (rejeição com motivo).

A coluna `status` em `PontoNoMapa` **já estava no modelo lógico do TCC** — o desenho sempre previu moderação; nunca foi implementada.

Esta fila julga **o ponto**, e só isso — reivindicação de posse saiu do produto junto com o [gerenciamento por dono](../../archive/gerenciamento-por-dono/), cancelado em 19/08/2026.

## Por que vale

- Mapa é vitrine pública: um ponto-lixo ("asdasd" no meio da praça) destrói a confiança que é a tese do produto.
- Duplicatas são inevitáveis com criação aberta; a fila é onde se detecta ("já existe ponto a 30m com nome parecido").
- Fecha os casos de uso do ator Administrador do TCC (remover conteúdo).

## Dependências

| O quê | Situação |
|---|---|
| Coluna `status` em `markers` (`pendente`/`aprovado`/`rejeitado`) | ❌ — prevista no modelo, nunca criada |
| Filtro de status no `GET /api/markers` público (só aprovados) | ❌ |
| Papel de admin | 🔴 pré-requisito |
| Aviso ao criador sobre o resultado | 💤 vira tipo em [Notificações](../user/notifications.md) |

**Decisão a tomar:** pontos já existentes entram como aprovados (grandfathering) — trivial, mas precisa estar no script da migração.

## Escopo inicial

- Fila com padrão de tabela + painel de detalhe com mini-mapa
- Aprovar / rejeitar com motivo (server actions)
- Alerta de possível duplicata por proximidade + nome similar

## Fora do escopo inicial

- Moderação de avaliações/comentários (é do módulo [Denúncias e feedback](./reports.md))
- Aprovação de reivindicação de ponto (cancelada em 19/08/2026 — ver [`archive/gerenciamento-por-dono/`](../../archive/gerenciamento-por-dono/))
- Auto-aprovação por reputação do criador

> Decisões já tomadas e componentes existentes em
> [adr/admin/0003-moderacao-mestre-detalhe.md](../../adr/admin/0003-moderacao-mestre-detalhe.md)
