# 🗺️ TODO da API — Expansão para o Produto Completo

> **Estado atual da API (Baseline entregue):**
> - **Arquitetura:** Camadas (`Controllers`, `Services`, `DTOs`, `Mappings`, `Models`, `Data`, `Results`, `Errors`) documentadas nos ADRs [0001](../../ArchitectureDecisionRecords/0001-FundacaoEModuloUsuarios.md), [0002](../../ArchitectureDecisionRecords/0002-ModuloAutenticacaoESessoes.md) e [0003](../../ArchitectureDecisionRecords/0003-ArquiteturaEmCamadas.md).
> - **Banco de Dados:** 7 tabelas iniciais (`tb_users`, `tb_sessions`, `tb_categories`, `tb_places`, `tb_reviews`, `tb_comments`, `tb_achievements`) com `Guid` (`gen_random_uuid()`), `snake_case`, prefixo `tb_` e Row Level Security (RLS) automático via `RlsMigrationsSqlGenerator`.
> - **Contrato:** Respostas de erro em `ProblemDetails` com `extensions.code`, enums serializados como string (`JsonStringEnumConverter`) e OpenAPI v3 (`/openapi/v1.json`) pronto para geração de cliente no front-end via Orval.

Este diretório organiza **tudo o que o front-end (`soromaps_web`) exige e que a API ainda não provê** (ou provê apenas parcialmente na versão enxuta inicial).

---

## 📋 Módulos Pendentes na API

| # | Módulo | Arquivo | Status | Resumo do que falta na API |
|---|---|---|---|---|
| 01 | **Seed e Infraestrutura** | [01-SeedEInfraestrutura.md](./01-SeedEInfraestrutura.md) | ✅ | `DatabaseSeeder` (`--seed`), `SupabaseStorageService` (`soromaps_places` / `soromaps_pfps`) e documentação de segredos concluídos. |
| 02 | **Administração de Usuários** | [02-UsuariosAdmin.md](./02-UsuariosAdmin.md) | ✅ | `PATCH /api/users/{id}`, `DELETE /api/users/{id}`, filtros `role`/`search`, upload de avatar e trava `user.last_admin` entregues. |
| 03 | **Catálogo, Categorias e Locais** | [03-CatalogoELocais.md](./03-CatalogoELocais.md) | ✅ | `tb_categories` enriquecida, `tb_places` + `tb_place_photos` + `tb_tags` + `tb_place_tags`, médias, filtros `bbox`/`search`/`tag` e upload assinado entregues. |
| 04 | **Moderação de Locais** | [04-ModeracaoDeLocais.md](./04-ModeracaoDeLocais.md) | ✅ | `tb_moderation_decisions`, fila `/api/admin/moderation/places`, decisão individual e em lote, desfazer 24h e detecção Haversine de duplicatas entregues. |
| 05 | **Avaliações, Comentários e Moderação** | [05-AvaliacoesEComentarios.md](./05-AvaliacoesEComentarios.md) | 💤 | Votos de utilidade (`tb_review_useful_votes`), respostas de 1 nível em comentários (`reply_to_id`), listagem global `/api/admin/reviews` com sinais automáticos e remoção auditada (`tb_content_removals`). |
| 06 | **Atividade do Explorador** | [06-AtividadeDoExplorador.md](./06-AtividadeDoExplorador.md) | 💤 | Check-in de visitas com validação Haversine (`tb_visits`), coleção de favoritos (`tb_favorites`), estatísticas do perfil (`/api/users/me/stats`) e régua de verificação (`/api/users/me/verification`). |
| 07 | **Motor de Gamificação** | [07-Gamificacao.md](./07-Gamificacao.md) | 💤 | Regras estruturadas em `tb_achievements` (`event`, `quantity`, `target_type`, `target`, `icon`, `color`, `is_active`), tabela `tb_user_achievements`, avaliador idempotente e estimativa de alcance para o admin. |
| 08 | **Denúncias e Feedback** | [08-DenunciasEFeedback.md](./08-DenunciasEFeedback.md) | 💤 | Tabela polimórfica `tb_reports` agrupada por alvo para `/admin/reports` e tabela `tb_feedback` para envio de sugestões/bugs. |
| 09 | **Comunidade, Feed e Pautas** | [09-ComunidadeFeedEPautas.md](./09-ComunidadeFeedEPautas.md) | 💤 | Ranking público de exploradores (`/api/community/explorers`), consulta do feed sem grafo social (`/api/feed` + `tb_feed_mutes`) e pautas editoriais/IA (`tb_stories` + `tb_story_places`). |
| 10 | **Dashboard Administrativo** | [10-DashboardAdmin.md](./10-DashboardAdmin.md) | 💤 | Endpoint agregado `GET /api/admin/stats` (filas de atenção, KPIs de 7/30/90 dias, séries diárias e distribuição por categoria). |

---

## 🧭 Convenções para Implementação na API

1. **Padrão de Tabelas e Colunas:**
   - Tabelas no plural em `snake_case` com prefixo `tb_` (configurado automaticamente no `AppDbContext` + `UseSnakeCaseNamingConvention()`).
   - Chaves primárias `Guid` geradas pelo PostgreSQL (`DEFAULT gen_random_uuid()`), salvo tabelas associativas puras de chave composta.
   - Row Level Security (RLS) ativado automaticamente na geração da migration via `RlsMigrationsSqlGenerator`.
2. **Padrão de Camadas (ADR 0003):**
   - **Controllers:** Apenas transporte HTTP, delegação ao `Service` e retorno via `.ToActionResult(this)` com atributos OpenAPI (`[EndpointName]`, `[EndpointSummary]`, `[ProducesResponseType]`).
   - **Services:** Regras de negócio retornando `Result` ou `Result<T>` utilizando `DomainErrors`.
   - **DTOs e Mappings:** `record` imutáveis em `DTOs/` com validação por DataAnnotations e métodos de extensão `.ToDto()` em `Mappings/`.
3. **Sem dados desnormalizados de contagem/média:**
   - Nota média, total de avaliações, contagem de locais por categoria e progresso de conquistas são sempre calculados em consulta (`GROUP BY` / projeção EF Core), evitando dessincronização.
