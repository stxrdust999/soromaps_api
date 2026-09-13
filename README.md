# ⚙️ Soromaps API

> API REST em ASP.NET Core que serve o [Soromaps](https://github.com/stxrdust999/soromaps) — plataforma para descobrir, avaliar e compartilhar experiências em estabelecimentos locais de Sorocaba. Este repositório é o backend; o frontend web vive em [`soromaps_web`](https://github.com/stxrdust999/soromaps).

---

## 🧱 Stack

| Camada | Tecnologia |
|---|---|
| Framework | ASP.NET Core 10 (`net10.0`) |
| ORM | Entity Framework Core + Npgsql |
| Banco de Dados | PostgreSQL (**Supabase** em produção) |
| Hash de senha | BCrypt.Net-Next |

---

## 🚀 Como rodar

Precisa do frontend web rodando em paralelo — instruções completas em [`soromaps_web`](https://github.com/stxrdust999/soromaps).

```bash
dotnet restore
dotnet run
```

Sobe em `http://localhost:5068` (perfil `http`) ou `https://localhost:7240` (perfil `https`), conforme `Properties/launchSettings.json`.

### Configuração

A connection string vem de `ConnectionStrings:DefaultConnection`, vazia por padrão em `appsettings.json`. Preencha em `appsettings.Development.json` (ignorado pelo git):

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=soromaps;Username=postgres;Password=SUA_SENHA"
  }
}
```

> ⚠️ **Sem migrations.** O schema é criado manualmente — DDL das tabelas e passo a passo completo em [Ambiente e setup](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/13-ambiente-e-setup.md), na wiki do repo web.

### CORS

`Program.cs` libera apenas `http://localhost:3000`. Rodar o frontend em outra origem exige ajustar a policy `AllowFrontend`.

---

## ☁️ Produção

Publicada em **Azure App Service**, com banco **PostgreSQL no Supabase**. O deploy é manual (publish pelo Visual Studio / Azure CLI) — não há pipeline neste repositório.

A connection string real vive nas Application Settings do App Service, na chave `ConnectionStrings__DefaultConnection` (o duplo underscore é a convenção do .NET para aninhamento em variável de ambiente). O `appsettings.json` versionado mantém o valor **vazio** de propósito.

> 🔴 **Dois pontos abertos que afetam produção:**
>
> 1. **CORS** ainda fixo em `http://localhost:3000`, enquanto o front está na Vercel — as chamadas do mapa feitas pelo navegador estão quebradas. Ver [Deploy](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/14-deploy.md).
> 2. **Nenhum endpoint exige autenticação**, e a API agora está exposta na internet. É o item mais urgente do backlog — ver [Autenticação e sessão](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/10-autenticacao-e-sessao.md).

---

## 📖 Documentação

Toda a documentação do projeto — arquitetura, modelagem de dados, endpoints, autenticação — vive centralizada no repositório web:

👉 **[Wiki do Soromaps](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/00-home.md)**

Páginas mais relevantes para este repo:

| Página | Conteúdo |
|---|---|
| [08 — Banco atual](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/08-banco-atual.md) | Tabelas reais, mapeamento EF Core, ausência de migrations |
| [09 — Endpoints da API](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/09-api-endpoints.md) | Catálogo completo de rotas, payloads e respostas |
| [10 — Autenticação e sessão](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/10-autenticacao-e-sessao.md) | Fluxo de login e riscos de segurança conhecidos |
| [12 — Gap modelo × implementação](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/12-gap-modelo-vs-implementacao.md) | Backlog priorizado, incluindo os itens de segurança da API |

---

## 📝 Convenções

- Commits: [Conventional Commits](https://www.conventionalcommits.org/)
- Código em inglês, comentários em português
