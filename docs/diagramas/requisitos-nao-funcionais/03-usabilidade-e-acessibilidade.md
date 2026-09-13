# 03. Usabilidade e acessibilidade

## Interação e feedback

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-USA-01 | O produto tem tema claro e escuro, o mapa incluído | Trocar o tema troca o basemap (`positron` ↔ `dark-matter`) sem recarregar a página | 🟢 |
| RNF-USA-02 | Resultado de mutação sempre tem retorno visível | Toda Server Action devolve `FormState` que a tela transforma em toast | 🟢 |
| RNF-USA-03 | Erro de formulário aparece no campo | React Hook Form + Zod com mensagem no campo; **nenhum `alert()` no produto** | 🟢 |
| RNF-USA-04 | Toda listagem tem estado vazio desenhado | Lista sem item explica o que fazer, não fica em branco | 🟢 |
| RNF-USA-05 | Ação destrutiva pede confirmação | Excluir ponto, usuário, categoria e conquista abre diálogo | 🟢 |
| RNF-USA-06 | Remoção de conteúdo exige motivo | Catálogo único de motivos em `src/constants/content-removal.ts` | 🟢 |
| RNF-USA-07 | Filtro que o usuário criou é visível e reversível | Regra de "ver menos disso" vira chip removível com contador no topo | 🟢 |

**RNF-USA-03 fechou uma dívida concreta:** as três telas de marker eram `fetch`
cru no cliente com `alert()` de erro.

**RNF-USA-07 é regra de produto, não estética.** Filtro que o usuário esqueceu
de ter criado é pior que filtro nenhum — um feed que some com metade do
conteúdo sem dizer por quê não tem conserto pelo lado de quem lê.

## Navegação

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-USA-08 | Modal é rota, não estado | Modal tem URL compartilhável, botão voltar funcionando e sobrevive a F5 | 🟢 |
| RNF-USA-09 | Conteúdo que se lê e se compartilha tem URL própria | As abas de `/profile` são segmento de rota, não `useState` | 🟢 |
| RNF-USA-10 | URL publicada não morre | Rota fundida ou promovida vira `redirect()`, nunca 404 | 🟢 |
| RNF-USA-11 | Cada tela responde a uma pergunta que nenhuma outra responde | Tela nova só nasce com pergunta própria; recorte do mesmo dado é aba | 🟢 |
| RNF-USA-12 | Erro de rota tem página | `error.tsx`, `not-found.tsx` e `global-error.tsx` nas fronteiras | 🔴 |
| RNF-USA-13 | Espera tem esqueleto | `loading.tsx` nas rotas com busca de dado | 🟡 |

**RNF-USA-10 tem cinco `redirect()` vivos hoje:** `/places` → `/discover`, e
`/visits`, `/favorites`, `/stats`, `/achievements` → as abas de `/profile`.

**RNF-USA-12 é 🔴 e é o buraco mais visível desta página.** Não existe um único
`error.tsx` ou `not-found.tsx` no projeto: `/places/999` e qualquer exceção em
Server Component caem na tela de erro padrão do Next, em inglês, sem caminho de
volta.

**RNF-USA-13 é 🟡** porque a espera é tratada com `Suspense` e esqueleto local
em `/admin/users` e nos modais de usuário — nas demais rotas, não.

## Acessibilidade e idioma

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-USA-14 | Componente interativo é acessível por teclado e leitor de tela | Base Radix, com `aria-label` ou `sr-only` em todo controle só-ícone | 🟡 |
| RNF-USA-15 | Fila de trabalho é operável por teclado | A moderação percorre e decide sem mouse | 🟡 |
| RNF-USA-16 | Contraste atende WCAG AA nos dois temas | Texto e componente medidos em claro e escuro | ⚪ |
| RNF-USA-17 | Cor nunca é o único portador de informação | Estado de aprovado/atrasado leva rótulo além do semáforo | 🟡 |
| RNF-USA-18 | O documento declara o idioma correto | `<html lang>` reflete o conteúdo | 🟡 |
| RNF-USA-19 | Toda a interface está em pt-BR | Nenhum texto de tela em inglês | 🟢 |
| RNF-USA-20 | A interface funciona em tela pequena | Layout e sidebar utilizáveis no celular | ⚪ |

**RNF-USA-18 é 🟡 por um detalhe de uma linha:** o layout raiz declara
`lang="pt"`, e o conteúdo é `pt-BR`. Leitor de tela e correção ortográfica
escolhem a voz e o dicionário por esse atributo.

**RNF-USA-14 é 🟡, não 🟢:** há 56 ocorrências de `aria-label`/`sr-only` no
código e a base Radix cobre foco e papel ARIA, mas nunca houve auditoria. Sem
teste, "acessível" é intenção.

**RNF-USA-20 é ⚪** e importa mais do que parece: o app é sobre andar pela
cidade, então o celular é o dispositivo do caso de uso — e o cliente mobile
Expo previsto no TCC não foi iniciado.

**As cores de estado são `--success` e `--warning`, não cor de marca.**
Aprovado e atrasado precisam de semáforo; o resto do produto continua azul.

## Evidência

| ID | Onde |
|---|---|
| RNF-USA-01 | `next-themes` + `src/components/ui/map.tsx` |
| RNF-USA-02 | `src/types/form.ts` (`FormState`) + `sonner` |
| RNF-USA-03 | `src/components/ui/form.tsx`, escrito à mão sobre RHF |
| RNF-USA-06 | `src/components/blocks/removal-dialog.tsx` |
| RNF-USA-07 | `(explorer)/feed/_components/use-feed.ts` |
| RNF-USA-08 | `src/app/(app)/@modals/` e as rotas espelho |
| RNF-USA-09 | `(explorer)/profile/layout.tsx` + `PROFILE_TABS` |
| RNF-USA-15 | `admin/moderation/_components/use-moderation-queue.ts` |
| RNF-USA-18 | `src/app/layout.tsx:37` |

---

➡️ [04 — Manutenibilidade](./04-manutenibilidade.md)
