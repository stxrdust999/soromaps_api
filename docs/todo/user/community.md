# 👥 Comunidade

> Área: usuário público · Rotas: `/community`, `/community/[id]`, `/pautas/[slug]` · Status: 🟡 parcial — telas prontas, dados em mock

## Ideia

O diretório da cidade, **sem seguir ninguém** — consequência da mesma decisão
que tirou `Segue` do [Feed](./feed.md). Três partes:

1. **Busca e perfil público** — quem é a pessoa que avaliou, medido pelo que
   ela registrou: visitas, avaliações, pontos cadastrados, conquistas.
2. **Ranking de contribuição**, geral e **por bairro**.
3. **Pautas da cidade** — texto editorial ancorado em lugares do mapa,
   redigido por IA e publicado só depois de revisão humana.

## Por que vale

- Perfil público com histórico dá contexto à avaliação ("quem é essa pessoa que
  avaliou?") — a dor nº 2, confiança. Sem seguidores, é o histórico que faz
  esse papel.
- Ranking ataca a dor nº 6 (reconhecimento). O recorte por bairro é o ponto: em
  ranking geral só os dez primeiros existem; por bairro, quase todo explorador
  ativo é destaque de alguma coisa. Reconhecimento distribuído é retenção
  distribuída.
- O **selo de explorador verificado** é o que faz "quem avaliou" virar
  argumento de confiança — a página do ponto já reserva espaço para ele.
- A pauta é o conteúdo que **não depende de terceiros**: em base nova, há
  semana em que ninguém avalia nada.

## Dependências

| O quê | Situação |
|---|---|
| Busca de usuários | 🟢 `GET /api/users` existe (falta filtro por nome no server) |
| `Analise`/`Visita` (contadores, ranking e critério do selo) | ❌ não existem |
| `GanhaConquista` (título do explorador) | ❌ |
| Entidade de pauta (`slug`, `status`, `origem`, corpo) | ❌ nem modelada |
| `GEMINI_API_KEY` no ambiente do servidor | 🟢 opcional — sem ela o gerador avisa que está desligado |
| Privacidade: API não devolver e-mail/hash em perfil público | 🔴 DTO de saída — backlog de segurança |
| `Segue` | 🚫 fora de escopo por decisão |

## Escopo restante

- Persistir a pauta e criar a fila de revisão em `/admin` — hoje o rascunho
  volta para a tela e é copiado à mão
- Ranking sobre `Analise`/`Visita` reais, com corte de janela (semana/mês)
- Busca e paginação no servidor, no lugar do recorte de array no cliente
- Selo como coluna derivada em `tbUsuario`, para não recalcular a cada leitura

## Fora do escopo

- Seguir usuário, seguidores, perfil privado, bloqueio — não vão existir
- Mensagem direta entre usuários
- Ranking com pesos (avaliação vale mais que visita etc.)
- Geração automática de pauta em lote, sem pedido humano

> Decisões já tomadas e componentes existentes em
> [adr/user/0003-comunidade-selo-e-pauta-ia.md](../../adr/user/0003-comunidade-selo-e-pauta-ia.md)

## Fronteira com as telas vizinhas

- **[Feed](./feed.md) (`/feed`)**: as pautas aparecem lá como card de curadoria
  e aqui como vitrine; as duas levam à mesma rota `/pautas/[slug]`.
- **[Conquistas](./achievements.md) (`/achievements`)**: o título do explorador
  sai da contagem de conquistas, então as duas telas leem o mesmo número por
  `src/constants/explorer-titles.ts`.
