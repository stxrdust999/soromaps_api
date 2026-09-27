# ADR-0001: Fundação da API e Módulo de Usuários

## Status

Accepted

## Data

2026-09-27

## Rodada / Módulo

00 Fundação & 02 Usuários

---

## Contexto

A versão anterior da API (`soromaps_api_OLD`) apresentava acúmulo de dívida técnica:
- Schemas manuais criados sem controle de migrations (`tbUsuario`, `markers`).
- Senhas em texto ou hashes expostos diretamente em payloads de resposta JSON.
- Ausência de Data Transfer Objects (DTOs) e validações fracas.
- Três formatos concorrentes de resposta de erro (texto puro, JSON cru e falhas 500 sem tratamento).
- Armazenamento acidental de segredos no repositório Git.

O projeto foi reiniciado do zero em ASP.NET Core (.NET 10) para estabelecer uma fundação profissional, tipada e com padrões arquiteturais consolidados antes do avanço para os demais módulos.

---

## Decisões Tomadas

### 1. Framework e Banco de Dados
- **ASP.NET Core 10 (`net10.0`)**: adoção do ecossistema moderno com C# 12/13, utilizando *Top-Level Statements* e *Primary Constructors*.
- **PostgreSQL no Supabase via Session Pooler**: conexão obrigatória na porta 5432 (IPv4) para evitar falhas de roteamento IPv6 na rede local e em nuvem.
- **Projetos Supabase Independentes (Dev e Prod)**: abandono da estratégia de múltiplos schemas manuais no mesmo projeto em favor de instâncias isoladas com schema padrão `public`.

### 2. Entity Framework Core e Mapeamento Relacional
- **Reset Limpo (Clean Slate)**: criação de migrations do zero via CLI (`dotnet-ef`), sem manter tabelas legadas.
- **Convenção de Nomes Automática (`EFCore.NamingConventions`)**:
  - Banco de dados: `snake_case` plural (`users`, `created_at`).
  - C#: `PascalCase` (`User`, `CreatedAt`).
  - JSON / API: `camelCase` nativo (`createdAt`, `avatarUrl`).
- **Fluent API Exclusiva (`Data/Configurations/`)**: configuração de constraints, tamanhos máximos e índices isolados em classes `IEntityTypeConfiguration<T>`, carregadas automaticamente via `ApplyConfigurationsFromAssembly`.
- **Chave Primária em `Guid` (UUID)**: substituição de inteiros autoincrementais por UUIDs gerados via `gen_random_uuid()` no PostgreSQL, prevenindo ataques de enumeração direta (IDOR).

### 3. Segurança e Gerenciamento de Configurações
- **User Secrets Nativo (`dotnet user-secrets`)**: segredos locais (como a connection string do Supabase) armazenados fora da pasta do repositório, eliminando riscos de vazamento no Git.
- **Hash de Senha com BCrypt.Net-Next**: utilização do algoritmo BCrypt com custo explícito (`workFactor: 12`) e limite estrito de 72 caracteres no input de senha.
- **DTOs Imutáveis com `record` (`DTOs/Users/`)**: separação rígida entre entidades de domínio e contratos de transporte (`RegisterUserDto`, `UpdateUserDto`, `UserResponseDto`). A entidade `User` e seu campo `PasswordHash` nunca são expostos diretamente.

### 4. Tratamento de Erros Padronizado (RFC 7807)
- Adoção de `AddProblemDetails()`, `UseExceptionHandler()` e `UseStatusCodePages()`.
- Respostas de conflito (ex.: e-mail já cadastrado) retornam `409 Conflict` com corpo ProblemDetails contendo a extensão semântica `"code": "user.email_taken"`, permitindo internacionalização desacoplada no frontend.

### 5. OpenAPI e Integração com o Frontend (Orval)
- Documentação de rotas e modelos no OpenAPI v3 via anotações `[ProducesResponseType]` e `[EndpointSummary]`, viabilizando a geração automatizada de contratos e hooks no Next.js (`soromaps_web`) através do Orval.

---

## Alternativas Consideradas e Descartadas

- **Abordagem com `.env` e biblioteca DotNetEnv:** descartada para evitar dependência externa desnecessária e manter o padrão idiomático da Microsoft (User Secrets em dev, Environment Variables em prod).
- **Mapeamento via Data Annotations nos Models:** descartada para manter as entidades de domínio limpas e sem poluição de atributos de banco.
- **Chaves Primárias Inteiras (`int`/`bigint`):** descartada devido à fragilidade de enumeração de dados e previsibilidade de URLs públicas.
- **JSON em `snake_case`:** descartada para respeitar o padrão idiomático da web moderna e do TypeScript no frontend Next.js.

---

## Consequências e Trade-offs

### Positivas (Ganhos)
- Base de código limpa, tipada e fortemente protegida contra SQL Injection e IDOR.
- Zero atrito no versionamento do banco via migrations idempotentes e automatizadas.
- Contratos HTTP previsíveis e estáveis para o frontend consumir via Orval.
- Ausência total de segredos no controle de versão.

### Negativas / Limitações (Custos aceitos)
- Identificadores UUID ocupam 16 bytes no índice em comparação a 4 bytes de inteiros.
- Necessidade de mapeamento explícito entre Entidade e DTO nas respostas.

---

## Referências
- [RFC 7807 — Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc7807)
- [Microsoft Docs — Entity Framework Core Naming Conventions](https://github.com/efcore/EFCore.NamingConventions)
- [Microsoft Docs — Safe storage of app secrets in development in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
