# 🤖 CLAUDE.md — Soromaps API

> Registro vivo deste repositório (backend). O registro do projeto como um todo — produto, decisões compartilhadas, roadmap — vive em [`soromaps_web/CLAUDE.md`](https://github.com/stxrdust999/soromaps/blob/master/CLAUDE.md). Este arquivo cobre só o que é específico da API.
> Última atualização: 2026-08-16

---

## 🎯 Contexto

TCC (FATEC Sorocaba). Este repo é a API REST que serve o frontend web
(`soromaps_web`) e, futuramente, o app mobile em Expo. Foi separado do repo
web no commit inicial (`a210a31`) para deploy independente e histórico limpo
por responsabilidade.

---

## 🧱 Stack

| Camada | Tecnologia |
|---|---|
| Framework | ASP.NET Core 10 (`net10.0`) |
| ORM | EF Core + `Npgsql.EntityFrameworkCore.PostgreSQL` |
| Banco de Dados | PostgreSQL — **Supabase** em produção |
| Nuvem | **Azure App Service** (deploy manual, sem pipeline) |
| Hash de senha | BCrypt.Net-Next |

---

## 📐 Convenções

- Código em inglês, comentários em português
- Commits: Conventional Commits
- Controllers falam direto com `AppDbContext` — sem camada de serviço/repositório por enquanto (CRUD puro sobre 2 entidades; revisitar quando entrar regra de negócio real)

---

## 🗄️ Estado do banco

Duas tabelas, sem relacionamento entre elas e **sem migrations** — o schema foi criado à mão no SQL Editor do Supabase, e existe uma cópia igualmente manual no ambiente local:

| Tabela | Colunas | Model |
|---|---|---|
| `tbUsuario` | `id`, `user_name`, `user_email`, `user_password`, `created_at`, `updated_at` | `Models/User.cs` |
| `markers` | `id`, `nome`, `lat`, `lng` | `Models/Marker.cs` |

Modelo completo de 10 tabelas (projetado no TCC) e o gap contra o estado atual: [wiki 08](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/08-banco-atual.md) e [wiki 12](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/12-gap-modelo-vs-implementacao.md).

### Conexão: pooler, nunca o host direto (2026-08-16)

A `DefaultConnection` aponta para o **session pooler** do Supabase:

```
Host=aws-1-sa-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<ref>;Password=<senha>;SSL Mode=Require;Trust Server Certificate=true
```

O host direto (`db.<ref>.supabase.co`) é **IPv6-only** desde jan/2024 — resolve
só AAAA. Rede sem rota IPv6 não chega nele e a conexão pendura até o timeout,
sem erro que aponte a causa. Foi o que fazia a API conectar em rede móvel e
nunca em Wi-Fi. A saída do Azure App Service também é IPv4-only, então o pooler
vale nos dois ambientes.

Três coisas que quebram se passarem batido:

- `Username` é `postgres.<project-ref>`, **não** `postgres`
- o prefixo `aws-0-` / `aws-1-` varia por projeto — copiar do painel em
  **Connect → Session pooler**
- errar qualquer um dos dois devolve `XX000: (ENOTFOUND) tenant/user ... not
  found`, que parece erro de credencial mas é roteamento de tenant

**Session mode (5432), não transaction (6543):** session é proxy transparente e
o EF Core não muda em nada. Transaction descarta estado de sessão entre
comandos (prepared statement, `SET`, tabela temporária, `LISTEN/NOTIFY`) e
exigiria `Max Auto Prepare=0` + `No Reset On Close=true`. O Npgsql já mantém
pool próprio no cliente e a API roda em instância única, então o ganho de
escala do transaction não se aplica.

**Produção:** a mesma string precisa estar nas Application Settings do App
Service (`ConnectionStrings__DefaultConnection`).

---

## 🔌 Endpoints

`AuthController`, `UsersController`, `MarkersController`. Catálogo completo com payloads: [wiki 09](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/09-api-endpoints.md).

`WeatherForecastController` é resto do template `dotnet new webapi` — candidato a remoção.

---

## 🔴 Pendências de segurança (a API JÁ ESTÁ publicada)

> ⚠️ Esta lista era "não fazer deploy sem isso". O deploy no Azure aconteceu
> mesmo assim, então os itens abaixo deixaram de ser preventivos e passaram a
> ser dívida ativa em produção.

- [ ] Registrar autenticação (`AddAuthentication` + `[Authorize]`) — **hoje todo endpoint é público**, o cookie de sessão do Next.js não protege a API
- [ ] Parar de devolver `user_password` (hash) nas respostas de `/api/users` — criar DTO de saída
- [ ] `UNIQUE` em `user_name` e `user_email`
- [ ] Resposta genérica de erro no login (hoje diferencia "usuário não encontrado" de "senha incorreta")
- [ ] Papel/role de administrador, checado na API (hoje `/admin` no front não tem correspondente de autorização aqui)

Detalhe de cada risco: [wiki 10](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/10-autenticacao-e-sessao.md).

---

## 🟠 Pendências de fundação

- [ ] **CORS**: origem configurável — hoje `http://localhost:3000` fixo em `Program.cs`, enquanto o front está na Vercel. Isso quebra as chamadas do mapa feitas pelo navegador ([wiki 14](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/14-deploy.md))
- [ ] EF Core Migrations — o schema é mantido à mão em dois lugares (local e Supabase), sem nada que os compare
- [ ] FK ligando `markers` ao usuário criador
- [ ] Padronizar nomenclatura de tabelas/colunas (`tbUsuario` × `markers`)
- [ ] `PUT /api/users/{id}` aceitar atualização parcial (senha opcional, ou um `PATCH`) — hoje re-hasheia sempre
- [ ] Tirar `bin/` e `obj/` do controle de versão (`git rm -r --cached`); o `.gitignore` já existe
- [ ] Pipeline de deploy (hoje é publish manual pelo Visual Studio)

---

## 🟡 Pendências de produto

Entidades ainda sem tabela: `Categoria`, `Analise`, `Comentario`, `Favorita`, `Visita`, `Segue`, `Conquista`, `GanhaConquista`. Prioridade e requisito atendido por cada uma: [wiki 12](https://github.com/stxrdust999/soromaps/blob/master/docs/wiki/12-gap-modelo-vs-implementacao.md).

---

## 🟢 Limpeza

- [ ] Apagar `WeatherForecastController`
