# Explorar lugares: trilhas por critério, sobre marker real + resto mock

> Módulo: user/explore · Rota: `/discover` · Data: 2026-08-09
>
> A rota era `/places` quando esta ADR foi escrita; a tela virou `/discover` em
> 2026-08-17, com os componentes em `discover/_components/`. As decisões abaixo
> continuam valendo.

## O que foi implementado

Feed de lugares em trilhas horizontais sobre a mesma lista de pontos, cada
uma com um critério diferente — **Perto de você**, **Em alta**, **Nota
máxima da galera**, **Joias escondidas** e **Recém-adicionados** — mais dois
eixos de recorte no topo: chips de **categoria** e chips de **vibe**.

| Peça | Onde |
|---|---|
| Trilhas, chips de categoria e vibe, tiles por tipo | `discover/_components/discover-explorer.tsx` |
| Card de local reutilizável | `src/components/blocks/place-card.tsx` |
| Trilha horizontal com cabeçalho e "Ver tudo" | `src/components/blocks/place-rail.tsx` |
| Categorias, vibes e cor de tag | `src/constants/places.ts` |

## Decisões

- **Recorte por vibe, além de categoria.** Categoria responde "o que é",
  vibe responde "pra quê" — um bar e um parque podem servir à mesma noite
  com a galera, e é assim que as pessoas decidem, não por taxonomia. É a
  parte que não existe em app de review comum.
- **A lista de pontos vem real** (`getMarkers()`); **todo o resto é
  fictício** e sai de `src/mocks/markers.ts`, escolhido de forma
  determinística pelo id do marker — mesmo padrão usado nas outras telas que
  leem `src/mocks/markers.ts` (feed da home, página de detalhe).

## 2026-08-17 — a tela absorve `/discover` e muda de rota

`/places` (esta tela) e `/discover` (planejada, nunca construída) respondiam à
**mesma pergunta do usuário** — "que lugar vale hoje?" — com critérios
diferentes: aqui público e igual para todos, lá derivado do histórico de
visitas. Critério é seção, não rota.

A tela virou `/discover`, segunda posição da navegação. A personalização que
justificaria a rota separada entra como **mais uma trilha** ("Você esteve
aqui", recomendação por tag em comum), no dia em que `Visita` existir — ao lado
das cinco que já existem, reusando chips, busca e cards em vez de duplicá-los.

Somando o [feed](./0002-feed-sem-grafo-social.md), eram três telas disputando
"sugerir lugar". A fronteira que ficou: **catálogo** aqui (o que existe, como
filtrar), **cronologia** no feed (o que mudou), e o feed personaliza por
vínculo com lugar, não por histórico de visita.

**O nome que ficou foi `Descobrir`** porque a tela faz descoberta, não
listagem — quem quer lista abre o mapa ou busca —, e porque era o nome que já
carregava a promessa de personalização.

O que mudou no código:

| Antes | Depois |
|---|---|
| `places/_components/places-explorer.tsx` | `discover/_components/discover-explorer.tsx` |
| `places/_components/places-header.tsx` | `discover/_components/discover-header.tsx` |
| `places/page.tsx` (vitrine) | `discover/page.tsx`; a antiga virou `redirect("/discover")` |
| Sidebar: "Explorar Lugares" + "Descobrir" | Sidebar: "Descobrir", 2ª posição |

**A rota-índice `/places` continua existindo, só redirecionando**, porque
`/places` segue sendo prefixo real de `/places/[id]` e `/places/new`: sem ela,
uma URL que o produto já publicou responderia 404.

**A régua que sai daqui:** tela nova só se responder uma pergunta que nenhuma
outra responde. Pelo mesmo critério, `/visits`, `/favorites` e `/stats` são
abas de `/profile` — pendente de decisão do time.

## Pendências conhecidas

- Foto, descrição e categoria no ponto — colunas ausentes em `markers`,
  spec em `docs/propostas/2026-08-03-expansao-modelo-ponto.md`
- `Categoria` como tabela (hoje os chips são constante no front)
- `tags` no modelo do ponto (alimentam vibe e recomendação) — só existem no
  mock
- `Analise` — nota e total de avaliações
- `Visita` — critério real de "Em alta" (hoje é nota, não movimento)
- Geolocalização do usuário para a distância real — `distancia` é número
  fixo no mock
- Paginação / bounding box em `GET /api/markers` — a rota devolve a tabela
  inteira

Detalhe completo do escopo restante em
[docs/todo/user/explore.md](../../todo/user/explore.md). O documento de
planejamento anterior, de quando a tela era `/places`, está em
[docs/archive/telas-fundidas/places.md](../../archive/telas-fundidas/places.md).
