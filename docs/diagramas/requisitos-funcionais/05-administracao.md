# 05. Administração

Rotas: `/admin/dashboard`, `/admin/moderation`, `/admin/reports`,
`/admin/reviews`, `/admin/categories`, `/admin/achievements`, `/admin/users`.

Sete telas 🟡 sobre mock e uma 🟢 sobre a API. **Nenhuma delas checa papel.**

## Dashboard

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-ADM-01 | Exibir filas de atenção | O que está esperando decisão aparece antes de qualquer gráfico | RF-10 | 🟡 |
| RF-ADM-02 | Exibir os quatro cards de número | Contadores do estado da plataforma | RF-10 | 🟡 |
| RF-ADM-03 | Exibir cadastros e qualidade do dado | Dois gráficos Recharts sobre série temporal determinística | RF-10 | 🟡 |
| RF-ADM-04 | Consumir um agregado único | `GET /api/admin/stats` em vez de N chamadas de lista só para contar | RF-10 | 🔴 |

## Moderação de pontos

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-ADM-05 | Trabalhar a fila em mestre-detalhe | Selecionar na fila troca o detalhe sem perder a posição | moderação | 🟡 |
| RF-ADM-06 | Revisar a ficha do ponto com contexto | Campos enviados, autor e bairro no mini-mapa, na mesma tela da decisão | moderação | 🟡 |
| RF-ADM-07 | Aprovar ponto | O item sai da fila e entra no histórico | moderação | 🟡 |
| RF-ADM-08 | Rejeitar com motivo obrigatório | Rejeição sem motivo não é aceita | moderação | 🟡 |
| RF-ADM-09 | Comparar duplicata | Diálogo lado a lado com o ponto já existente | moderação | 🟡 |
| RF-ADM-10 | Decidir em lote | Seleção múltipla aplica a mesma decisão | moderação | 🟡 |
| RF-ADM-11 | Operar por teclado | Atalhos percorrem a fila e disparam as decisões | — | 🟡 |
| RF-ADM-12 | Consultar histórico de decisões | Aba com o que já foi decidido, por quem e quando | moderação | 🟡 |
| RF-ADM-13 | Ter estado de moderação no ponto | Coluna `status` em `markers` e tabela de decisão | moderação | 🔴 |

## Denúncias e feedback

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-ADM-14 | Agrupar denúncias por alvo | Várias denúncias do mesmo conteúdo viram um item de fila | moderação | 🟡 |
| RF-ADM-15 | Sinalizar denúncia coordenada | Selo quando o padrão indica ataque combinado | moderação | 🟡 |
| RF-ADM-16 | Renderizar o conteúdo denunciado por tipo | Avaliação, comentário e ponto aparecem cada um na própria forma | moderação | 🟡 |
| RF-ADM-17 | Remover conteúdo com motivo | Catálogo único de motivos, o mesmo usado em `/admin/reviews` | moderação | 🟡 |
| RF-ADM-18 | Triar feedback de usuário | Aba própria, separada da fila de denúncia | — | 🟡 |

## Avaliações

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-ADM-19 | Exibir os quatro indicadores da base | Volume, nota média e o que exige atenção | RF-07, RF-10 | 🟡 |
| RF-ADM-20 | Marcar sinais formais | Spam, duplicada e discrepante, calculados, não digitados | RF-07 | 🟡 |
| RF-ADM-21 | Expandir a linha com os comentários | Sem sair da listagem | RF-09 | 🟡 |
| RF-ADM-22 | Pivotar por autor ou por local | Da avaliação para tudo daquele autor ou daquele ponto | RF-07 | 🟡 |
| RF-ADM-23 | Remover em lote | Seleção múltipla com um motivo só | moderação | 🟡 |

