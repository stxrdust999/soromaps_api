# 02. Desempenho

A maior parte desta página existe porque um refactor específico a produziu: a
extração do `MapDrawerLayout` em 2026-08-02, que nasceu de três problemas
medíveis na home.

## Rede

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-DES-01 | Gesto no mapa não vira rajada de requisições | Um `GET /api/markers` por **cruzamento** do limiar de zoom, não por quadro do gesto | 🟢 |
| RNF-DES-02 | Requisição obsoleta é cancelada | Sair da faixa de zoom aborta a busca em andamento | 🟢 |
| RNF-DES-03 | Leitura de servidor é cacheada por tag | Toda leitura declara `next: { tags: [...] }` com a tag vinda de constante, nunca string solta | 🟢 |
| RNF-DES-04 | Escrita invalida o que ela mesma mudou | Server Action chama `updateTag`, e a leitura seguinte já enxerga a escrita | 🟢 |
| RNF-DES-05 | Listagem não traz a tabela inteira | `GET /api/markers` com paginação e recorte por área visível | 🔴 |

**RNF-DES-01 é a diferença entre `move` e `moveend`.** A versão anterior tinha
`viewport.zoom` como dependência de `useEffect`, e `move` dispara a cada quadro
durante o gesto — dezenas de requisições por zoom. Hoje `use-markers.ts` assina
`moveend` e guarda só o booleano `zoom >= minZoom`: o efeito depende de um
booleano, não de um float.

**RNF-DES-04 usa `updateTag`, não `revalidateTag`.** No Next 16 o
`revalidateTag` exige um segundo argumento de perfil de cache, e é o `updateTag`
que dá semântica de *read-your-own-writes* dentro de uma Server Action. É o que
faz a tabela chegar atualizada quando o modal fecha, sem `router.refresh()`.

**RNF-DES-05 é 🔴 e escala mal por construção:** a API devolve todos os markers
em toda chamada, e o mapa busca essa lista sempre que o zoom cruza o limiar.

## Render

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-DES-06 | O mapa nunca é desmontado durante a navegação | Expandir o painel até a altura de página mantém a instância MapLibre viva | 🟢 |
| RNF-DES-07 | Estado de alta frequência fica fora do React | O viewport não é `useState`; as camadas leem a instância por `useMap()` | 🟢 |
| RNF-DES-08 | Server Component é o padrão | `"use client"` só onde há gesto, estado de interação ou hook de navegação | 🟢 |
| RNF-DES-09 | A primeira pintura já mostra a estrutura | O gate de `mounted` do vaul renderiza o esqueleto do painel recolhido, não tela vazia | 🟢 |
| RNF-DES-10 | Imagem passa por otimização | `next/image` com `remotePatterns` declarado | 🟡 |
| RNF-DES-11 | Web Vitals medidos em produção | LCP, INP e CLS coletados e acompanhados | ⚪ |

**RNF-DES-06 e RNF-DES-07 corrigiram custos reais.** O mapa era desmontado ao
expandir (`{!isFullyExpanded && <Map/>}`), o que recarregava estilo CARTO e
tiles ao recolher; e o viewport em state re-renderizava drawer e feed a 60fps
durante o pan.

**RNF-DES-10 é 🟡** porque o único host remoto declarado é `picsum.photos`, que
serve as fotos de mock. Quando entrar upload real, o `remotePatterns` precisa
acompanhar — senão a imagem não renderiza.

**RNF-DES-11 é ⚪ de propósito:** não há coleta. Afirmar desempenho de
carregamento sem medir seria chute.

## Evidência

| ID | Onde |
|---|---|
| RNF-DES-01, 02 | `src/hooks/use-markers.ts` — `moveend`, booleano de limiar, `AbortController` |
| RNF-DES-03 | `src/constants/{markers,users}.ts` (tags) e as chamadas em `(app)/**/page.tsx` |
| RNF-DES-04 | `src/actions/{markers,users,stories}.ts` |
| RNF-DES-06, 07, 09 | `src/components/blocks/map-drawer-layout/` |
| RNF-DES-10 | `next.config.ts` — `images.remotePatterns` |

---

➡️ [03 — Usabilidade e acessibilidade](./03-usabilidade-e-acessibilidade.md)
