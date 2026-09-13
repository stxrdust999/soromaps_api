# 03. Descoberta e feed

Rotas: `/discover` (absorveu `/places` em 2026-08-17) e `/feed`.
Nenhum requisito desta página toca a API: as duas telas rodam sobre
`src/mocks/markers.ts` e `src/mocks/feed.ts`.

## Descobrir · `/discover`

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-DES-01 | Apresentar a cidade em trilhas | Perto de você, melhor avaliados e recém-adicionados, cada uma com o critério visível | RF-11 | 🟡 |
| RF-DES-02 | Buscar lugar por nome | Digitar filtra os cards sem recarregar a página | RF-11 | 🟡 |
| RF-DES-03 | Filtrar por categoria | Chips de categoria combinam com a busca | RF-12 | 🟡 |
| RF-DES-04 | Filtrar por vibe | Chips de vibe combinam com categoria e busca | RF-12 | 🟡 |
| RF-DES-05 | Abrir a página do ponto pelo card | O card leva a `/places/[id]` | RF-06 | 🟢 |
| RF-DES-06 | Oferecer trilha personalizada | "Você esteve aqui" e recomendação por tag, a partir do histórico | RF-11 | 🔴 |
| RF-DES-07 | Manter estado vazio útil | Combinação de filtros sem resultado explica o que remover | — | 🟡 |

**RF-DES-06 é a razão de a tela se chamar Descobrir** — a personalização é para
onde ela vai, e espera a tabela `Visita`.

## Feed · `/feed`

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-FED-01 | Listar seis tipos de item | Avaliação, rajada, ponto novo, conquista, marco de lugar e pauta, despachados por `kind` | — | 🟡 |
| RF-FED-02 | Declarar o motivo em todo card | Nenhum card renderiza sem um dos cinco motivos: `perto`, `salvo`, `categoria`, `cidade`, `curadoria` | — | 🟡 |
| RF-FED-03 | Agregar rajada de avaliações | Uma linha com "4 pessoas avaliaram nas últimas 6 horas" no lugar de quatro cards repetidos | — | 🟡 |
| RF-FED-04 | Filtrar por fonte | Chips no topo restringem os motivos exibidos | RF-12 | 🟡 |
| RF-FED-05 | Ordenar por relevância ou por data | O cronológico agrupa por faixa de tempo | — | 🟡 |
| RF-FED-06 | Marcar avaliação como útil | Alterna o estado no item, e desmarcar volta ao anterior | RF-09 | 🟡 |
| RF-FED-07 | Acompanhar um lugar | Alterna entre acompanhar e acompanhando, com aviso em toast | RF-13¹ | 🟡 |
| RF-FED-08 | Ver menos disso | Silencia bairro, categoria ou tipo; a regra vira chip removível com contador no topo | RF-12 | 🟡 |
| RF-FED-09 | Paginar o feed | Carrega mais itens sem perder a posição de leitura | — | 🟡 |
| RF-FED-10 | Receber motivo e relevância do servidor | Quem sabe o que casou é a consulta; o front só desenha e deixa corrigir | — | 🔴 |

¹ Substitui `Seguir usuário`. `Segue` foi **cancelado** em 2026-08-17.

## Evidência

| ID | Onde |
|---|---|
| RF-DES-01..07 | `(explorer)/discover/_components/` — `use-discover.ts` concentra o estado |
| RF-FED-01 | `_components/feed-item.tsx` (despachante) + cinco `feed-*-card.tsx` |
| RF-FED-02 | `_components/feed-card-frame.tsx` — `reason` é obrigatório na moldura |
| RF-FED-04..09 | `_components/use-feed.ts` |
| RF-FED-02, 05 | `src/constants/feed.ts` — taxonomia de motivo e ordenação |

## Notas

**RF-FED-02 é obrigatório por construção, não por disciplina.** `FeedCardFrame`
exige `reason` no tipo: card que não sabe dizer por que está ali não compila.
Feed que não explica não dá ao usuário como corrigi-lo — e é o "ver menos
disso" (RF-FED-08) que faz a correção.

**RF-FED-06 é "útil", não "curtir", de propósito.** Útil mede se a dica ajudou
a decidir, e serve para ordenar avaliação na página do ponto; curtida mediria
simpatia pelo autor, que é o eixo que este feed não tem.

**RF-FED-01 quebra em tempo de compilação se alguém criar um tipo novo** — a
união é discriminada por `kind` e o `switch` do despachante é exaustivo.

**RF-FED-03 existe porque não há grafo.** Sem seguir, o lugar movimentado do dia
soterraria o resto do feed; agregar troca cinco cards repetidos por um número.

Justificativa completa em
[`adr/user/0002-feed-sem-grafo-social.md`](../../adr/user/0002-feed-sem-grafo-social.md).

---

➡️ [04 — Comunidade e perfil](./04-comunidade-e-perfil.md)
