# 02. Mapa e ponto

Rotas: `/home`, `/places/new`, `/places/[id]`.

## Mapa · `/home`

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-MAP-01 | Exibir mapa interativo centrado em Sorocaba | Abre em `SOROCABA_VIEWPORT` com pan, zoom e bússola | RF-03 | 🟢 |
| RF-MAP-02 | Sincronizar o basemap com o tema | Tema claro usa `positron`, escuro usa `dark-matter`, trocando sem recarregar a página | RF-03 | 🟢 |
| RF-MAP-03 | Carregar marcadores conforme o zoom | Com zoom ≥ 14 busca a lista; abaixo, limpa. Uma requisição por cruzamento do limiar, não por quadro | RF-04 | 🟢 |
| RF-MAP-04 | Centralizar na posição do usuário | Controle de localizar usa a geolocalização do navegador | RF-04 | 🟡 |
| RF-MAP-05 | Identificar o marcador no hover | Rótulo de uma linha, que some no `mouseleave` | RF-04 | 🟢 |
| RF-MAP-06 | Abrir o card do marcador no clique | Card de display com atalho "Ver detalhes"; nada de formulário dentro dele | RF-06 | 🟢 |
| RF-MAP-07 | Sobrepor painel arrastável ao mapa | O painel sobe até virar página **sem desmontar o mapa** | — | 🟡 |
| RF-MAP-08 | Buscar pontos por raio ou área visível | Filtro por bounding box na consulta | RF-11 | 🔴 |

**RF-MAP-04 é 🟡** porque a posição não é persistida nem usada para ordenar
nada — "perto de você" em `/discover` e o motivo `perto` do feed são mock.

**RF-MAP-07 é 🟡** pela casca, não pelo conteúdo: o `MapDrawerLayout` funciona,
o feed que ele exibe é fictício.

**RF-MAP-03 tem uma pegadinha de ambiente.** O hook lê
`NEXT_PUBLIC_API_URL || ""`, que em produção é string vazia — a chamada vira
`/api/markers`, caminho relativo. Isso **não** dá 404 por acaso: existe um
`rewrites()` em `next.config.ts` encaminhando `/api/markers/:path*` para
`${API_URL}`. Ou seja, já há um proxy server-side para markers, e ele também
dispensa o CORS. Ver `RNF-OPE-06` — a nota de `wiki/08-deploy.md` e do
`/CLAUDE.md` que descreve isto como quebrado precisa ser reconferida contra a
Vercel.

## Ponto · `/places/new` e `/places/[id]`

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-PTO-01 | Cadastrar ponto em dois estágios | `picking` posiciona o pino, `form` coleta os dados, ambos no mesmo card flutuante sobre o mapa | RF-05 | 🟡 |
| RF-PTO-02 | Trocar de lugar sem perder o preenchido | "Trocar de lugar" volta ao estágio `picking` e o formulário conserva os campos | RF-05 | 🟢 |
| RF-PTO-03 | Ver a página de detalhe do ponto | `/places/[id]` mostra a ficha completa e avisa quais campos são exemplo | RF-06 | 🟡 |
| RF-PTO-04 | Editar ponto | Formulário na página cheia grava via `PUT /api/markers/{id}` | RF-06 | 🟢 |
| RF-PTO-05 | Excluir ponto | Diálogo de confirmação, `DELETE /api/markers/{id}`, cache invalidado | moderação | 🟢 |
| RF-PTO-06 | Restringir editar e excluir a quem tem papel | Sessão comum não vê nem alcança as duas ações | — | 🔴 |
| RF-PTO-07 | Persistir foto, sobre, categoria, wifi, petfriendly, melhor horário e segredo local | O que o formulário coleta é o que a API grava | RF-06 | 🔴 |
| RF-PTO-08 | Enviar foto do ponto | Upload com armazenamento e URL persistida | RF-08 | 🔴 |
| RF-PTO-09 | Vincular o ponto a quem criou | FK de `markers` para o usuário | — | 🔴 |

## Evidência

| ID | Onde |
|---|---|
| RF-MAP-01, 02, 05, 06 | `src/components/ui/map.tsx`, `(explorer)/home/_components/*` |
| RF-MAP-03 | `src/hooks/use-markers.ts` — `moveend` + booleano de limiar + `AbortController` |
| RF-MAP-07 | `src/components/blocks/map-drawer-layout/` |
| RF-PTO-01, 02 | `(explorer)/places/new/_components/*`, `src/actions/markers.ts` |
| RF-PTO-03, 04, 05 | `(explorer)/places/[id]/`, `src/http/markers/markers.ts` |

## Notas

**RF-PTO-01 é 🟡 dentro de um fluxo que grava.** O formulário valida 8 campos e
a API recebe 3 (`nome`, `lat`, `lng`) — os outros 5 são descartados no envio. É
deliberado: o time quis ver o fluxo inteiro antes de mexer no banco. Spec em
[`propostas/2026-08-03-expansao-modelo-ponto.md`](../../propostas/2026-08-03-expansao-modelo-ponto.md).

**RF-PTO-06 não é hipótese.** Hoje qualquer sessão válida edita e exclui
qualquer ponto, e `markers` não tem dono (RF-PTO-09) — nem sequer daria para
restringir ao autor.

**A conversão `FormData` → número mora na action** (`toMarkerInput`), não no
schema Zod: `z.coerce.number()` deixa o tipo de entrada `unknown` e quebra o
`zodResolver`.

---

➡️ [03 — Descoberta e feed](./03-descoberta-e-feed.md)
