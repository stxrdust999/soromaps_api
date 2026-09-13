# 📊 Estatísticas pessoais

> Área: usuário público · Rota: `/profile/stats` (aba de [Perfil](./profile.md)) · Status: 🟡 tela pronta sobre `src/mocks/profile.ts` — falta `Visita`

## Ideia

Dashboard pessoal do explorador: lugares visitados, **% da cidade explorada** (bairros com pelo menos um check-in ÷ bairros totais), categorias favoritas, gráfico de atividade mensal e um mini-mapa com a "mancha" do que já foi coberto.

O número-âncora é o % de exploração — é a mecânica de completar mapa que jogos usam há décadas, aplicada à cidade real. Vira o gancho do "modo explorador" (fog of war) se ele sair do papel.

## Por que vale

- Fecha o ciclo da gamificação: conquista é o prêmio, estatística é o placar.
- "Wrapped" anual ("seu 2026 em Sorocaba: 43 lugares, 9 bairros") é conteúdo compartilhável de custo quase zero em cima destes mesmos dados.
- Pra apresentação do TCC, é uma tela visualmente rica que demonstra o produto inteiro funcionando junto.

## Dependências

| O quê | Situação |
|---|---|
| `Visita` (fonte de quase tudo) | ❌ não existe |
| `Analise` (contador de avaliações) | ❌ |
| Bairro no ponto (ou geocodificação reversa das coords) | ❌ — `markers` não tem endereço/bairro |
| Lib de gráficos | ❌ — avaliar Recharts (casa com shadcn) |

## Escopo inicial

- Cards de contadores + gráfico de barras mensal
- % de exploração com barra de progresso
- Lista "categorias que você mais visita"

## Fora do escopo inicial

- Mini-mapa de cobertura (camada MapLibre — fase 2)
- Wrapped anual compartilhável
- Comparação com a média dos usuários

> A rota antiga redireciona para a aba desde 2026-08-19. Decisões de
> implementação em [adr/user/0004-perfil-hub-com-abas.md](../../adr/user/0004-perfil-hub-com-abas.md)
