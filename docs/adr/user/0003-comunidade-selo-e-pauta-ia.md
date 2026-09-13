# Comunidade: selo por régua pública e pauta redigida por IA

> Módulo: user/community · Rotas: `/community`, `/community/[id]`, `/pautas/[slug]` · Data: 2026-08-17

## O que foi implementado

Três telas sobre `src/mocks/community.ts` (12 exploradores + o usuário da
sessão) e `src/mocks/stories.ts` (4 pautas, uma delas rascunho). O único
caminho de verdade é a chamada ao Gemini.

| Peça | Arquivo |
|---|---|
| Busca, grid e composição da tela | `community/_components/community-workspace.tsx` |
| Card de explorador | `community/_components/explorer-card.tsx` |
| Ranking geral × bairro | `community/_components/contribution-ranking.tsx` |
| Critério do selo, visível | `community/_components/verification-card.tsx` |
| Vitrine de pautas (destaque + duas) | `community/_components/story-showcase.tsx` |
| Gerador de rascunho | `community/_components/story-generator-dialog.tsx` |
| Perfil público | `community/[id]/page.tsx` |
| Pauta | `pautas/[slug]/page.tsx` |
| Régua do selo | `src/constants/verification.ts` |
| Cliente do modelo, server-only | `src/lib/gemini.ts` |
| Server Action + prompt + grounding | `src/actions/stories.ts` |
| Schemas de entrada e do rascunho | `src/validations/stories.ts` |

Sem botão de seguir em lugar nenhum — consequência direta de
[0002](./0002-feed-sem-grafo-social.md).

## Selo de explorador verificado: régua, não chancela

≥5 visitas, ≥3 avaliações publicadas, nenhuma removida e ≥1 mês de conta.
Função pura sobre contadores em `src/constants/verification.ts`.

- **Decisão caso a caso não escala nem se explica** para quem não recebeu.
- **A régua muda em um arquivo**, sem migração — é derivada, não coluna.
- **`missingForVerification` transforma "você não tem" em roteiro**: o perfil
  de quem não tem o selo lista o que falta, em vez de só negar.
- **O critério aparece inteiro na tela.** Selo cujo critério não se lê vira
  fofoca.

Chancela manual, se um dia entrar, é um booleano a mais no `&&`. O mock inclui
de propósito quem **não** passa: conta nova e alguém com avaliação removida —
amostra em que todo mundo é verificado não testa nada.

## Pauta com IA: geração é autoria, não render

Nada roda durante a navegação do leitor. Alguém abre o gerador, escolhe tema e
até quatro lugares, o modelo escreve, um humano decide. **Texto sobre comércio
real que inventa preço, horário ou qualidade vira prejuízo para um negócio de
verdade** — um guia local vive de não fazer isso.

Três defesas, nesta ordem:

1. **Os lugares vêm da tela.** O modelo recebe lista fechada e a ficha de fatos
   de cada lugar; não escolhe assunto sobre a cidade inteira.
2. **A instrução de sistema enumera o proibido** — preço, horário, endereço,
   telefone, prato, pessoa, prêmio, história — e manda calar sobre o que não
   estiver na lista.
3. **A saída é revalidada com Zod.** `responseSchema` garante forma, não
   conteúdo: saída de LLM é entrada não confiável como qualquer outra.

Mais duas regras de produto: pauta de origem `ia` **aparece rotulada** ao
leitor, e `status: "rascunho"` **responde 404** na rota pública.

`GEMINI_API_KEY` é server-only, sem `NEXT_PUBLIC_` — chave de modelo no bundle
é conta de terceiro paga por quem abrir o DevTools. `src/lib/gemini.ts` não
lança: devolve envelope discriminado como `src/http`, e falta de chave é estado
esperado (`sem-chave`), não exceção — sem a chave a tela funciona e o gerador
avisa que está desligado. `GEMINI_MODEL` sobrescreve o padrão
`gemini-2.5-flash`.

## Decisões

- **Pauta em rota própria, fora de `/community`.** Ela é destino de três
  lugares: a vitrine da comunidade, o card `curadoria` do feed (que agora leva
  a ela pelo `slug`) e, no futuro, a página do ponto. Aninhar daria uma URL que
  mente sobre o assunto, e evita `/community/pautas` disputar com
  `/community/[id]`.
- **A vitrine abre a tela, antes das pessoas.** Atividade de usuário tem dia
  fraco; em base nova há semana sem avaliação nenhuma. Editorial é o conteúdo
  que o produto controla.
- **Ranking por bairro ao lado do geral, com a linha do usuário fixa** quando
  ele está fora do topo. Em ranking geral só os dez primeiros existem; por
  bairro, quase todo explorador ativo é destaque de algo. Ranking em que a
  pessoa não se acha é ranking que ela não acompanha.
- **Contribuição é soma simples** de visitas, avaliações e pontos cadastrados.
  Peso por tipo de ação é calibragem que o produto ainda não tem como validar.
- **O rascunho não é persistido.** Não existe tabela de pauta: a action devolve
  o texto, a tela mostra com aviso e o revisor copia. Quando a entidade nascer,
  é a mesma action gravando com `status: "rascunho"`.
- **Perfil público sem botão nenhum de relação** — nem seguir, nem mensagem. A
  confiança se responde pelo histórico: o que a pessoa registrou, desde quando
  e o que escreveu.

## Pendências conhecidas

- Rascunho copiado à mão; falta persistir e criar a fila de revisão em `/admin`
- Busca e ranking recortam array no cliente, sem paginação nem filtro no server
- `isVerifiedExplorer` roda sobre contadores fictícios
- Os ids de lugar do mock podem não existir no banco, como nas outras telas

Sai de 🟡 quando `Analise`, `Visita`, `GanhaConquista` e a entidade de pauta
existirem — ver [docs/todo/user/community.md](../../todo/user/community.md).
