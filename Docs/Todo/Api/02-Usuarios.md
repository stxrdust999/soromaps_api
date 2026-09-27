# 👤 02. Usuários

> **Status:** 🟡 Parcial (CRUD base, DTOs e validação de e-mail entregues) · **Depende de:** [00](./00-fundacao.md) · Anda junto com [01](./01-autenticacao.md) · [Índice](./README.md)

Substitui `tbUsuario` (descartada, [D2](./decisoes.md#d2--reset-limpo-do-banco))
por `users`, com papel e perfil. É o que destrava o gate de `/admin`, a edição
parcial de dados e, adiante, autoria de ponto e contribuição.

---

## 🗄️ Banco

- [x] Tabela `users`:

| Coluna | Tipo | Nota |
|---|---|---|
| `id` | `uuid` PK | `Guid` no C#, gerado com `gen_random_uuid()` |
| `name` | `varchar(100)` | Nome de exibição |
| `email` | `varchar(255)` **UNIQUE** | |
| `password_hash` | `varchar` | BCrypt custo 12 |
| `role` | `varchar(20)` | default `explorer` |
| `avatar_url` | `varchar(500)` null | |
| `biography` | `varchar(280)` null | |
| `neighborhood` | `varchar(60)` null | bairro declarado — ranking por bairro e motivo `perto` do feed |
| `created_at` / `updated_at` | `timestamptz` | |

- [x] Unicidade de `email` via `builder.HasIndex(u => u.Email).IsUnique()`
- [ ] Seeder: usuários de demonstração a partir de `src/mocks/community.ts`

---

## 🔌 API

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `POST` | `/api/users` | anônimo | Cadastro; sempre `role = explorer`; `409` em `email` duplicado com `code: user.email_taken` |
| `GET` | `/api/users` | anônimo/admin | Listagem segura com `UserResponseDto` |
| `GET` | `/api/users/{id}` | anônimo | Busca usuário por ID |
| `PATCH` | `/api/users/{id}` | autenticado | Atualização parcial de perfil |
| `DELETE` | `/api/users/{id}` | admin | Remove usuário por ID |

- [x] DTO de saída `UserResponseDto` sem `password_hash`
- [x] DTOs de entrada `RegisterUserDto` e `UpdateUserDto` com anotações de validação
- [ ] `PATCH` só re-hasheia quando a senha vier (hoje o `PUT` re-hasheia sempre, e por isso o formulário de edição exige senha)
- [ ] Admin não pode rebaixar o próprio papel se for o último admin (`409`)

---

## 🖥️ Front

- [ ] `src/http/users/users.ts` e `src/actions/users.ts` apontam para `/api/admin/users`
- [ ] Formulário de edição em `/admin/users` deixa de exigir senha
- [ ] Coluna/filtro de papel em `/admin/users`
- [ ] `/settings` passa a ter destino (rota e item de navegação ainda não existem — ver `soromaps_web/docs/todo/user/settings.md`)
- [ ] Perfil (`/profile`) lê `avatar_url`, `bio`, `neighborhood` reais

---

## ✅ Verificação

- [ ] Cadastro com e-mail já usado (qualquer caixa): `409` com `code: user.email_taken`
- [ ] `GET /api/admin/users` nunca contém `password_hash`
- [ ] `PATCH /api/users/me` sem senha mantém o hash anterior
- [ ] Rebaixar o último admin: `409`
