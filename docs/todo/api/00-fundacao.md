# 🧱 00. Fundação

> **Status:** 💤 · **Bloqueia:** todas as outras fases · [Índice](./README.md)
>
> Decisões: [D2](./decisoes.md#d2--reset-limpo-do-banco),
> [D3](./decisoes.md#d3--nomes-inglês-snake_case-no-banco-pascalcase-no-c-camelcase-no-json),
> [D5](./decisoes.md#d5--dois-schemas-no-mesmo-projeto-supabase),
> [D6](./decisoes.md#d6--só-supabase-sem-postgres-local),
> [D7](./decisoes.md#d7--seed-a-partir-dos-mocks-do-front),
> [D8](./decisoes.md#d8--primeiro-admin-via-configuração),
> [D9](./decisoes.md#d9--erro-em-problemdetails-com-code-texto-no-front),
> [D11](./decisoes.md#d11--sem-cors),
> [D12](./decisoes.md#d12--tabelas-no-plural),
> [D13](./decisoes.md#d13--configuração-via-dotnetenv)

Nenhuma tabela de produto nasce aqui. A fase deixa o terreno pronto para as
próximas: o ORM passa a ser a única fonte da estrutura do banco, segredo sai do
alcance do git e toda resposta de erro tem o mesmo formato.

---

## 🗄️ Migrations e schema

- [ ] Pacotes `Microsoft.EntityFrameworkCore.Design` e `EFCore.NamingConventions`
- [ ] `dotnet-ef` fixado em tool manifest (`dotnet new tool-manifest` + `dotnet tool install dotnet-ef`), para todo dev usar a mesma versão
- [ ] `UseSnakeCaseNamingConvention()` no `AddDbContext`
- [ ] `HasDefaultSchema` lendo `Database:Schema`, e a tabela de histórico no mesmo schema:
  ```csharp
  options.UseNpgsql(connectionString, npgsql =>
      npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema));
  ```
- [ ] Descartar `tbUsuario` e `markers` (D2); remover os `[Table]`/`[Column]` de `Models/User.cs` e `Models/Marker.cs`
- [ ] **Sem `Migrate()` automático no startup:** o App Service reinicia sozinho, e migration em produção precisa ser ação deliberada

### Regras de ciclo de vida

| Momento | Regra |
|---|---|
| Sempre | **Uma migration por fase**, só com as tabelas cujos endpoints estão sendo feitos. O modelo muda ao implementar, e tabela sem endpoint é peso morto |
| Enquanto não houver usuário real | Vale apagar e regerar migrations e rodar `db:fresh` à vontade em `soromaps_dev` |
| Depois do primeiro usuário real | Migration é só adição; `db:fresh` nunca mais toca `soromaps` (e o comando já recusa) |
| Aplicar em produção | `dotnet ef migrations script --idempotent` revisado → `dotnet ef database update` com `Database__Schema=soromaps` |

---

## 🌱 Seed

Equivalente ao `migrate:fresh --seed` do Laravel.

| Laravel | Aqui |
|---|---|
| `php artisan migrate:fresh --seed` | `dotnet run -- db:fresh --seed` |
| `DatabaseSeeder` | `Data/Seed/DatabaseSeeder.cs` |
| Insert fixo dentro da migration | `HasData` |
| Factories | JSON exportado dos mocks do front |

### 1. Exportar os mocks (no front)

- [ ] `soromaps_web/scripts/export-seed.ts`, rodado com `npx tsx scripts/export-seed.ts`
- [ ] Importa `src/mocks/*`, traduz para o formato da tabela (inglês, sem agregados) e grava `soromaps_api/Data/Seed/*.json`
- [ ] **O JSON é versionado na API**, que não depende do repo do front para semear
- [ ] Rodar de novo sempre que um mock mudar

```ts
// esboço — nomes de export dos mocks a conferir na implementação
import { writeFileSync, mkdirSync } from "node:fs";
import { categoriesMock } from "@/mocks/admin-categories";

const outputDirectory = "../soromaps_api/Data/Seed";
mkdirSync(outputDirectory, { recursive: true });

function write(name: string, rows: unknown[]) {
  writeFileSync(`${outputDirectory}/${name}.json`, JSON.stringify(rows, null, 2));
}

write("categories", categoriesMock.map((category, index) => ({
  id: index + 1,
  name: category.nome,
  slug: category.slug,
  icon: category.icone,
  color: category.cor,
  order: index + 1,
})));
```

A conferir ao implementar:
- mock que importa componente ou ícone React em vez de string (o `tsx` não roda; separar dado de apresentação);
- `tsx` resolvendo o alias `@/` do `tsconfig`;
- forma exata de cada mock.

### 2. Catálogo fixo — `HasData` (dev e prod)

- [ ] `categories`, `tags`, `achievements` lidos dos JSON por um helper `SeedFile.Read<T>()`
- [ ] Identity começando em 1000, porque id explícito não avança a sequence do Postgres

```csharp
public void Configure(EntityTypeBuilder<Category> builder)
{
    builder.Property(category => category.Id).HasIdentityOptions(startValue: 1000);
    builder.HasData(SeedFile.Read<Category>("categories.json"));
}
```

Mudar o JSON e gerar migration produz `UpdateData`/`DeleteData`: o catálogo fica
versionado como schema.

### 3. `DatabaseSeeder`

- [ ] `SeedAdminAsync()` — qualquer schema; só cria se não existe `role = admin` (D8)
- [ ] Dado de demonstração — **só se `Database:Schema == soromaps_dev`**, idempotente
- [ ] `ReviewGenerator.MatchAverages` — avaliações determinísticas (sem `Random` solto) cuja média e total batem com o mock; a nota nunca é gravada

```csharp
public class DatabaseSeeder(AppDbContext context, IConfiguration configuration)
{
    public async Task SeedAsync()
    {
        await SeedAdminAsync();

        if (configuration["Database:Schema"] != "soromaps_dev")
            return;

        if (await context.Places.AnyAsync())
            return;

        var users = SeedFile.Read<User>("users.json");
        var places = SeedFile.Read<Place>("places.json");
        context.Users.AddRange(users);
        context.Places.AddRange(places);
        context.Reviews.AddRange(ReviewGenerator.MatchAverages(places, users));
        await context.SaveChangesAsync();
    }

    private async Task SeedAdminAsync()
    {
        if (await context.Users.AnyAsync(user => user.Role == UserRole.Admin))
            return;

        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        context.Users.Add(new User
        {
            Name = "admin",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            Role = UserRole.Admin,
        });
        await context.SaveChangesAsync();
    }
}
```

> As fases 02+ adicionam ao seeder os dados de cada tabela nova, na mesma
> entrega da migration.

### 4. Comando `db:fresh`

- [ ] Tratado em `Program.cs`, depois de `builder.Build()` e antes de `app.Run()`
- [ ] Ordem: **recusa se o schema não for `soromaps_dev`** → `DROP SCHEMA soromaps_dev CASCADE` → `MigrateAsync()` → `SeedAsync()` se veio `--seed`
- [ ] Nunca `dotnet ef database drop`: o banco `postgres` do Supabase contém `storage`, `auth` e `realtime`

```csharp
if (args.Contains("db:fresh"))
{
    await DatabaseCommands.FreshAsync(app.Services, seed: args.Contains("--seed"));
    return;
}
```

---

## ⚙️ Configuração (DotNetEnv)

- [ ] Pacote `DotNetEnv`; `DotNetEnv.Env.NoClobber().TraversePath().Load();` na primeira linha do `Program.cs`
- [ ] `.env.example` versionado com as chaves abaixo, sem valores
- [ ] `appsettings.json` só com o que não é segredo
- [ ] Mesmas chaves nas Application Settings da Azure

| Chave | Uso | Fase |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Session pooler do Supabase | 00 |
| `Database__Schema` | `soromaps_dev` ou `soromaps` | 00 |
| `Seed__AdminEmail` / `Seed__AdminPassword` | Primeiro admin | 00 |
| `Auth__PrivateKey` | Chave privada ES256 | 01 |
| `Supabase__Url` / `Supabase__ServiceKey` | Storage das fotos | 03 |

---

## 📨 Contrato de resposta

- [ ] `AddProblemDetails` + `UseExceptionHandler`; sem stack trace em produção
- [ ] Extensões `code` (estável, ex.: `user.email_taken`) e `errors` por campo
- [ ] Violação de `UNIQUE` → `409`: capturar `PostgresException` com `SqlState == "23505"` e usar o nome da constraint para o campo
- [ ] **DTO de saída em todo endpoint.** Resolve o vazamento de `user_password` e a divergência atual `userEmail` (login, objeto anônimo) × `email` (cadastro, entidade), que obriga `src/actions/auth.ts` a ler campos diferentes
- [ ] `JsonStringEnumConverter` com nomes em minúsculo; `[JsonStringEnumMemberName("...")]` para valores com hífen
- [ ] No banco, enum como `varchar` com `CHECK` (`HasConversion<string>()`)

**Por que enum como string:** o .NET serializa enum como número por padrão
(`"status": 2`). O front espera literal (`"approved"`), teria de manter uma
tabela `2 → "approved"` duplicada e, se alguém reordenar o enum, **todo
`approved` vira outro status sem erro nenhum** — no JSON e no banco.

Tabela de status em [D9](./decisoes.md#d9--erro-em-problemdetails-com-code-texto-no-front).

---

## 🖥️ Contrapartida no front

- [ ] `ApiErrorBody` (`src/http/*`) espelha o ProblemDetails
- [ ] Dicionário `code → mensagem pt-BR` com mensagem padrão de fallback
- [ ] `loginAction` e `registerAction` param de exibir `response.text()`
- [ ] Erros de `errors` aplicados com `setError` nos formulários

---

## 🧹 Limpeza

- [ ] Remover `WeatherForecast.cs` e `Controllers/WeatherForecastController.cs` — boilerplate de `dotnet new webapi --use-controllers`, público em produção sem motivo
- [ ] `Soromaps.http`: tirar o `GET /weatherforecast/` do template e adicionar os endpoints reais
- [ ] Remover `AddCors`/`UseCors` do `Program.cs` (D11) — **só depois** da `/api/proxy` existir no Next

---

## ✅ Verificação

- [ ] `dotnet run -- db:fresh --seed` com `Database__Schema=soromaps_dev` sobe sem erro; rodar duas vezes dá o mesmo resultado
- [ ] O mesmo comando com `Database__Schema=soromaps` **recusa**
- [ ] Os schemas `storage` e `auth` do Supabase continuam intactos após o fresh
- [ ] Clonar o repo, copiar `.env.example` para `.env`, preencher e rodar: a API sobe
- [ ] Um `INSERT` pela API numa tabela com `HasData` não colide de id
- [ ] Erro de validação chega como ProblemDetails com `errors`; e-mail duplicado chega como `409` com `code`
- [ ] No front, forçar um erro mostra mensagem pt-BR, nunca o corpo cru
