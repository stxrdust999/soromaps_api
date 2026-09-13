# 05. Catálogo de casos

Um caso por linha, com ator, rota, relação com o requisito do TCC e o estado do
dado. ✅ = passa pela API. 🟡 = a tela funciona sobre `src/mocks/*`.

## Sessão

| Caso | Ator | Rota | RF | Dado |
|---|---|---|---|---|
| Cadastrar conta | Visitante | `/register` | RF-01 | ✅ |
| Entrar | Visitante | `/login` | RF-01 | ✅ |
| Sair | Explorador | — | RF-01 | ✅ |
| Validar sessão no middleware | — | todas | — | ✅ |

## Mapa e ponto

| Caso | Ator | Rota | RF | Dado |
|---|---|---|---|---|
| Explorar o mapa | Explorador | `/home` | RF-03, RF-04 | ✅ |
| Carregar marcadores por zoom | — | `/home` | RF-04 | ✅ ⚠️ quebrado em produção |
| Ver rótulo no hover | Explorador | `/home` | RF-04 | ✅ |
| Abrir card do marcador | Explorador | `/home` | RF-04 | ✅ |
| Ler o painel arrastável | Explorador | `/home` | — | 🟡 |
| Cadastrar ponto | Explorador | `/places/new` | RF-05 | ✅ (3 de 8 campos) |
| Ver detalhes do ponto | Explorador | `/places/[id]` | RF-06 | 🟡 |
| Editar ponto | Administrador¹ | `/places/[id]` | RF-06 | ✅ |
| Excluir ponto | Administrador¹ | `/places/[id]` | moderação | ✅ |

## Descoberta e feed

| Caso | Ator | Rota | RF | Dado |
|---|---|---|---|---|
| Descobrir lugares | Explorador | `/discover` | RF-11 | 🟡 |
| Buscar por nome | Explorador | `/discover` | RF-11 | 🟡 |
| Filtrar por categoria / vibe | Explorador | `/discover` | RF-12 | 🟡 |
| Ver as trilhas da cidade | Explorador | `/discover` | RF-11 | 🟡 |
| Ler o feed | Explorador | `/feed` | — | 🟡 |
| Ver por que o item apareceu | Explorador | `/feed` | — | 🟡 |
| Filtrar por fonte / ordenar | Explorador | `/feed` | RF-12 | 🟡 |
| Marcar avaliação como útil | Explorador | `/feed` | RF-09 | 🟡 |
| Acompanhar lugar | Explorador | `/feed` | RF-13² | 🟡 |
| Ver menos disso | Explorador | `/feed` | — | 🟡 |

## Comunidade e perfil

| Caso | Ator | Rota | RF | Dado |
|---|---|---|---|---|
| Explorar a comunidade | Explorador | `/community` | — | 🟡 |
| Buscar exploradores | Explorador | `/community` | — | 🟡 |
| Ver perfil público | Explorador | `/community/[id]` | RF-02 | 🟡 |
| Ver ranking de contribuição | Explorador | `/community` | RF-10 | 🟡 |
| Consultar a régua do selo | Explorador | `/community`, `/profile` | — | 🟡 |
| Gerar rascunho de pauta | Administrador¹ | `/community` | — | ✅ (Gemini) |
| Ler uma pauta | Explorador | `/pautas/[slug]` | — | 🟡 |
| Ver o próprio perfil | Explorador | `/profile` | RF-02 | 🟡 (identidade ✅) |
| Ver histórico de visitas | Explorador | `/profile/visits` | RF-10 | 🟡 |
| Ver lugares salvos | Explorador | `/profile/favorites` | — | 🟡 |
| Ver galeria de conquistas | Explorador | `/profile/achievements` | — | 🟡 |
| Ver placar e cobertura | Explorador | `/profile/stats` | RF-10 | 🟡 |

## Administração

| Caso | Ator | Rota | RF | Dado |
|---|---|---|---|---|
| Acompanhar indicadores | Administrador¹ | `/admin/dashboard` | RF-10 | 🟡 |
| Moderar fila de pontos | Administrador¹ | `/admin/moderation` | moderação | 🟡 |
| Aprovar / rejeitar com motivo | Administrador¹ | `/admin/moderation` | moderação | 🟡 |
| Comparar duplicata | Administrador¹ | `/admin/moderation` | moderação | 🟡 |
| Decidir em lote | Administrador¹ | `/admin/moderation` | moderação | 🟡 |
| Tratar denúncias | Administrador¹ | `/admin/reports` | moderação | 🟡 |
| Remover conteúdo com motivo | Administrador¹ | `/admin/reports`, `/admin/reviews` | moderação | 🟡 |
| Triar feedback | Administrador¹ | `/admin/reports` | — | 🟡 |
| Revisar avaliações | Administrador¹ | `/admin/reviews` | RF-07 | 🟡 |
| Manter categorias | Administrador¹ | `/admin/categories` | RF-12 | 🟡 |
| Manter conquistas | Administrador¹ | `/admin/achievements` | — | 🟡 |
| Manter usuários | Administrador¹ | `/admin/users` | moderação | ✅ |

¹ **Ator pretendido, não checado.** Sem papel no banco, no `middleware.ts` ou
na API, qualquer sessão válida executa estes casos.
² `Acompanhar lugar` substitui `Seguir usuário` (RF-13) — o grafo social saiu
do produto em 2026-08-17.

---

## Casos do TCC que não existem no código

| Caso projetado | RF | Situação |
|---|---|---|
| Avaliar ponto | RF-07 | Falta `Analise`. As telas de admin já moderam avaliações fictícias |
| Criar comentário | RF-09 | Falta `Comentario` |
| Interagir com avaliações | RF-09 | Só "útil" no feed, sobre mock |
| Carregar foto | RF-08 | Sem upload; foto de ponto vem de `src/mocks/markers.ts` |
| Configurar perfil | RF-02 | É `/settings`, ainda sem rota — travado pelo `PUT` que re-hasheia a senha |
| Seguir / deixar de seguir usuário | RF-13 | **Cancelado** em 2026-08-17 |
| Remover perfil | moderação | Existe como `Excluir usuário` em `/admin/users` ✅ |
| Gerar estatísticas | RF-10 | Dashboard e `/profile/stats` existem sobre mock; falta `GET /api/admin/stats` |

Comparar com o catálogo projetado em
[`archive/wiki-trilha-projetada/03-casos-de-uso.md`](../../archive/wiki-trilha-projetada/03-casos-de-uso.md) e com o gap técnico
em [`wiki/09-backlog.md`](../../wiki/09-backlog.md).
