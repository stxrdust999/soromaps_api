# 04. Comunidade, pauta e perfil

Rotas: `/community`, `/community/[id]`, `/pautas/[slug]`, `/profile` e as
quatro abas.

## Comunidade · `/community`

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-COM-01 | Buscar exploradores | Digitar filtra a lista de perfis | RF-13¹ | 🟡 |
| RF-COM-02 | Exibir perfil público | `/community/[id]` mostra contribuição, selo e lugares do explorador | RF-01, RF-02 | 🟡 |
| RF-COM-03 | Ranking de contribuição | Placar geral e por bairro, com a linha do próprio usuário sempre visível | RF-10 | 🟡 |
| RF-COM-04 | Conceder o selo de verificado por régua objetiva | ≥5 visitas, ≥3 avaliações publicadas, 0 removidas e ≥1 mês de conta — função pura, sem decisão manual | — | 🟡 |
| RF-COM-05 | Publicar a régua do selo | O critério aparece inteiro na tela, com o que falta para quem não tem | — | 🟡 |

¹ A comunidade é o que **sobrou** de RF-13 sem o grafo: perfil público, ranking
e selo, sem botão de seguir.

## Pauta · `/pautas/[slug]`

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-COM-06 | Gerar rascunho de pauta com IA | O modelo recebe tema mais lista fechada de lugares com ficha de fatos, e devolve chapéu, título e parágrafos | — | 🟢 |
| RF-COM-07 | Revalidar a saída do modelo | O JSON passa por um schema Zod antes de chegar à tela | — | 🟢 |
| RF-COM-08 | Rotular pauta escrita por IA | O leitor vê a origem; pauta da equipe mostra "Escrita pela equipe" | — | 🟡 |
| RF-COM-09 | Esconder rascunho do público | `status: "rascunho"` responde 404 na rota pública | — | 🟡 |
| RF-COM-10 | Persistir a pauta e ter fila de revisão | Rascunho salvo com `slug`, `status`, `origem` e corpo, revisável em `/admin` | — | 🔴 |
| RF-COM-11 | Degradar sem a chave do modelo | Sem `GEMINI_API_KEY` a tela funciona e o gerador avisa que está desligado | — | 🟢 |
| RF-COM-12 | Restringir a geração a quem publica | Só quem tem papel editorial dispara o modelo | — | 🔴 |

**RF-COM-12 é a pendência mais cara desta área:** o gerador fica no cabeçalho da
vitrine, aberto a qualquer sessão, e cada clique queima cota de um modelo pago.

## Perfil · `/profile`

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-PER-01 | Reunir a área pessoal em abas de rota | Cinco abas com URL própria, botão voltar e F5 no lugar certo | RF-02 | 🟢 |
| RF-PER-02 | Mostrar a identidade da sessão | Nome e e-mail vêm do cookie, não de mock | RF-01 | 🟢 |
| RF-PER-03 | Mostrar a régua do selo item a item | Cada critério com atingido ou faltante, não só um selo aceso ou apagado | — | 🟡 |
| RF-PER-04 | Listar visitas em linha do tempo | Agrupadas por mês, sobre a mesma base que alimenta o ranking público | RF-10 | 🟡 |
| RF-PER-05 | Listar lugares salvos | Coleção do explorador, com atalho para o ponto | — | 🟡 |
| RF-PER-06 | Exibir a galeria de conquistas | Obtida mostra a data; travada mostra o quanto falta, não só cinza | — | 🟡 |
| RF-PER-07 | Derivar o título do explorador da contagem de conquistas | `0–2 Novato · 3–6 Explorador · 7–12 Guia local · 13+ Veterano`, sem coluna, sem XP | — | 🟡 |
| RF-PER-08 | Exibir placar e cobertura da cidade | Números do explorador e quanto de Sorocaba ele já cobriu | RF-10 | 🟡 |
| RF-PER-09 | Remover favorito | Desfazer o salvamento pela própria aba | — | 🔴 |
| RF-PER-10 | Desenhar a mancha explorada no mini-mapa | Bairros visitados destacados | RF-10 | 🔴 |

## Evidência

| ID | Onde |
|---|---|
| RF-COM-01..05 | `(explorer)/community/_components/`, `src/constants/verification.ts` |
| RF-COM-06, 07, 11 | `src/lib/gemini.ts`, `src/actions/stories.ts`, `src/validations/stories.ts` |
| RF-PER-01 | `(explorer)/profile/layout.tsx` e `PROFILE_TABS` em `src/constants/navigation.ts` |
| RF-PER-03 | `_components/verification-checklist.tsx` — `missingForVerification()` |
| RF-PER-06 | `_components/achievement-progress-card.tsx` |
| RF-PER-07 | `src/constants/explorer-titles.ts` |

## Notas

**RF-COM-04 é função pura de propósito.** Decisão caso a caso não escala nem se
explica a quem não recebeu; régua pura muda em um arquivo, sem migração. Se um
dia entrar chancela manual, é um booleano a mais no `&&`.

**RF-PER-07 é o que restou da gamificação.** Não existe XP nem coluna `nivel`:
conquista é estado idempotente, XP é acumulador que erra para sempre se
conceder duas vezes. Decisão de 2026-08-12.

**RF-PER-01 e RF-PER-02 são os únicos 🟢 do perfil** — navegação e identidade
são reais, o conteúdo das abas deriva de `src/mocks/profile.ts`. E deriva em vez
de repetir: o progresso de "visitar 5 cafeterias" conta cafeterias na lista de
24 visitas, não um número digitado, então o perfil privado e o público não têm
como discordar.

**Conquista de evento `seguir` não entra na galeria do explorador**, mesmo
existindo no catálogo do admin: cobrar "siga 15 pessoas" seria pedir o que a
plataforma não faz.

Detalhe em
[`adr/user/0003-comunidade-selo-e-pauta-ia.md`](../../adr/user/0003-comunidade-selo-e-pauta-ia.md)
e [`adr/user/0004-perfil-hub-com-abas.md`](../../adr/user/0004-perfil-hub-com-abas.md).

---

➡️ [05 — Administração](./05-administracao.md)
