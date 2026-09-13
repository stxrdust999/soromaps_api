# Perfil: hub com cinco abas, no lugar de cinco rotas

> Módulo: user/profile · Rota: `/profile` · Data: 2026-08-19

## O que foi implementado

`src/app/(app)/(explorer)/profile/`, com `layout.tsx` servindo o cabeçalho e a
navegação, e uma pasta por aba. Identidade vem da sessão real; contadores e
atividade vêm de mock.

| Peça | Arquivo |
|---|---|
| Cabeçalho (sessão + contadores + selo) e moldura | `profile/layout.tsx` |
| Navegação entre abas (único client da tela) | `_components/profile-tabs.tsx` |
| `Tabs` do shadcn, entrada nova no `ui/` | `src/components/ui/tabs.tsx` |
| Régua do selo, item a item | `_components/verification-checklist.tsx` |
| Conquista travada com progresso | `_components/achievement-progress-card.tsx` |
| Gráfico de atividade | `_components/weekly-activity-chart.tsx` |
| Abas | `page.tsx`, `visits/`, `favorites/`, `achievements/`, `stats/` |
| Atividade fictícia derivada | `src/mocks/profile.ts` |
| Contador de perfil, promovido | `src/components/blocks/stat-card.tsx` |
| Lista de abas | `PROFILE_TABS` em `src/constants/navigation.ts` |

## As cinco rotas viraram uma

`/visits`, `/favorites`, `/stats` e `/achievements` eram itens próprios do
grupo "Eu" da sidebar e cinco stubs de dez linhas. Passam a ser abas, e as
rotas antigas viraram `redirect()` — mesmo arranjo de `/places`, que fundiu em
`/discover` em 2026-08-17.

A régua é a daquela decisão: **tela nova só se responder uma pergunta que
nenhuma outra responde**. Histórico, coleção, galeria e placar respondem à
mesma pergunta ("o que eu já fiz?") com recortes diferentes do mesmo dado, e
recorte é aba. Mantê-las separadas obrigaria cada uma a repetir cabeçalho,
identidade e contadores — e deixaria o usuário navegando pela sidebar para
comparar números que pertencem à mesma tela.

## Aba é segmento de rota, não `useState`

As abas de `/admin/reports` e `/admin/achievements` são estado de cliente,
porque ali a aba faz parte de um fluxo de trabalho contínuo. Aqui não: cada aba
é conteúdo que se lê, se compartilha e se volta. Segmento de rota entrega URL
compartilhável, botão voltar funcionando e F5 caindo no lugar certo — e mantém
as cinco abas como Server Components, com um único `"use client"` na navegação,
que precisa do `usePathname`.

O visual vem do `Tabs` do shadcn (`variant="line"`), que entrou no `ui/` por
esta tela. Ele é usado **só como desenho**: cada `TabsTrigger` é `asChild` de um
`Link`, e o `value` do `Tabs` sai do pathname — quem troca de aba é o roteador,
não o Radix. O sublinhado do ativo é um `::after` em `bottom: -5px`, então o
scroll horizontal mora no `Tabs` raiz com `pb-1.5` — posto no `TabsList`, o
`overflow` recortaria justamente o traço que marca a aba.

## A pergunta que `/profile` responde, e `/community/[id]` não

O perfil público já existia e é vitrine: responde *"dá para confiar nessa
pessoa?"*. Espelhar isso em `/profile` seria duas telas disputando a mesma
pergunta — exatamente o que a régua acima proíbe.

`/profile` responde **"o que eu já fiz e o que falta para o próximo passo?"**,
e a diferença é estrutural, não de tom:

| | `/community/[id]` | `/profile` |
|---|---|---|
| Selo | badge "tem" / "não tem" | os quatro critérios com o progresso de cada um |
| Conquista | contador | galeria com as travadas e o quanto falta |
| Visitas | contador | timeline por mês, com repetição |

`missingForVerification()` (`src/constants/verification.ts`) existia desde a
Comunidade justamente para isto: transformar "você não tem" em roteiro. É aqui
que ele finalmente aparece na tela.

## Mock que deriva em vez de repetir

`currentExplorerMock` (`src/mocks/community.ts`) continua sendo a fonte dos
contadores, porque `/community` já os publica no ranking. `src/mocks/profile.ts`
**deriva** o resto:

- As 24 visitas são as 24 do contador, e o progresso de cada conquista é
  contado **sobre elas** — `visitar 5 lugares da categoria Cafeteria` conta
  cafeterias na lista, não um número digitado à mão. Mexer nas visitas mexe na
  galeria sozinho, e as duas telas não têm como discordar.
- Exatamente cinco conquistas fecham, que é o `conquistas: 5` do contador. Isso
  não foi ajustado à mão: caiu de pé porque os dados são os mesmos.
- `currentExplorerMock.ultimasAvaliacoes` estava vazio com `avaliacoes: 4`.
  Ganhou as quatro avaliações, que são as mesmas visitas com nota.

**Conquistas de evento `seguir` ficam fora da galeria.** `Segue` saiu do
produto em 2026-08-17 (ADR [0002](./0002-feed-sem-grafo-social.md)); cobrar
"siga 15 pessoas" seria pedir algo que a plataforma não faz. O catálogo do
admin continua com elas — quem decide o que é cobrável é a tela do explorador.

## Data fixa, fuso fixo

`PROFILE_ANCHOR = "2026-08-19"` e todo formatador de data preso em `timeZone:
"UTC"`. As datas chegam como dia puro (`2026-08-17`), que o `Date` lê como
meia-noite UTC: formatar no fuso local devolveria o dia anterior no Brasil, e
um fuso diferente no servidor e no navegador quebraria a hidratação. Mesma
regra que já valia para `Math.random()` e `Date.now()` em mock.

## `PlaceCard` e `PlaceRow` aceitam o recorte do marcador

Os dois pediam `MarkerResource` inteiro, mas só leem `id` e `nome` — o resto vem
de `getMarkerDetailsMock`. Passaram a aceitar `Pick<MarkerResource, "id" |
"nome">`, para telas sobre mock reusarem os cards sem inventar `lat`/`lng`. A
alternativa era duplicar os dois componentes, como o feed precisou fazer.

## Pendências conhecidas

- Tudo que não é identidade é mock: `Visita`, `Favorita`, `Analise`,
  `Conquista` e `GanhaConquista` não existem no banco.
- **Remover favorito não existe.** A ação nasce com a tabela, como
  `toggleFavoriteAction`; botão que não desfaz nada seria pior que botão nenhum.
- Sem edição de dados — é `/settings`, bloqueado pelo `PUT` que re-hasheia a
  senha a cada chamada. "Editar perfil" e "Gerenciar Conta" no popover da
  sidebar continuam sem destino.
- Mini-mapa da mancha explorada segue fase 2: exige camada MapLibre própria.
- O gráfico mostra oito semanas porque a conta do mock tem um mês. Com dado
  real, a janela provavelmente vira escolha do usuário.
