# ⚙️ Soromaps API

> API REST em ASP.NET Core 10 (`net10.0`) que serve a plataforma [Soromaps](https://github.com/stxrdust999/soromaps) — ambiente interativo para descobrir, avaliar e compartilhar experiências em estabelecimentos locais de Sorocaba com geolocalização e gamificação.

---

## 🧱 Stack Tecnológica

| Camada | Tecnologia | Detalhes |
|---|---|---|
| **Framework** | ASP.NET Core 10 (`net10.0`) | Minimal Hosting, Top-Level Statements, C# 12/13 |
| **ORM** | Entity Framework Core 10 | Code-First, Migrations versionadas, Fluent API isolada |
| **Nomenclatura** | `EFCore.NamingConventions` | `snake_case` no Postgres, `PascalCase` no C#, `camelCase` no JSON |
| **Banco de Dados** | PostgreSQL (Supabase) | Schema `public`, conexão via **Session Pooler** (porta 5432, IPv4) |
| **Identificadores** | `Guid` (UUID v4) | Chaves geradas com `gen_random_uuid()` no PostgreSQL |
| **Criptografia** | BCrypt.Net-Next | Custo explícito (`workFactor: 12`) e limite estrito de 72 bytes |
| **Contrato de Erros** | ProblemDetails (RFC 7807) | Erros estruturados com extensão semântica `code` para o frontend |
| **Documentação & Tipagem** | OpenAPI v3 + Swagger | Schemas tipados para geração de clientes e hooks via **Orval** |

---

## 📁 Arquitetura do Projeto

O projeto segue padrões consolidados de arquitetura em .NET:

```text
soromaps_api/
├── Controllers/              # Controladores REST da API
│   └── UsersController.cs    # Endpoints do recurso de usuários
├── Data/                     # Camada de Acesso a Dados (EF Core)
│   ├── AppDbContext.cs       # Contexto do banco e registro automático de mapeamentos
│   └── Configurations/       # Mapeamento Fluent API isolado por entidade
│       └── UserConfiguration.cs
├── DTOs/                     # Data Transfer Objects (Contratos de entrada/saída com record)
│   └── Users/
│       ├── RegisterUserDto.cs
│       ├── UpdateUserDto.cs
│       └── UserResponseDto.cs
├── Models/                   # Entidades de Domínio puras (POCO)
│   └── User.cs
├── Migrations/               # Histórico versionado de migrations do banco
└── Docs/                     # Documentação viva do sistema
    ├── ArchitectureDecisionRecords/  # ADRs congelados por rodada de desenvolvimento
    ├── Diagrams/             # Diagramas (Classes, Casos de Uso, DER e DBML)
    └── Todo/Api/             # Roadmap de fases e decisões da API
```

---

## 🚀 Como Rodar Localmente

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Ferramenta de linha de comando `dotnet-ef` instalada localmente no repositório.

### 1. Clonar e Restaurar
```bash
git clone https://github.com/stxrdust999/soromaps_api.git
cd soromaps_api
dotnet restore
```

### 2. Configurar Segredos de Desenvolvimento (User Secrets)
A API utiliza o **User Secrets** nativo do .NET para desenvolvimento local. Dessa forma, credenciais e senhas **nunca** são versionadas no Git:

```bash
# Inicializar o cofre de segredos local (se necessário)
dotnet user-secrets init

# Configurar a string de conexão do Supabase Session Pooler
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=aws-0-sa-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<ref>;Password=<senha>;SSL Mode=Require;Trust Server Certificate=true"
```

### 3. Aplicar Migrations no Banco
```bash
dotnet ef database update
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
