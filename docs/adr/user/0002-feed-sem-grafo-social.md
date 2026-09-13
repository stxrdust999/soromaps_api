# Feed: cinco vínculos com a cidade no lugar de seguidores

> Módulo: user/feed · Rota: `/feed` · Data: 2026-08-17

## O que foi implementado

Tela em `src/app/(app)/(explorer)/feed/`, inteira sobre `src/mocks/feed.ts`
(24 itens em quatro faixas de tempo). Coluna de itens + coluna de apoio, com
chips de fonte no topo.

| Peça | Arquivo |
|---|---|
| Estado: filtro, ordenação, silenciamento, reações, paginação | `_components/use-feed.ts` |
| Moldura obrigatória do card (motivo + "menos disso") | `_components/feed-card-frame.tsx` |
| Despachante por `kind` | `_components/feed-item.tsx` |
| Cinco cards de item | `_components/feed-{review,burst,new-place,milestone,curation}-card.tsx` |
| Chips de fonte | `_components/feed-source-chips.tsx` |
| Coluna de apoio (recorte, desafio, ranking, movimento) | `_components/feed-aside.tsx` |
| Taxonomia de motivo e ordenação | `src/constants/feed.ts` |
| Título por contagem de conquistas | `src/constants/explorer-titles.ts` |

## `Segue` sai do produto

O feed não tem seguir, seguidor nem aba "Seguindo". Todo item entra por um
**vínculo com lugar**, declarado em cinco motivos: `perto` (bairro/raio),
`salvo` (lugar acompanhado), `categoria` (o que a pessoa explora), `cidade`
(movimento público) e `curadoria` (pauta da equipe).

O custo de `Segue` nunca foi a tabela, foi o que ela obriga:

- **Cold start.** Feed de grafo abre vazio para quem chegou agora, e a solução
  seria uma aba "descobrir" que, no fim, é este feed.
- **Escopo social.** Seguir traz bloqueio, perfil privado e denúncia de
  perseguição para dentro de um TCC.
- **Assunto errado.** Desloca o produto para gente, quando a tese é lugar.

Os cinco motivos saem de `Visita`, `Favorita` e da geolocalização — dados que o
produto precisa de qualquer forma. RF-13 fica fora do escopo.

## Seis tipos de item, união discriminada

`avaliacao`, `movimento`, `novo-ponto`, `conquista`, `marco` e `curadoria`,
discriminados por `kind` — tipo novo quebra o `switch` do despachante em tempo
de compilação, que é onde se quer descobrir que faltou desenhar o card.

**`movimento` é o que sustenta o feed sem grafo.** É rajada já agregada — "4
pessoas avaliaram o Cabocafé nas últimas 6 horas". Sem seguidores para dosar o
volume, o lugar movimentado do dia soterraria o resto; agregar troca cinco
cards repetidos por um número e uma média.

**`marco` é o tipo que só um produto de mapa tem:** o sujeito é o ponto, não
uma pessoa — "chegou a 100 avaliações", "virou a cafeteria mais bem avaliada do
bairro". Nasce do acúmulo, não de quem você segue.

## Decisões

- **Motivo é obrigatório por construção.** `FeedCardFrame` exige `reason` e as
  regras de "ver menos disso"; card que não sabe dizer por que está ali não
  compila. Feed que não explica não dá ao usuário como corrigi-lo.
- **"Ver menos disso" em três escopos** — bairro, categoria e tipo —, virando
  chip removível com contador no topo. Filtro que o usuário esqueceu de ter
  criado é pior que filtro nenhum. Itens de curadoria não oferecem bairro nem
  categoria: um roteiro fala de três lugares, e silenciar por ele calaria os
  outros dois de quebra.
- **"Útil" em vez de "curtir".** Útil mede se a dica ajudou a decidir e pode
  ordenar avaliação na página do ponto; curtida mede simpatia pelo autor, que é
  o eixo que este feed não tem.
- **"Acompanhar lugar" em vez de "seguir pessoa"** — a única assinatura do
  produto, e ela realimenta o motivo `salvo`.
- **Duas ordenações com comportamentos diferentes:** relevância mistura fontes
  numa lista corrida; cronológica agrupa em Hoje / Ontem / Esta semana / Antes
  disso. Agrupar por dia na ordenação por relevância contradiria a própria
  ordenação.
- **`relevancia` é campo do item, escrito à mão no mock.** Quando a API
  existir, `motivo` e `relevancia` vêm do backend: é a consulta que sabe o que
  casou. O front desenha e deixa corrigir.
- **Título do explorador vem de `COUNT(conquistas)`** (`explorerCredential`),
  materializando a decisão de 2026-08-12 — não existe nível nem pontuação.
- **Tempo é `diasAtras` + `hora` fixa**, nunca `Date.now()`: data relativa ao
  agora renderiza diferente no servidor e no cliente e quebra a hidratação.

## Pendências conhecidas

- Silenciamento e reações vivem só na sessão
- "Carregar mais" fatia o array; não há cursor
- O card `curadoria` leva a `/pautas/[slug]`, cuja pauta também é mock
- Os cinco lugares que ainda mostram "Nível N" sobre mock não migraram para
  `explorerCredential`
- O painel da home ainda não mostra o recorte "N novidades — ver feed"

Sai de 🟡 quando `Analise`, `Visita`, `Favorita`, `GanhaConquista` e a coluna
`status` do ponto existirem — ver
[docs/todo/user/feed.md](../../todo/user/feed.md).
