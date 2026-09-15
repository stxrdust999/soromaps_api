# 📍 03. Lugar e catálogo

> **Status:** 💤 · **Depende de:** [01](./01-autenticacao.md), [02](./02-usuarios.md) · [Índice](./README.md)
>
> Decisão: [D4 — fotos por URL assinada](./decisoes.md#d4--fotos-por-url-assinada).
> Absorve a proposta `soromaps_web/docs/propostas/2026-08-03-expansao-modelo-ponto.md`.

Núcleo do produto: tudo o que vem depois (avaliação, visita, conquista, pauta)
pendura em `places`. Tira de uma vez do mock o mapa, `/discover`, `/places/*` e
`/admin/categories`.

---

## 🗄️ Banco

- [ ] `categories`: `id`, `name` UNIQUE, `slug` UNIQUE, `icon` (chave de `CATEGORY_ICONS`), `color` `char(7)`, `order` `smallint`, `active` (default `true`), `created_at`, `updated_at` — **`HasData`**
- [ ] `places` (substitui `markers`):

| Coluna | Nota |
|---|---|
| `id`, `name`, `lat`, `lng` | |
| `neighborhood` | obrigatório — eixo de feed, ranking, cobertura e alvo de conquista |
| `category_id` | FK `ON DELETE RESTRICT` |
| `author_id` | FK → `users`; quem sugeriu |
| `about` `varchar(160)` | chamada de uma linha (card e popup) |
| `description` `varchar(600)` | |
| `has_wifi`, `pet_friendly` | `boolean` default `false` |
| `best_time` `varchar(60)`, `local_secret` `varchar(200)` | opcionais |
| `status` | `CHECK IN ('pending','returned','approved','rejected')`, default `pending` |
| `created_at`, `updated_at` | |

  Índices: `status`, `category_id`, `neighborhood`, `(lat, lng)`.
  **Não são colunas:** nota média, total de avaliações e distância — são calculados.

- [ ] `place_photos`: `id`, `place_id`, `url`, `is_cover`, `uploaded_by`, `created_at`; índice único parcial `ON place_photos (place_id) WHERE is_cover`
- [ ] `tags`: `id`, `name` UNIQUE, `slug` UNIQUE — **`HasData`**
- [ ] `place_tags`: PK `(place_id, tag_id)`
- [ ] Seeder: pontos, fotos e tags de demonstração a partir de `src/mocks/markers.ts`

---

## 🔌 API

### Pontos

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `GET` | `/api/markers?bbox=&category=&tag=&page=` | anônimo | Só `approved`; filtro por bounding box (`minLng,minLat,maxLng,maxLat`) |
| `GET` | `/api/markers/{id}` | anônimo | Detalhe com categoria, fotos, tags, nota e total calculados; `404` se não aprovado (exceto autor/admin) |
| `POST` | `/api/markers` | autenticado | Nasce `pending`, `author_id` do token |
| `PUT` | `/api/markers/{id}` | admin ou autor | |
| `DELETE` | `/api/markers/{id}` | admin ou autor | |

- [ ] Rota em inglês `/api/markers` mantida (já consumida pelo front)

### Categorias e tags

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `GET` | `/api/categories` | anônimo | Ativas, por `order` |
| `GET` | `/api/admin/categories` | admin | Todas, com contagem de pontos e novos na semana (calculados) |
| `POST` / `PUT` | `/api/admin/categories[/{id}]` | admin | `slug` derivado do nome |
| `PATCH` | `/api/admin/categories/order` | admin | Reordena (1..n sem buraco) |
| `DELETE` | `/api/admin/categories/{id}?reassignTo=` | admin | `UPDATE places SET category_id = destino` + `DELETE` na mesma transação; sem `reassignTo` com pontos vinculados → `409` |
| `GET` | `/api/tags` | anônimo | |

### Fotos (D4)

1. [ ] `POST /api/markers/{id}/photos/upload-url` — confere permissão (autor ou admin), decide o caminho `places/{id}/{uuid}.webp`, devolve URL assinada de upload (~2 min)
2. [ ] Navegador redimensiona (~1600 px, WebP, sem EXIF) e faz `PUT` direto no Supabase Storage
3. [ ] `POST /api/markers/{id}/photos` — confere que o objeto existe, tamanho e tipo; insere em `place_photos`
- [ ] `DELETE /api/markers/{id}/photos/{photoId}` e `PATCH` para marcar capa
- [ ] Bucket **privado**; respostas trazem URL assinada de leitura
- [ ] Limpeza periódica de objetos sem linha em `place_photos` há mais de 24 h
- [ ] `Supabase__Url` e `Supabase__ServiceKey` só na API

---

## 🖥️ Front

- [ ] `/api/proxy/[...path]` (P0) e `src/hooks/use-markers.ts` passando o `bbox` do viewport
- [ ] `MarkerResource` ganha os campos novos; `src/validations/markers.ts` recebe o schema que hoje vive em `places/new/_components/create-marker-form.tsx`
- [ ] `src/actions/markers.ts` para de descartar os 7 campos
- [ ] `VIBE_OPTIONS` fixo substituído por categorias vindas da API
- [ ] `/places/[id]` sem `getMarkerDetailsMock`; gate de editar/excluir por admin ou autor
- [ ] `/admin/categories` sobre a API, com a exclusão com reatribuição
- [ ] Upload de foto no fluxo de criação

Mocks que saem: `src/mocks/markers.ts`, `src/mocks/admin-categories.ts`.

---

## ✅ Verificação

- [ ] Ponto recém-criado não aparece em `GET /api/markers` (está `pending`)
- [ ] `bbox` retorna só pontos dentro dos limites
- [ ] Excluir categoria com pontos sem `reassignTo`: `409`; com `reassignTo`: pontos migram e a categoria some
- [ ] Explorador editando ponto de outro: `403`
- [ ] Upload de 8 MB funciona (não passa pela Vercel); arquivo sem confirmação some após a limpeza
- [ ] URL da foto de ponto pendente não abre sem assinatura