## Catálogos

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-ADM-24 | Manter categorias | Criar, editar e excluir, com pin renderizado e preview ao vivo | RF-12 | 🟡 |
| RF-ADM-25 | Alertar colisão de cor | Cor já usada por outra categoria avisa antes de salvar | RF-12 | 🟡 |
| RF-ADM-26 | Reatribuir na exclusão | Excluir categoria exige escolher para onde vão os pontos dela | RF-12 | 🟡 |
| RF-ADM-27 | Manter conquistas | Catálogo com construtor de critério declarativo | — | 🟡 |
| RF-ADM-28 | Estimar alcance do critério | Quantas pessoas desbloqueariam com a regra atual | — | 🟡 |
| RF-ADM-29 | Prever o desbloqueio | Prévia de como a conquista aparece a quem a recebe | — | 🟡 |
| RF-ADM-30 | Desativar conquista sem apagar | Sai de circulação mantendo quem já ganhou | — | 🟡 |
| RF-ADM-31 | Calibrar a faixa de badges | Aba própria para ajustar a régua | — | 🟡 |

## Usuários

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-ADM-32 | Listar usuários | Dado real da API, com ordenação, busca por coluna, sheet de filtro, visibilidade de coluna, seleção e paginação | moderação | 🟢 |
| RF-ADM-33 | Criar usuário | Modal em rota interceptada, validado com Zod, cache invalidado ao fechar | RF-01 | 🟢 |
| RF-ADM-34 | Editar usuário | Idem, por `PUT /api/users/{id}` | RF-02 | 🟢 |
| RF-ADM-35 | Excluir usuário | Confirmação em modal de rota, `DELETE /api/users/{id}` | moderação | 🟢 |
| RF-ADM-36 | Editar sem reenviar a senha | Atualização parcial na API | RF-02 | 🔴 |

## Transversal

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-ADM-37 | Restringir `/admin` a administrador | Sessão comum recebe 403 ou redirecionamento, no middleware **e** na API | — | 🔴 |
| RF-ADM-38 | Reusar a biblioteca de listagem | Nenhuma tela chama `useReactTable` direto; comportamento novo entra no hook global | — | 🟢 |

## Evidência

| ID | Onde |
|---|---|
| RF-ADM-01..04 | `admin/dashboard/_components/`, `src/mocks/admin-dashboard.ts` |
| RF-ADM-05..12 | `admin/moderation/_components/`, `use-moderation-queue.ts` |
| RF-ADM-14..18 | `admin/reports/_components/`, `use-reports.ts` |
| RF-ADM-19..23 | `admin/reviews/_components/`, `use-reviews.ts` |
| RF-ADM-17, 23 | `src/components/blocks/removal-dialog.tsx` + `src/constants/content-removal.ts` |
| RF-ADM-24..26 | `admin/categories/_components/`, `use-categories.ts` |
| RF-ADM-27..31 | `admin/achievements/_components/`, `AchievementBadge` adaptado do Trophy UI Kit |
| RF-ADM-32..35 | `admin/users/`, `(app)/@modals/admin/(.)users/`, `src/http/users/`, `src/actions/users.ts` |
| RF-ADM-38 | `useTableConfig` + `src/components/table/` |

## Notas

**RF-ADM-17 já é um requisito só, antes da API.** `/admin/reviews` e
`/admin/reports` removiam o mesmo conteúdo por caminhos diferentes, e duas
implementações divergiriam no primeiro ajuste de regra — por isso `RemovalDialog`
e `REMOVAL_REASONS` são compartilhados. Quando a Server Action nascer, as duas
telas já falam igual.

**Três formas de tela para três formas de trabalho:** fila mestre-detalhe onde a
decisão *é* a tela (moderação), listagem simples onde o CRUD é o trabalho
(categorias), e listagem com painel lateral onde se despacha vários itens sem
perder a posição na tabela.

**RF-ADM-37 é o requisito que destrava outros dois** — `RF-PTO-06` (gate de
editar e excluir ponto) e `RF-COM-12` (gate do gerador de pauta) dependem do
mesmo papel.

**O mini-mapa da moderação é SVG, não MapLibre**, e a série do dashboard é ruído
determinístico sobre data-âncora fixa: `Math.random()` ou `Date.now()` em mock
renderiza diferente no servidor e no cliente e quebra a hidratação.

---

➡️ [06 — Rastreabilidade](./06-rastreabilidade.md)
