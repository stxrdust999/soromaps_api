# 📋 Requisitos funcionais — sistema atual

> Trilha: **atual**. O que o Soromaps faz hoje, 2026-08-19, lido do código.
> Os 13 requisitos de alto nível levantados no TCC continuam em
> [`archive/wiki-trilha-projetada/02-requisitos.md`](../../archive/wiki-trilha-projetada/02-requisitos.md) — esta pasta os
> desdobra no que virou tela.

## Páginas

| Arquivo | Área | Requisitos |
|---|---|---|
| [01 — Sessão e acesso](./01-sessao-e-acesso.md) | `(auth)` + `middleware.ts` | `RF-SES-*` |
| [02 — Mapa e ponto](./02-mapa-e-ponto.md) | `/home`, `/places/*` | `RF-MAP-*`, `RF-PTO-*` |
| [03 — Descoberta e feed](./03-descoberta-e-feed.md) | `/discover`, `/feed` | `RF-DES-*`, `RF-FED-*` |
| [04 — Comunidade e perfil](./04-comunidade-e-perfil.md) | `/community`, `/pautas`, `/profile` | `RF-COM-*`, `RF-PER-*` |
| [05 — Administração](./05-administracao.md) | `/admin/*` | `RF-ADM-*` |
| [06 — Rastreabilidade](./06-rastreabilidade.md) | — | RF do TCC ↔ requisito atual |

---

## Convenção de ID

`RF-<ÁREA>-<NN>`, com a área em três letras: `SES`, `MAP`, `PTO`, `DES`,
`FED`, `COM`, `PER`, `ADM`.

**Não se reusa a numeração `RF-01..RF-13` do TCC.** Aquele é um requisito de
alto nível ("permitir a avaliação de um ponto"); estes são o que o produto
executa, e um RF do TCC vira vários daqui. Reciclar o número faria os dois
documentos discordarem sobre o que "RF-07" significa. A ligação entre eles é a
coluna **RF-TCC** de cada tabela e a matriz da página 06.

## Legenda de estado

| Marca | Significado |
|---|---|
| 🟢 | Funciona ponta a ponta, com dado persistido |
| 🟡 | A tela funciona, mas o dado é fictício (`src/mocks/*`) ou o campo é descartado |
| 🔴 | Não implementado |
| ⛔ | Cancelado — decisão registrada, não é pendência |

🟢 aqui é o mesmo ✅ dos [diagramas de casos de uso](../casos-de-uso/README.md).

Cada requisito traz **critério de aceite verificável** — a frase que se testa
para dizer se está atendido — e a **evidência** no código. Requisito sem
critério é opinião.

---

## 🧭 Decisões deste documento

### Requisito descreve o que existe, não o que se deseja
**Decisão:** só entra aqui o que tem tela ou endpoint. O 🔴 aparece apenas
quando é a **contraparte direta** de algo já construído — "restringir edição de
ponto a administrador" existe porque a edição existe e não tem gate.
**Motivo:** desejo de produto já tem casa em [`todo/`](../../todo/README.md), e
dívida técnica em
[`wiki/12-gap`](../../wiki/09-backlog.md). Um terceiro
lugar listando o mesmo backlog envelheceria em duas semanas.

### Tela sobre mock é 🟡, nunca 🟢
**Decisão:** requisito cuja tela roda inteira mas lê de `src/mocks/*` fica 🟡,
mesmo com a interação completa.
**Motivo:** oito das telas do produto estão nesse estado. Se o documento
marcasse "feito" por haver interface, ele mediria esforço de front, não
capacidade do sistema — e é exatamente o risco que o 🟡 do
[`todo/README.md`](../../todo/README.md) já vigia.
