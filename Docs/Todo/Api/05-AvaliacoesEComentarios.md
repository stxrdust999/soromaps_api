# 💬 05. Avaliações, Comentários e Moderação de Conteúdo

> **Status:** ✅ Concluído · **Depende de:** [03-CatalogoELocais.md](./03-CatalogoELocais.md) · [Índice](./README.md)

`ReviewsController` e `CommentsController` já possuem CRUD básico sobre `tb_reviews` e `tb_comments`. Este módulo adiciona as regras de produto que faltam para `/places/[id]` (dados do autor da avaliação, voto de "útil", resposta a comentário de 1 nível, regra de 1 avaliação ativa por usuário/local) e para `/admin/reviews` (moderação global com sinais automáticos e log unificado de remoção).

---

## 🗄️ Banco de Dados

### 1. Ajustes em `tb_reviews` e `tb_comments`
- [x] `tb_reviews`:
  - Adicionar `removed_at` (`timestamptz` null) para soft-delete em remoções administrativas (necessário para a régua do selo de explorador verificado: *"nenhuma avaliação removida por moderação"*).
  - Índice único parcial `ON tb_reviews (place_id, user_id) WHERE removed_at IS NULL` (garante 1 avaliação ativa por usuário em cada local).
- [x] `tb_comments`:
  - Adicionar `reply_to_id` (`uuid` null FK → `tb_comments.id` `ON DELETE CASCADE`) para suportar respostas em até 1 nível de profundidade.
  - Adicionar `removed_at` (`timestamptz` null).

### 2. Votos de Utilidade (`tb_review_useful_votes`)
- [x] Criar tabela `tb_review_useful_votes` (`ReviewUsefulVote`):
  - PK composta `(review_id, user_id)` (FKs para `tb_reviews` e `tb_users` com `ON DELETE CASCADE`).
  - `created_at` (`timestamptz`, default `now()`).

### 3. Remoções Administrativas (`tb_content_removals`)
- [x] Criar tabela `tb_content_removals` (`ContentRemoval`) — compartilhada entre `/admin/reviews` e `/admin/reports`:
  - `id` (`uuid` PK, `gen_random_uuid()`)
  - `target_type` (`varchar(20)` — `'review'` | `'comment'`)
  - `target_id` (`uuid` — ID da avaliação ou comentário removido)
  - `author_id` (`uuid` FK → `tb_users` — autor do conteúdo removido)
  - `moderator_id` (`uuid` FK → `tb_users` — admin que removeu)
  - `reason` (`varchar(50)` — lista fechada `REMOVAL_REASONS`)
  - `note` (`varchar(500)` null)
  - `snapshot_text` (`text` — cópia congelada do texto no momento da remoção para auditoria)
  - `created_at` (`timestamptz`, default `now()`)

---

## 🔌 API

### Endpoints de Explorador (`/api/reviews` e `/api/comments`)
- [x] Enriquecer `ReviewResponseDto` e `CommentResponseDto` com:
  - Dados do autor (`id`, `name`, `avatarUrl`, `achievementsCount`, `isVerifiedExplorer`, `visitedPlace` indicando se o autor possui check-in em `tb_visits` naquele local).
  - `usefulVotesCount` (`int`) e `markedUsefulByMe` (`bool`) em `ReviewResponseDto`.
  - `replyToId` (`Guid?`) em `CommentResponseDto`.
- [x] `GET /api/reviews/place/{placeId:guid}?sort=useful|recent|rating_desc|rating_asc`:
  - Ignorar registros com `removed_at != null`.
- [x] `POST /api/reviews/{id:guid}/useful` e `DELETE /api/reviews/{id:guid}/useful`:
  - Marca/desmarca voto de utilidade; impedir que o próprio autor vote na sua avaliação (`409 Conflict`, `code: "review.cannot_vote_own"`).
- [x] Validar 1 nível de resposta em `POST /api/comments`: se `replyToId` apontar para um comentário que já tem `reply_to_id != null`, retornar `400 BadRequest` (`code: "comment.max_depth_exceeded"`).

### Endpoints de Administração (`/api/admin/reviews` e `/api/admin/removals`)
- [x] `GET /api/admin/reviews?signal=&rating=&placeId=&search=&page=`:
  - Lista avaliações de todos os locais com cálculo de sinais automáticos:
    - `outlier`: nota diverge ≥ 2.5 estrelas da média do local (em locais com ≥ 5 avaliações).
    - `burst`: ≥ 4 avaliações de contas criadas há menos de 7 dias no mesmo local em 24h.
    - `reported`: possui denúncias abertas em `tb_reports`.
- [x] `POST /api/admin/removals` (`AdminOnly`):
  - Recebe `{ targetType, targetId, reason, note }`, preenche `removed_at = UtcNow` na avaliação ou comentário alvo, grava o snapshot em `tb_content_removals` e encerra eventuais denúncias abertas daquele alvo na mesma transação.
- [x] `GET /api/admin/removals` (`AdminOnly`):
  - Histórico paginado de conteúdos removidos.

---

## ✅ Verificação

- [x] Avaliar duas vezes o mesmo local retorna `409 Conflict` (`code: "review.already_exists"`).
- [x] Votar "útil" duas vezes na mesma avaliação é idempotente; votar na própria avaliação retorna `409`.
- [x] Responder a uma resposta (`reply_to_id` de nível 2) retorna `400 BadRequest`.
- [x] Remover avaliação via `POST /api/admin/removals` oculta a avaliação da página do local, atualiza a média do ponto e registra o snapshot em `tb_content_removals`.
