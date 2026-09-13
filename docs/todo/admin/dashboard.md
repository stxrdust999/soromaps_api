# 📊 Dashboard admin

> Área: admin · Rota: `/admin/dashboard` · Status: 🟡 parcial — tela pronta, dados em mock

## Ideia

Este módulo dá ao admin a visão de cima: usuários totais e novos na semana, pontos criados, avaliações, e os atalhos com contadores de pendência ("3 pontos aguardando moderação", "2 denúncias abertas").

É deliberadamente um **hub**, não um BI: números que orientam ação, cada card linkando pra tela onde a ação acontece.

## Por que vale

- Atalhos com contador transformam o admin de "lugar que se visita" em "fila de trabalho que se despacha".
- Métricas de crescimento são a primeira coisa que a banca (e um investidor) pergunta.
- Barato: são `COUNT`s sobre tabelas que já existem ou vão existir.

## Dependências

| O quê | Situação |
|---|---|
| Contadores de `tbUsuario` e `markers` | 🟢 dá pra fazer hoje |
| Endpoint agregado (`GET /api/admin/stats`) | ❌ — evitar N chamadas de lista só pra contar |
| Papel/role de admin checado na API | 🔴 pré-requisito — hoje qualquer sessão acessa `/admin` |
| Contadores de moderação/denúncias | ❌ dependem dos módulos irmãos |

## Escopo inicial

- Cards: usuários, pontos, novos na semana (com variação)
- Atalhos com contador para os outros módulos admin
- Gráfico simples de cadastros por semana

## Fora do escopo inicial

- Filtros de período customizados
- Exportar relatórios

> Decisões já tomadas e componentes existentes em
> [adr/admin/0001-dashboard-sobre-mock.md](../../adr/admin/0001-dashboard-sobre-mock.md)
