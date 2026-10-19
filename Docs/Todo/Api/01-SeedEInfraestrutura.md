# 🌱 01. Seed e Infraestrutura Complementar

> **Status:** 💤 Pendente · **Depende de:** Baseline (ADRs 0001–0003) · [Índice](./README.md)

A fundação da API (migrations com RLS, `ProblemDetails` com `code`, autenticação JWT + Refresh Token e arquitetura em camadas) já está implementada. Este documento registra os itens de infraestrutura que faltam para alimentar o banco de desenvolvimento/produção e fechar a superfície de rede.

---

## 🗄️ 1. Seeder do Banco de Dados (`DatabaseSeeder`)

O banco precisa nascer com um administrador garantido em qualquer ambiente e, em ambiente de desenvolvimento, com massa de dados equivalente aos mocks do front-end para permitir testes reais fim a fim.

### Tarefas
- [ ] Criar `Options/SeedOptions.cs` (`SectionName = "Seed"`) contendo `AdminEmail`, `AdminPassword` e `AdminName` (default `"Administrador"`).
- [ ] Criar `Data/Seed/DatabaseSeeder.cs`:
  - `SeedAdminAsync()`: verifica se existe algum registro em `tb_users` com `Role == UserRole.Admin`. Se não existir e `Seed:AdminEmail` + `Seed:AdminPassword` estiverem configurados nos User Secrets / variáveis de ambiente, cria o usuário administrador com `BCrypt` (custo 12).
  - `SeedDevelopmentDataAsync()`: executado apenas em ambiente `Development` quando as tabelas estiverem vazias (idempotente). Popula categorias iniciais, conquistas iniciais, usuários exploradores de demonstração, locais aprovados/pendentes e avaliações baseadas nos JSONs de seed em `Data/Seed/*.json`.
- [ ] Adicionar suporte ao argumento de linha de comando `--seed` no `Program.cs` (ou execução controlada via CLI) para disparar o `DatabaseSeeder` sem acoplar ao startup normal de cada requisição.

---

## 🔒 2. Fechamento de CORS pós-Proxy

Atualmente o `Program.cs` mantém a política `FrontendClient` (`http://localhost:3000`) para chamadas diretas do navegador.

### Tarefas
- [ ] Quando o front-end centralizar todas as chamadas client-side na rota `/api/proxy/[...path]` do Next.js (comunicação servidor-para-servidor), remover ou restringir `AddCors` / `UseCors` em produção para que a API não exponha cabeçalhos CORS desnecessários a origens externas.

---

## ✅ Verificação

- [ ] Rodar o seed duas vezes seguidas não duplica o usuário admin nem os registros de demonstração.
- [ ] O admin criado pelo seed consegue autenticar em `POST /api/auth/login` e acessar rotas com `[Authorize(Policy = "AdminOnly")]`.
