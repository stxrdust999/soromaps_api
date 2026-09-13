# 🔔 Notificações

> Área: usuário público · Rota: `/notifications` · Status: 💤 não iniciado

## Ideia

Central de notificações in-app: sino na sidebar com contador de não lidas, página com a lista. Tipos de lançamento: **responderam seu comentário**, **novo seguidor**, **conquista obtida**, **lugar que você favoritou recebeu avaliação**.

Começa 100% in-app (tabela + polling leve ou Supabase Realtime). Web Push é fase 2 — exige service worker, permissão do usuário e vira naturalmente parte do pacote PWA.

## Por que vale

- É o tecido conjuntivo dos outros módulos: feed, comentários, seguidores e favoritos só geram re-engajamento se avisarem o usuário.
- Sem notificação, comentário-com-resposta (RF-09) é conversa que ninguém descobre que aconteceu.

## Dependências

| O quê | Situação |
|---|---|
| Entidade nova `Notificacao` (`id`, `usuarioID`, `tipo`, `payload JSON`, `lida`, `created_at`) | ❌ — **não estava no modelo do TCC**, é adição nossa |
| Módulos que geram eventos (`Comentario`, `Segue`, `GanhaConquista`, `Favorita`) | ❌ |
| Autenticação na API | 🔴 pré-requisito |

A decisão de design que importa: `payload` em JSON por tipo (padrão que o time já usou no HireFlow) — adicionar tipo novo de notificação não exige migration.

## Escopo inicial

- Criação da notificação dentro da própria action que gera o evento (síncrono; fila só se doer)
- Sino com contador + dropdown das 5 últimas
- `/notifications` com lista completa, marcar como lida individual e em massa

## Fora do escopo inicial

- Web Push (fase 2, junto com PWA)
- Preferências por tipo ("não me avise sobre X")
- E-mail
