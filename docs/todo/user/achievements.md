# 🏆 Conquistas

> Área: usuário público · Rota: `/profile/achievements` (aba de [Perfil](./profile.md)) · Status: 🟡 galeria pronta sobre `src/mocks/profile.ts` — faltam `Conquista` e `GanhaConquista`

## Ideia

Galeria de badges do usuário: as obtidas em cores com a data de obtenção, as bloqueadas em cinza **com o critério visível** ("Visite 5 cafeterias — 3/5"). Mostrar o progresso da conquista bloqueada é o que transforma a galeria em motor de comportamento: o usuário sabe exatamente o que falta.

Modelagem já definida no TCC: `Conquista` é catálogo, `GanhaConquista` é a N:N com `data` — criar conquista nova não mexe em `Usuario`.

## Por que vale

- É o pilar da gamificação inteiro — hoje zero implementado dos três pilares, este é o mais visível.
- Ataca a dor nº 6 (falta de reconhecimento pra quem contribui).
- Conquista compartilhável ("Explorador da Zona Norte 🏅") é marketing orgânico.

## Dependências

| O quê | Situação |
|---|---|
| Tabelas `Conquista` + `GanhaConquista` | ❌ não existem — já modeladas |
| Motor de concessão (avaliar critérios após check-in/avaliação) | ❌ — pode começar síncrono na própria action |
| `Visita` e `Analise` (fontes dos critérios) | ❌ |
| [Admin: catálogo de conquistas](../admin/achievements.md) | ❌ — irmão deste módulo |

## Escopo inicial

- 6–10 conquistas de lançamento (primeira avaliação, primeiro check-in, 5 lugares, 3 categorias, 1 por bairro…)
- Critérios **declarativos** no catálogo (`tipo` + `alvo` + `quantidade`), não hard-coded — é o que permite o admin criar conquista sem deploy
- Toast de celebração no momento da obtenção
- Galeria em `/achievements` + badges em destaque no perfil público

## Fora do escopo inicial

- **Pontuação/nível — descartado, não adiado.** Decisão de 2026-08-12 no
  `CLAUDE.md`: conquista é estado idempotente, XP é acumulador e exigiria
  ledger + curva + balanceamento por conquista. No lugar do nível entra um
  título derivado de `COUNT(GanhaConquista)`:
  `0–2 Novato · 3–6 Explorador · 7–12 Guia local · 13+ Veterano`
- Conquistas sazonais/por tempo limitado

> A rota antiga redireciona para a aba desde 2026-08-19. Decisões de
> implementação em [adr/user/0004-perfil-hub-com-abas.md](../../adr/user/0004-perfil-hub-com-abas.md)
