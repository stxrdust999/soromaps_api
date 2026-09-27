# Diagrama Entidade-Relacionamento (DER) — Módulo de Usuários

Este diagrama descreve a tabela `users` no PostgreSQL do Supabase, incluindo tipos de dados nativos, constraints e índices criados via Fluent API no EF Core.

```mermaid
erDiagram
    users {
        uuid id PK "default: gen_random_uuid()"
        varchar(100) name "not null"
        varchar(255) email "not null, UNIQUE"
        varchar password_hash "not null (BCrypt)"
        varchar(20) role "not null, default: explorer"
        varchar(500) avatar_url "nullable"
        varchar(280) biography "nullable"
        varchar(60) neighborhood "nullable"
        timestamptz created_at "not null, default: now()"
        timestamptz updated_at "not null, default: now()"
    }

    sessions {
        bigint id PK
        uuid user_id FK "ref: users.id"
        char(64) token_hash "UNIQUE, SHA-256"
        timestamptz expires_at
        timestamptz revoked_at
        varchar(300) user_agent
        timestamptz created_at
    }

    users ||--o{ sessions : "possui sessões ativas"
```
