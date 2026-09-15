# 🧭 06. Atividade

> **Status:** 💤 · **Depende de:** [03](./03-lugar-e-catalogo.md) · [Índice](./README.md)

O que a pessoa faz na cidade: visitar e salvar lugares. Tira `/profile` inteiro
do mock e alimenta os motivos `perto`, `salvo` e `categoria` do feed, a régua
do selo de verificado e os critérios de conquista.

---

## 🗄️ Banco

- [ ] `visits`: PK `(user_id, place_id, visited_at)`, `source` `CHECK IN ('gps','manual')` default `manual`; índice `(user_id, visited_at)`
  - Data na PK porque visita é **evento repetível**: a timeline mostra o mesmo café três vezes de propósito
- [ ] `favorites`: PK `(user_id, place_id)`, `created_at`
  - Sem data na PK porque favoritar é **estado**: desfavoritar é `DELETE`
- [ ] Seeder: as 24 visitas e os favoritos de `src/mocks/profile.ts` (o progresso das conquistas precisa cair de pé sobre elas, como no mock)

---

## 🔌 API

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `POST` | `/api/markers/{id}/visits` | autenticado | `source=gps` exige `lat`/`lng` e a API confere a distância até o ponto; fora da tolerância → grava `manual` ou `400` (a decidir na implementação). Dispara evento `visit` |
| `GET` | `/api/me/visits?page=` | autenticado | Timeline agrupável por mês |
| `DELETE` | `/api/me/visits/{placeId}/{visitedAt}` | autenticado | Corrigir visita registrada por engano |
| `GET` | `/api/me/favorites` | autenticado | |
| `PUT` | `/api/me/favorites/{markerId}` | autenticado | Idempotente; dispara evento `favorite` |
| `DELETE` | `/api/me/favorites/{markerId}` | autenticado | |
| `GET` | `/api/me/stats` | autenticado | Lugares distintos, bairros cobertos sobre o total, categorias mais visitadas, sequência de dias |
| `GET` | `/api/me/verification` | autenticado | Contadores da régua: visitas, avaliações publicadas, avaliações removidas, meses de conta. **A régua em si continua em `src/constants/verification.ts`** |

- [ ] Distância por fórmula de haversine sobre `lat`/`lng` (PostGIS só se entrar busca por raio)

---

## 🖥️ Front

- [ ] `/profile` (visão geral, régua do selo), `/profile/visits`, `/profile/favorites`, `/profile/stats` sobre a API
- [ ] Botão de salvar lugar em `/places/[id]` e nos cards (`toggleFavoriteAction`)
- [ ] Check-in por GPS na página do ponto

Mock que sai: `src/mocks/profile.ts` (a parte de visitas e salvos; conquistas saem na [fase 07](./07-gamificacao.md)).

---

## ✅ Verificação

- [ ] Três visitas ao mesmo lugar em datas diferentes aparecem três vezes na timeline
- [ ] Favoritar duas vezes não duplica
- [ ] Check-in `gps` a 5 km do ponto não vira visita conferida
- [ ] `/api/me/verification` bate com a régua exibida em `/profile`
