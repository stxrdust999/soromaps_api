# ⚙️ Soromaps API

> API REST em ASP.NET Core 10 (`net10.0`) que serve a plataforma [Soromaps](https://github.com/stxrdust999/soromaps) — ambiente interativo para descobrir, avaliar e compartilhar experiências em estabelecimentos locais de Sorocaba com geolocalização e gamificação.

---

## 🧱 Stack Tecnológica

| Camada | Tecnologia | Detalhes |
|---|---|---|
| **Framework** | ASP.NET Core 10 (`net10.0`) | Minimal Hosting, Top-Level Statements, C# 12/13 |
| **ORM** | Entity Framework Core 10 | Code-First, Migrations versionadas com RLS, Fluent API isolada |
| **Nomenclatura** | `EFCore.NamingConventions` | Prefixo `tb_` + `snake_case` no Postgres, `PascalCase` no C#, `camelCase` no JSON |
| **Banco de Dados** | PostgreSQL (Supabase) | Schema `public`, conexão via **Session Pooler** (porta 5432, IPv4) |
| **Storage** | Supabase Storage | Buckets `soromaps_places` (locais) e `soromaps_pfps` (avatares) com URLs assinadas |
| **Identificadores** | `Guid` (UUID v4) | Chaves geradas com `gen_random_uuid()` no PostgreSQL |
| **Criptografia** | BCrypt.Net-Next | Custo explícito (`workFactor: 12`) e limite estrito de 72 bytes |
| **Contrato de Erros** | ProblemDetails (RFC 7807) | Erros estruturados com extensão semântica `code` para o frontend |
| **Documentação & Tipagem** | OpenAPI v3 | Schemas tipados em `/openapi/v1.json` para geração de clientes via **Orval** |

---

## 🚀 Como Rodar Localmente

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Ferramenta de linha de comando `dotnet-ef` instalada localmente no repositório (`dotnet tool restore`).

### 1. Clonar e Restaurar
```bash
git clone https://github.com/stxrdust999/soromaps_api.git
cd soromaps_api
dotnet restore
dotnet tool restore
```

### 2. Configurar Segredos de Desenvolvimento (User Secrets)
A API utiliza o **User Secrets** nativo do .NET para desenvolvimento local (e **Application Settings / Variáveis de Ambiente** em produção no Azure). Dessa forma, credenciais e senhas **nunca** são versionadas no Git.

Execute os comandos abaixo na raiz do projeto preenchendo com os seus valores:

```bash
# 1. Inicializar o cofre de segredos local (caso ainda não esteja inicializado)
dotnet user-secrets init

# 2. String de conexão do PostgreSQL (Supabase Session Pooler - porta 5432)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=aws-0-sa-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<db-password>;SSL Mode=Require;Trust Server Certificate=true"

# 3. Chave de assinatura do JWT (mínimo de 32 caracteres aleatórios)
dotnet user-secrets set "Jwt:Key" "<sua-chave-secreta-jwt-com-no-minimo-32-caracteres>"

# 4. Credenciais do primeiro Administrador (usado pelo comando --seed)
dotnet user-secrets set "Seed:AdminEmail" "admin@soromaps.com"
dotnet user-secrets set "Seed:AdminPassword" "<senha-forte-do-admin>"
dotnet user-secrets set "Seed:AdminName" "Administrador Soromaps"

# 5. Configuração do Supabase Storage (Project Settings -> API no painel do Supabase)
dotnet user-secrets set "Supabase:Url" "https://<project-ref>.supabase.co"
dotnet user-secrets set "Supabase:ServiceRoleKey" "<sua-service-role-secret-key>"
dotnet user-secrets set "Supabase:PlacesBucket" "soromaps_places"
dotnet user-secrets set "Supabase:AvatarsBucket" "soromaps_pfps"
```

#### Tabela de Referência de Variáveis de Configuração

| Chave (User Secrets) | Variável de Ambiente (Azure / Docker) | Obrigatória? | Descrição |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | Sim | Conexão PostgreSQL (Session Pooler do Supabase) |
| `Jwt:Key` | `Jwt__Key` | Sim | Segredo HMAC-SHA256 para assinar tokens de acesso (≥ 32 chars) |
| `Jwt:Issuer` | `Jwt__Issuer` | Não | Emissor do token (padrão: `soromaps_api`) |
| `Jwt:Audience` | `Jwt__Audience` | Não | Audiência do token (padrão: `soromaps_web`) |
| `Seed:AdminEmail` | `Seed__AdminEmail` | Para `--seed` | E-mail do primeiro administrador criado no seed |
| `Seed:AdminPassword` | `Seed__AdminPassword` | Para `--seed` | Senha inicial do primeiro administrador |
| `Seed:AdminName` | `Seed__AdminName` | Não | Nome de exibição do primeiro admin |
| `Supabase:Url` | `Supabase__Url` | Para upload | URL base do projeto no Supabase (`https://<ref>.supabase.co`) |
| `Supabase:ServiceRoleKey` | `Supabase__ServiceRoleKey` | Para upload | Chave `service_role` (`secret`) do Supabase para assinar URLs de upload |
| `Supabase:PlacesBucket` | `Supabase__PlacesBucket` | Não | Nome do bucket de fotos de locais (padrão: `soromaps_places`) |
| `Supabase:AvatarsBucket` | `Supabase__AvatarsBucket` | Não | Nome do bucket de fotos de perfil (padrão: `soromaps_pfps`) |

### 3. Aplicar Migrations e Popular Dados Iniciais (Seed)
```bash
# Aplicar as migrations no banco configurado
dotnet ef database update

# Popular o primeiro administrador e o catálogo inicial (idempotente)
dotnet run -- --seed
```

### 4. Executar a API
```bash
dotnet run
```
A API iniciará por padrão em:
- HTTP: `http://localhost:5210`
- HTTPS: `https://localhost:7104`
- OpenAPI Schema: `http://localhost:5210/openapi/v1.json`

---

## 📚 Documentação Técnica e Decisões

Toda a documentação viva e modelagens visuais estão disponíveis na pasta [`Docs/`](./Docs):

- **[Architecture Decision Records (ADRs)](./Docs/ArchitectureDecisionRecords/README.md):** Histórico de decisões técnicas imutáveis tomadas a cada rodada de desenvolvimento.
- **[Diagramas do Sistema](./Docs/Diagrams/README.md):**
  - [Diagramas de Classes](./Docs/Diagrams/ClassDiagrams/README.md) (Mermaid)
  - [Diagramas de Casos de Uso](./Docs/Diagrams/UseCaseDiagrams/README.md) (Mermaid)
  - [Diagramas Entidade-Relacionamento](./Docs/Diagrams/EntityRelationship/README.md) (Mermaid)
  - [Modelagem DBML](./Docs/Diagrams/Dbml/README.md) (Pronta para o [dbdiagram.io](https://dbdiagram.io))
- **[Roadmap da API (TODOs)](./Docs/Todo/Api/README.md):** Fases de entrega e integração com o frontend.

---

## 📝 Convenções

- **Commits:** [Conventional Commits](https://www.conventionalcommits.org/)
- **Código:** C# idiomático em inglês, com documentação e ADRs em português.
