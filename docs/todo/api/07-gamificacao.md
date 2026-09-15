# 🏅 07. Gamificação

> **Status:** 💤 · **Depende de:** [04](./04-moderacao.md), [05](./05-contribuicao.md), [06](./06-atividade.md) · [Índice](./README.md)

Conquista só, sem XP (decisão de 2026-08-12). O título do explorador é
`COUNT(user_achievements)`. É a primeira regra de negócio real da API, e por
isso a fase que introduz a camada de serviço.

---

## 🗄️ Banco

- [ ] `achievements` — **`HasData`**:

| Coluna | Nota |
|---|---|
| `id`, `name` UNIQUE, `description`, `icon` (`ACHIEVEMENT_ICONS`), `color` | |
| `event` | `CHECK IN ('visit','review','create','favorite','streak')` — **sem `follow`** (seguir saiu do produto em 2026-08-17) |
| `quantity` | `CHECK >= 1` |
| `target_type` | `category` / `neighborhood` / null |
| `target` `varchar(60)` | nome da categoria ou do bairro |
| `active`, `created_at`, `updated_at` | |

  `CHECK ((target_type IS NULL) = (target IS NULL))`: escolher o tipo obriga o alvo.

- [ ] `user_achievements`: PK `(user_id, achievement_id)`, `earned_at`
  - PK composta é a decisão inteira: conceder é idempotente, e reprocessar o catálogo depois de corrigir um critério é seguro
- [ ] Filtrar do seed as conquistas de evento `follow` que ainda existem no mock

---

## ⚙️ Motor de concessão

- [ ] `AchievementService.EvaluateAsync(userId, event)`, chamado depois de visita, avaliação, favorito e aprovação de ponto
- [ ] Para cada conquista ativa do evento: recontar o critério contra `visits`/`reviews`/`favorites`/`places` e, se atingido, `INSERT ... ON CONFLICT DO NOTHING`
- [ ] Critério `streak` varre `visits` por janela de dias; pode exigir `source = 'gps'`
- [ ] Execução síncrona no request (volume de TCC); comando `dotnet run -- achievements:reprocess` para recalcular todos após mudança de critério
- [ ] **Introduzir a camada de serviço** (`Services/`): até aqui controllers falam direto com o `AppDbContext`; a regra de concessão é compartilhada por quatro controllers

---

## 🔌 API

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `GET` | `/api/me/achievements` | autenticado | Obtidas e travadas, com progresso ("3 de 5 cafeterias") |
| `GET` | `/api/users/{id}/achievements` | anônimo | Só obtidas (perfil público) |
| `GET` | `/api/admin/achievements` | admin | Catálogo com obtenções e raridade (`obtenções / COUNT(users)`) calculadas |
| `POST` / `PUT` | `/api/admin/achievements[/{id}]` | admin | Conquista nova é `INSERT`, não deploy |
| `GET` | `/api/admin/achievements/{id}/estimate` | admin | Quantos usuários já cumpririam o critério (aba de calibragem) |

---

## 🖥️ Front

- [ ] `/profile/achievements` e `/admin/achievements` sobre a API
- [ ] `explorerTitle`/`explorerCredential` recebem a contagem real
- [ ] Migrar os lugares que ainda mostram "Nível N" (`place-leaderboard`, `verified-comment-card`, `author-card`)

Mocks que saem: `src/mocks/admin-achievements.ts` e a parte de conquistas de `profile.ts`.

---

## ✅ Verificação

- [ ] A 5ª visita a cafeterias concede "visitar 5 cafeterias" uma vez só
- [ ] Rodar `achievements:reprocess` duas vezes não duplica nada
- [ ] Criar conquista com `target_type` e sem `target`: `400` (e o `CHECK` barra no banco)
- [ ] O título exibido muda ao cruzar 3, 7 e 13 conquistas
