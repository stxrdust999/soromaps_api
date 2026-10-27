# 📍 03. Catálogo, Categorias e Locais (Expansão)

> **Status:** ✅ Concluído · **Depende de:** Baseline (ADRs 0001–0003) · [Índice](./README.md)

As tabelas `tb_categories` e `tb_places` já existem na API com CRUD base, porém com schema enxuto (`tb_categories` só tem `id` e `name`; `tb_places` ainda não possui fotos, tags, campos de dica local, médias de avaliação nem filtros espaciais/textuais). Este módulo expande ambos os domínios para atender `/home`, `/discover`, `/places/new`, `/places/[id]` e `/admin/categories`.

---

## 🗄️ 1. Expansão de Categorias (`tb_categories`)

A tela `/admin/categories` e os pins do mapa precisam de identidade visual, ordenação e controle de ativação no banco.

### Alterações de Schema (`tb_categories`)
- [x] Adicionar colunas em `Category`:
  - `slug` (`varchar(60)`, **UNIQUE**, derivado automaticamente do `name` se omitido).
  - `icon` (`varchar(40)`, chave do ícone Lucide em `CATEGORY_ICONS`).
  - `color` (`varchar(7)`, cor hexadecimal `#RRGGBB`).
  - `sort_order` (`smallint`, ordem de exibição nos filtros).
  - `is_active` (`boolean`, default `true`).
  - `updated_at` (`timestamptz`).

### Endpoints e Regras de Categorias (`/api/categories`)
- [x] `GET /api/categories`:
  - Aceitar query `?includeInactive=true` (restrito a `Admin` quando `true`; para anônimos/exploradores, retornar apenas `is_active == true` ordenado por `sort_order`).
  - Enriquecer `CategoryResponseDto` com contadores calculados:
    - `placesCount`: total de locais aprovados na categoria.
    - `newPlacesThisWeekCount`: locais criados nos últimos 7 dias na categoria.
- [x] `PATCH /api/categories/order` (`AdminOnly`):
  - Recebe lista ordenada de `Guid`s e atualiza `sort_order` (`1..N`) em transação única.
- [x] `DELETE /api/categories/{id:guid}?reassignTo={targetCategoryId}` (`AdminOnly`):
  - Se existirem locais vinculados à categoria e `reassignTo` não for informado, retornar `409 Conflict` (`code: "category.in_use"`).
  - Se `reassignTo` for informado e válido, transferir todos os `tb_places` da categoria antiga para `reassignTo` e excluir a categoria na mesma transação.

---

## 🗄️ 2. Expansão de Locais (`tb_places`, Fotos e Tags)

### Alterações de Schema
- [x] Adicionar colunas opcionais em `tb_places` (`Place`):
  - `best_time` (`varchar(100)` null) — melhor horário de visita sugerido pelo autor.
  - `local_secret` (`varchar(300)` null) — dica/segredo local ("o que pedir / onde sentar").
- [x] Criar tabela `tb_place_photos` (`PlacePhoto`):
  - `id` (`uuid` PK, `gen_random_uuid()`)
  - `place_id` (`uuid` FK → `tb_places` `ON DELETE CASCADE`, indexado)
  - `uploaded_by_id` (`uuid` FK → `tb_users` `ON DELETE RESTRICT`)
  - `storage_path` (`varchar(500)` — caminho no bucket do Supabase Storage ou URL pública/assinada)
  - `is_cover` (`boolean`, default `false`)
  - `created_at` (`timestamptz`, default `now()`)
  - Índice único parcial garantindo no máximo uma capa por local (`WHERE is_cover = true`).
- [x] Criar tabelas `tb_tags` (`Tag`) e `tb_place_tags` (`PlaceTag`):
  - `tb_tags`: `id` (`uuid` PK), `name` (`varchar(50)` UNIQUE), `slug` (`varchar(60)` UNIQUE), `created_at` (`timestamptz`).
  - `tb_place_tags`: PK composta `(place_id, tag_id)`.

### Endpoints e Regras de Locais (`/api/places` e `/api/tags`)
- [x] Enriquecer `PlaceResponseDto` com dados agregados e relações:
  - `category`: resumo da categoria (`id`, `name`, `slug`, `icon`, `color`).
  - `author`: resumo do autor (`id`, `name`, `avatarUrl`).
  - `ratingAverage` (`double`): média calculada de `tb_reviews.rating` do local (`0` se sem avaliações).
  - `reviewsCount` (`int`): total de avaliações do local.
  - `coverPhotoUrl` (`string?`) e `photos` (`IReadOnlyList<PlacePhotoResponseDto>`).
  - `tags` (`IReadOnlyList<TagResponseDto>`).
  - `bestTime` e `localSecret`.
- [x] Expandir filtros de `GET /api/places`:
  - `bbox` (`string?` no formato `minLng,minLat,maxLng,maxLat`): filtra locais dentro do retângulo visível do mapa (`MapLibre`).
  - `categoryId` (`Guid?`) ou `categorySlug` (`string?`).
  - `tagSlug` (`string?`).
  - `neighborhood` (`string?`).
  - `search` (`string?`): busca textual em `name`, `about` e `neighborhood`.
  - `sort` (`"recent"` | `"rating"` | `"reviews"`).
- [x] `GET /api/places/{id:guid}`:
  - Se `status != Approved`, retornar `404 NotFound` para visitantes anônimos ou exploradores que **não** sejam o `author_id` do local (autor e `Admin` podem visualizar locais pendentes/devolvidos/rejeitados).
- [x] Upload e gestão de fotos (`/api/places/{id:guid}/photos`):
  - `POST /api/places/{id:guid}/photos/upload-url`: gera URL assinada de upload no Supabase Storage (`places/{placeId}/{photoId}.webp`).
  - `POST /api/places/{id:guid}/photos`: confirma upload e registra em `tb_place_photos`.
  - `PATCH /api/places/{id:guid}/photos/{photoId:guid}/cover`: define foto como capa.
  - `DELETE /api/places/{id:guid}/photos/{photoId:guid}`: remove foto (autor ou admin).
- [x] `GET /api/tags`: lista todas as tags disponíveis para os filtros de `/discover` e formulário de criação.

---

## ✅ Verificação

- [x] `GET /api/places?bbox=...` retorna apenas locais `Approved` cujas coordenadas estão dentro do bounding box.
- [x] `PlaceResponseDto` retorna `ratingAverage` e `reviewsCount` refletindo em tempo real as avaliações de `tb_reviews`.
- [x] Excluir categoria com locais vinculados sem `reassignTo` retorna `409 Conflict`; com `reassignTo` válido migra os locais e exclui a categoria.
- [x] Local com `status = pending` retorna `404` para terceiros, mas `200 OK` para o próprio autor e para administradores.
