# 04. Manutenibilidade

## Arquitetura e padrão

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-MAN-01 | Leitura e escrita têm camadas separadas | Leitura em `src/http` devolvendo envelope discriminado por `status`; escrita em `src/actions` com `"use server"`, Zod e `FormState` | 🟢 |
| RNF-MAN-02 | Erro de leitura é status, não exceção | Consumidor faz `status === 200 ? data : []`; não há `try/catch` na leitura | 🟢 |
| RNF-MAN-03 | Validação acontece na fronteira | Todo dado de fora passa por schema Zod em `src/validations/` antes de entrar no domínio | 🟢 |
| RNF-MAN-04 | Nenhuma tela chama `useReactTable` direto | Comportamento novo de tabela entra no `useTableConfig` global | 🟢 |
| RNF-MAN-05 | Identificador de cache vem de constante | Tag de cache nunca é string escrita na chamada | 🟢 |
| RNF-MAN-06 | Componente usado por uma rota mora nela | Um consumidor → `_components/` da rota; dois ou mais → `src/components/` | 🟢 |
| RNF-MAN-07 | TypeScript em modo estrito | `strict: true`, sem `any` implícito | 🟢 |
| RNF-MAN-08 | União de tipo é discriminada | Tipo novo quebra o `switch` em tempo de compilação, não em produção | 🟢 |

**RNF-MAN-01 existe porque leitura e escrita são contratos diferentes.** Leitura
precisa ser componível e cacheável; escrita precisa validar, invalidar cache e
devolver algo que o componente vire toast. Misturar os dois foi o que produziu o
`router.refresh()` da versão anterior.

**RNF-MAN-04 é a aposta que se pagou em `/admin/categories`:** busca, ordenação,
visibilidade de coluna, paginação e sheet de filtro vieram prontos, sem tocar em
nada compartilhado. O que custou foi só o domínio.

**RNF-MAN-06 tem uma exceção declarada:** `MapDrawerLayout` vive em
`components/blocks/` com um consumidor só, porque foi extraído justamente para
ser reusado.

## Mock

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-MAN-09 | Mock é determinístico | Nenhum `Math.random()` nem `Date.now()`; escolha por id (`id % lista.length`), série temporal como ruído sobre data-âncora fixa | 🟢 |
| RNF-MAN-10 | Mock deriva em vez de repetir número | Contador exibido em duas telas sai do mesmo cálculo sobre a mesma base | 🟢 |
| RNF-MAN-11 | Data em mock tem fuso fixo | Todo formatador preso em `timeZone: "UTC"` | 🟢 |
| RNF-MAN-12 | Mock avisa que é mock | Aviso no topo do arquivo e na tela que o consome | 🟢 |
| RNF-MAN-13 | Cada mock tem rota de morte declarada | O `.md` do módulo diz qual tabela o substitui | 🟢 |

**RNF-MAN-09 e RNF-MAN-11 são requisitos de correção, não de estilo.** Valor
aleatório, relativo a `now` ou formatado no fuso local renderiza diferente no
servidor e no cliente, e **quebra a hidratação**. Um dia puro como `2026-08-17`
é meia-noite UTC: formatado no Brasil, vira 16 de agosto.

## Qualidade e processo

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-MAN-14 | Lint e formatação em uma ferramenta só | `npm run lint` (Biome 2) limpo, com domínios `next` e `react` ligados | 🟢 |
| RNF-MAN-15 | Código em inglês, documentação e comentário em pt-BR | Vale para todo arquivo novo | 🟢 |
| RNF-MAN-16 | Comentário explica *por quê*, nunca *o quê* | JSDoc no símbolo exportado; `//` só para o não-inferível; nada de rótulo de bloco de JSX | 🟢 |
| RNF-MAN-17 | Decisão arquitetural é registrada quando é tomada | Entrada datada no `/CLAUDE.md` e ADR em `docs/adr/<área>/` na mesma entrega | 🟢 |
| RNF-MAN-18 | Existe teste automatizado | Suíte cobrindo ao menos as funções puras de regra (`verification`, `explorer-titles`, formatadores) | 🔴 |
| RNF-MAN-19 | Existe verificação automática antes do merge | Pipeline rodando lint, `tsc` e build a cada PR | 🔴 |
| RNF-MAN-20 | Não há código órfão | Nenhum arquivo ou pasta sem consumidor | 🟡 |

**RNF-MAN-18 é 🔴 total:** não há nenhum arquivo de teste no repositório, nem
runner instalado. As funções mais baratas de cobrir são as puras —
`isVerifiedExplorer`, `missingForVerification`, `explorerTitle` e os
formatadores — e são justamente as que carregam regra de produto.

**RNF-MAN-19 é 🔴:** não existe `.github/`. Hoje nada impede subir código que
não compila; o que segura é o build da Vercel falhar **depois** do push.

**RNF-MAN-20 é 🟡, com a lista fechada:** os Route Handlers
`src/app/api/auth/{login,logout}` são resquício da migração para Server Actions e
não têm chamador; a pasta `src/app/api/[TEMP]dev-bypass-login/` está **vazia**;
`registerSchema` em `src/validations/auth.ts` não é usado; e o `.gitignore` ainda
tem a regra `/src/services/Soromaps`, de quando a API morava neste repo.

## Documentação

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-MAN-21 | Diagrama é texto versionável | Mermaid no Markdown; export binário só em `docs/archive` | 🟢 |
| RNF-MAN-22 | Nenhum `.md` repete o texto de outro | Conteúdo compartilhado é linkado, nunca copiado | 🟢 |
| RNF-MAN-23 | Status de módulo é atualizado na mesma entrega | Concluiu módulo → mexe em `docs/todo/README.md` e no `.md` dele | 🟢 |
| RNF-MAN-24 | Material obsoleto é arquivado, nunca apagado | Vai para `docs/archive/` em subpasta por contexto | 🟢 |
| RNF-MAN-25 | Documento de estado não contradiz outro | Duas páginas não afirmam coisas diferentes sobre o mesmo comportamento | 🟡 |

**RNF-MAN-25 é 🟡 com dois casos abertos hoje:**
1. A coluna "Hoje" de `archive/wiki-trilha-projetada/02-requisitos.md` ainda marca RF-11, RF-12 e RF-13
   como não iniciados — foi escrita antes de `/discover`, `/feed` e
   `/community`.
2. `wiki/08-deploy.md` e o `/CLAUDE.md` descrevem os markers como quebrados em
   produção por caminho relativo, mas o `rewrites()` de `next.config.ts`
   encaminha `/api/markers/:path*` server-side desde 2026-06-07. Ver
   `RNF-OPE-06`.

## Evidência

| ID | Onde |
|---|---|
| RNF-MAN-01, 02 | `src/http/*/*.ts`, `src/actions/*.ts` |
| RNF-MAN-04 | `useTableConfig` + `src/components/table/` |
| RNF-MAN-07 | `tsconfig.json:7` |
| RNF-MAN-08 | `(explorer)/feed/_components/feed-item.tsx` |
| RNF-MAN-09..12 | `src/mocks/*.ts` |
| RNF-MAN-14 | `biome.json` |
| RNF-MAN-17 | `/CLAUDE.md` + `docs/adr/` |
| RNF-MAN-21 | `docs/diagramas/`, `docs/wiki/`, `docs/archive/diagramas-originais/` |

---

➡️ [05 — Operação e portabilidade](./05-operacao-e-portabilidade.md)
