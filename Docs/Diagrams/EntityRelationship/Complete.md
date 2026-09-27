# Diagrama Entidade-Relacionamento (DER) — Modelo Completo

Modelo lógico e relacional do banco de dados PostgreSQL (`public`) do Soromaps, desenhado com nomenclatura `snake_case` e integridade referencial.

```mermaid
erDiagram
    users ||--o{ sessions : "possui"
    users ||--o{ places : "sugere / cadastra"
    users ||--o{ reviews : "escreve"
    users ||--o{ visits : "registra"
    users ||--o{ favorites : "salva"
    users ||--o{ user_achievements : "conquista"

    categories ||--o{ places : "classifica"
    places ||--o{ place_photos : "contém"
    places ||--o{ place_tags : "possui"
    tags ||--o{ place_tags : "associa"
    places ||--o{ reviews : "recebe"
    places ||--o{ visits : "registra"
    places ||--o{ favorites : "recebe"

    achievements ||--o{ user_achievements : "atribui"

    users {
        uuid id PK
        varchar(100) name
        varchar(255) email UK
        varchar password_hash
        varchar(20) role
        varchar(500) avatar_url
        varchar(280) biography
        varchar(60) neighborhood
        timestamptz created_at
        timestamptz updated_at
    }

    sessions {
        bigint id PK
        uuid user_id FK
        char(64) token_hash UK
        timestamptz expires_at
        timestamptz revoked_at
        varchar(300) user_agent
        timestamptz created_at
    }

    categories {
        int id PK
        varchar(60) name UK
        varchar(60) slug UK
        varchar(40) icon
        varchar(7) color
        smallint order
        boolean active
        timestamptz created_at
        timestamptz updated_at
    }

    places {
        uuid id PK
        varchar(120) name
        double_precision latitude
        double_precision longitude
        varchar(160) about
        text description
        varchar(60) neighborhood
        int category_id FK
        uuid author_id FK
        boolean has_wifi
        boolean pet_friendly
        varchar(60) best_time
        varchar(200) local_secret
        varchar(20) status
        timestamptz created_at
        timestamptz updated_at
    }

    place_photos {
        uuid id PK
        uuid place_id FK
        varchar(500) url
        boolean is_cover
        uuid uploaded_by FK
        timestamptz created_at
    }

    tags {
        int id PK
        varchar(40) name UK
        varchar(40) slug UK
    }

    place_tags {
        uuid place_id PK,FK
        int tag_id PK,FK
    }

    reviews {
        uuid id PK
        uuid place_id FK
        uuid user_id FK
        smallint rating
        text comment
        timestamptz created_at
        timestamptz updated_at
    }

    visits {
        uuid id PK
        uuid place_id FK
        uuid user_id FK
        timestamptz visited_at
    }

    favorites {
        uuid place_id PK,FK
        uuid user_id PK,FK
        timestamptz created_at
    }

    achievements {
        int id PK
        varchar(80) title UK
        varchar(80) slug UK
        varchar(200) description
        varchar(40) icon
        int points
    }

    user_achievements {
        uuid user_id PK,FK
        int achievement_id PK,FK
        timestamptz unlocked_at
    }
```
