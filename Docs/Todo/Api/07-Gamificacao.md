# 🏆 07. Motor de Gamificação e Conquistas

> **Status:** ✅ Concluído · **Depende de:** [04-ModeracaoDeLocais.md](./04-ModeracaoDeLocais.md), [05-AvaliacoesEComentarios.md](./05-AvaliacoesEComentarios.md), [06-AtividadeDoExplorador.md](./06-AtividadeDoExplorador.md) · [Índice](./README.md)

`tb_achievements` e `AchievementsController` já existem com `id`, `name` e `description`. Falta estruturar os critérios de desbloqueio no banco, criar a tabela de conquistas desbloqueadas por usuário (`tb_user_achievements`) e implementar o motor idempotente de avaliação de progresso.

---

## 🗄️ Banco de Dados

### 1. Enriquecer `tb_achievements` (`Achievement`)
- [x] Adicionar colunas em `tb_achievements`:
  - `icon` (`varchar(40)` — chave do ícone em `ACHIEVEMENT_ICONS`)
  - `color` (`varchar(20)` — token de cor do badge)
  - `event_type` (`varchar(30)` — `'visit'`, `'review'`, `'favorite'`, `'create_place'`, `'comment'`)
  - `required_quantity` (`int` — meta numérica, ex.: `1`, `5`, `10`)
  - `target_type` (`varchar(30)` — `'any'`, `'category'`, `'neighborhood'`, `'distinct_categories'`, `'distinct_neighborhoods'`)
  - `target_value` (`varchar(60)` null — slug da categoria ou nome do bairro quando `target_type` for específico)
  - `is_active` (`boolean`, default `true`)
  - `updated_at` (`timestamptz`)

### 2. Conquistas do Usuário (`tb_user_achievements`)
- [x] Criar tabela `tb_user_achievements` (`UserAchievement`):
  - PK composta `(user_id, achievement_id)` (FKs para `tb_users` e `tb_achievements` com `ON DELETE CASCADE`).
  - `unlocked_at` (`timestamptz`, default `now()`).

---

## ⚙️ Motor de Concessão (`AchievementEvaluatorService`)

- [x] Implementar método `EvaluateForUserAsync(Guid userId, AchievementEventType eventType)`:
  - Busca todas as conquistas ativas daquele `event_type` que o usuário **ainda não possui** em `tb_user_achievements`.
  - Calcula o contador atual do usuário no banco conforme `(event_type, target_type, target_value)`:
    - `visit` → conta em `tb_visits` (join com `tb_places` quando filtrar por categoria/bairro).
    - `review` → conta em `tb_reviews` (`WHERE removed_at IS NULL`).
    - `favorite` → conta em `tb_favorites`.
    - `create_place` → conta em `tb_places` (`WHERE status = 'approved'`).
    - `comment` → conta em `tb_comments` (`WHERE removed_at IS NULL`).
  - Se `currentProgress >= required_quantity`, insere em `tb_user_achievements` (idempotente pela PK composta).
- [x] Disparar `EvaluateForUserAsync` após:
  - Check-in bem-sucedido (`VisitService`);
  - Criação de avaliação (`ReviewService`);
  - Adição de favorito (`FavoriteService`);
  - Aprovação de local pela moderação (`ModerationService`, para o `author_id` do local);
  - Criação de comentário (`CommentService`).

---

## 🔌 API

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `GET` | `/api/users/me/achievements` | Autenticado | Lista todas as conquistas ativas com `unlockedAt` (se conquistada) e progresso atual `{ current, required }` para exibir em `/profile/achievements`. |
| `GET` | `/api/achievements` | Anônimo / Admin | Lista o catálogo completo de conquistas (para Admin, inclui contagem `holdersCount` e percentual `holdersPercentage` sobre o total de exploradores). |
| `GET` | `/api/achievements/estimate` | `AdminOnly` | Recebe `?eventType=&requiredQuantity=&targetType=&targetValue=` e retorna quantos exploradores atuais cumpririam a regra (preview ao criar/editar conquista em `/admin/achievements`). |
| `POST` / `PATCH` / `DELETE` | `/api/achievements[/{id:guid}]` | `AdminOnly` | CRUD completo com os novos campos de critério e ícone/cor. |

---

## ✅ Verificação

- [x] Completar a 1ª visita concede automaticamente a conquista de primeira visita e grava em `tb_user_achievements`.
- [x] Rodar o avaliador múltiplas vezes para o mesmo usuário nunca duplica conquistas.
- [x] `GET /api/users/me/achievements` retorna o progresso parcial exato (ex.: `3/5` cafeterias visitadas) para conquistas ainda bloqueadas.
