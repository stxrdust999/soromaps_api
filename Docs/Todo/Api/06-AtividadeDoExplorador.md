# 🧭 06. Atividade do Explorador (Visitas, Favoritos, Estatísticas e Selo)

> **Status:** 💤 Pendente · **Depende de:** [03-CatalogoELocais.md](./03-CatalogoELocais.md), [05-AvaliacoesEComentarios.md](./05-AvaliacoesEComentarios.md) · [Índice](./README.md)

Sustenta as abas de `/profile` (`/profile/visits`, `/profile/favorites`, `/profile/stats`), os botões de check-in e favoritar em `/places/[id]` e a régua pública do selo de **Explorador Verificado**.

---

## 🗄️ Banco de Dados

### 1. Visitas / Check-ins (`tb_visits`)
- [ ] Criar tabela `tb_visits` (`Visit`):
  - `id` (`uuid` PK, `gen_random_uuid()`)
  - `user_id` (`uuid` FK → `tb_users` `ON DELETE CASCADE`, indexado)
  - `place_id` (`uuid` FK → `tb_places` `ON DELETE CASCADE`, indexado)
  - `visited_on` (`date` — data UTC da visita)
  - `created_at` (`timestamptz`, default `now()`)
  - Constraint `UNIQUE (user_id, place_id, visited_on)`: permite no máximo 1 check-in por local por dia para o mesmo usuário.

### 2. Favoritos (`tb_favorites`)
- [ ] Criar tabela `tb_favorites` (`Favorite`):
  - PK composta `(user_id, place_id)` (FKs para `tb_users` e `tb_places` com `ON DELETE CASCADE`).
  - `created_at` (`timestamptz`, default `now()`).

---

## 🔌 API

### Visitas (`/api/visits`)
| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/api/places/{placeId:guid}/visits` | Autenticado | Registra check-in no local validando a distância GPS (fórmula de Haversine ≤ 200 metros entre `latitude`/`longitude` enviadas no body e as coordenadas do local). |
| `GET` | `/api/users/me/visits` | Autenticado | Lista o histórico de visitas do usuário logado com dados do local e indicação se já avaliou o ponto. |

- [ ] **Erros de Check-in:**
  - Distância > 200m → `422 UnprocessableEntity` (`code: "visit.too_far"`).
  - Check-in já realizado hoje no mesmo local → `409 Conflict` (`code: "visit.already_checked_in_today"`).

### Favoritos (`/api/favorites`)
| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/api/places/{placeId:guid}/favorite` | Autenticado | Adiciona local aos favoritos (idempotente). |
| `DELETE` | `/api/places/{placeId:guid}/favorite` | Autenticado | Remove local dos favoritos. |
| `GET` | `/api/users/me/favorites` | Autenticado | Lista locais favoritados pelo usuário logado (filtrável por `categoryId` e `neighborhood`). |

### Estatísticas e Verificação (`/api/users/me/stats` e `/api/users/me/verification`)
| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `GET` | `/api/users/me/stats` | Autenticado | Retorna agregados para `/profile/stats`: visitas por mês (últimos 6 meses), distribuição por categoria, cobertura de bairros de Sorocaba (visitados vs. total de bairros com locais aprovados) e distribuição de notas dadas (`1..5`). |
| `GET` | `/api/users/me/verification` | Autenticado | Retorna os 4 contadores objetivos do selo de Explorador Verificado (`visitsCount`, `publishedReviewsCount`, `removedReviewsCount`, `accountAgeDays`) e o booleano `isVerified`. |

---

## ✅ Verificação

- [ ] Check-in a 50 metros do local retorna `201 Created`; a 500 metros retorna `422` com `code: "visit.too_far"`.
- [ ] Segundo check-in no mesmo local e mesmo dia retorna `409` com `code: "visit.already_checked_in_today"`.
- [ ] `GET /api/users/me/verification` retorna `isVerified = true` somente quando `visitsCount >= 5`, `publishedReviewsCount >= 3`, `removedReviewsCount == 0` e `accountAgeDays >= 30`.
