# ⭐ Favoritos

> Área: usuário público · Rota: `/profile/favorites` (aba de [Perfil](./profile.md)) · Status: 🟡 tela pronta sobre `src/mocks/profile.ts` — falta `Favorita`

## Ideia

Lista pessoal de pontos salvos. Cada item mostra nome, categoria, média de estrelas e distância atual, com ação "ver no mapa" (abre `/home` centrado no ponto) e "remover". Favoritar acontece no popup do marcador e na página do ponto — esta tela é onde a coleção vira útil.

Favoritar é **estado**, não evento (decisão de modelagem do TCC: `Favorita` sem `data` na PK) — a tela reflete isso: sem timeline, só a coleção atual.

## Por que vale

- É o recurso de retenção mais barato que existe: dá motivo pro usuário voltar ("aquele bar que salvei").
- Alimenta o funil das outras features — notificação "lugar que você favoritou recebeu avaliação nova" só existe se favorito existir.
- A evolução natural (fase 2) são **listas nomeadas compartilháveis** ("Cafés pra trabalhar"), que viram conteúdo compartilhável fora do app.

## Dependências

| O quê | Situação |
|---|---|
| Tabela `Favorita` (`PontoID`, `UsuarioID`, `data`) | ❌ não existe — já modelada no TCC |
| Endpoints `GET/POST/DELETE /api/users/{id}/favorites` | ❌ |
| Autenticação na API | 🔴 pré-requisito — favorito é dado por usuário; sem auth qualquer um lê/escreve o de todos |

## Escopo inicial

- Botão de favoritar no popup do marcador (coração, otimista)
- `/favorites` com a lista + estado vazio bem resolvido ("explore o mapa e salve seus lugares")
- Server action `toggleFavoriteAction` com `updateTag`

## Fora do escopo inicial

- Listas múltiplas nomeadas e compartilháveis (fase 2)
- Ordenação por distância em tempo real

> A rota antiga redireciona para a aba desde 2026-08-19. Decisões de
> implementação em [adr/user/0004-perfil-hub-com-abas.md](../../adr/user/0004-perfil-hub-com-abas.md)
