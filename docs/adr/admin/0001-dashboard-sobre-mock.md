# Dashboard admin construído sobre mock, como hub de ação

> Módulo: admin/dashboard · Rota: `/admin/dashboard` · Data: 2026-08-09

## O que foi implementado

Tela inteira em `src/app/(app)/admin/dashboard/`, seguindo o Figma. Quatro
blocos, todos lendo de `src/mocks/admin-dashboard.ts` — nenhuma chamada à API:

| Bloco | Componente |
|---|---|
| Necessita de atenção | `_components/attention-queues.tsx` |
| Números (4 cards) | `place-segmentation-card`, `new-places-card`, `metric-card` |
| Gráfico (cadastros/semana) | `_components/signups-chart.tsx` |
| Qualidade de dados | `_components/data-quality-chart.tsx` |

## Decisões

- **Deliberadamente um hub, não um BI.** Números que orientam ação, cada card
  linkando pra tela onde a ação acontece — não uma tela de métrica pra
  contemplar.
- **Gráficos com Recharts** via `src/components/ui/chart.tsx` do shadcn —
  entrou junto com `breadcrumb`; paleta `--chart-1..5` já existia no
  `globals.css`.
- **Nada de `Math.random`/`Date.now()` no mock.** A série de 90 dias de
  qualidade é gerada por ruído determinístico (`Math.sin`) a partir de uma
  data-âncora fixa — com valor aleatório, servidor e cliente renderizariam
  números diferentes e a hidratação quebraria. Pelo mesmo motivo a espera das
  filas é `aguardandoHaDias: number`, formatado por `formatWaitingDays`, e não
  uma data relativa a `now`.
- **Barra de "lugares novos" é `div`, não `Progress`.** Mostra quatro estágios
  de moderação lado a lado; `Progress` é de valor único, então foi removido
  depois de instalado.
- **Trilha de navegação virou `src/components/blocks/page-breadcrumb.tsx`** —
  primeira tela a ter; as outras de admin devem adotar.
- Dois cards do Figma repetiam "Avaliações 254" (copy-paste do protótipo); o
  segundo virou "Novos usuários", que é o que o escopo pedia.

## Pendências conhecidas

Sai de 🟡 quando `GET /api/admin/stats` existir e os módulos irmãos
(`moderation`, `reports`, `businesses`) tiverem tabela para contar — ver
[docs/todo/admin/dashboard.md](../../todo/admin/dashboard.md).
